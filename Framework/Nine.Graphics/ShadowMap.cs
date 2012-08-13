namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.PostEffects;

    /// <summary>
    /// Represents a shadow drawing technique using shadow map.
    /// </summary>
    [ContentSerializable]
    public class ShadowMap : Pass, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphics; }            
        }

        /// <summary>
        /// Gets the parent light that uses this shadow map.
        /// </summary>
        public Light Light 
        {
            get { return light; } 
            internal set { light = value; } 
        }
        private Light light;

        /// <summary>
        /// Gets or sets the size of the shadow map texture.
        /// </summary>
        public int Size
        {
            get { return size; }
            set { size = value; }
        }
        private int size = 512;

        /// <summary>
        /// Gets or sets the preferred surface format of the shadow map texture.
        /// The default value is SurfaceFormat.Single.
        /// </summary>
        public SurfaceFormat SurfaceFormat { get; set; }

        /// <summary>
        /// Gets the underlying shadow map texture.
        /// </summary>
        public Texture2D Texture { get; private set; }
        
        /// <summary>
        /// Gets whether a blurring pass is applied to the final shadow map.
        /// </summary>
        public bool BlurEnabled { get; set; }

        /// <summary>
        /// Gets or sets the blur amount.
        /// </summary>
        public float BlurAmount
        {
            get { return blur.BlurAmount; }
            set { blur.BlurAmount = value; }
        }
        #endregion

        #region Fields
        private bool hasBegin;
        private BlurMaterial blur;
        private GraphicsDevice graphics;
        private DepthMaterial depthMaterial;
        private RenderTarget2D renderTarget;
        private RenderTarget2D depthBlur;
        private FullScreenQuad fullScreenQuad;
        private FastList<IDrawableObject> shadowCasters = new FastList<IDrawableObject>();
        private ISpatialQuery<IDrawableObject> shadowCasterQuery;
        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowMap"/> class.
        /// </summary>
        public ShadowMap(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            
            this.graphics = graphics;
            this.depthMaterial = new DepthMaterial(graphics);
            this.blur = new BlurMaterial(graphics);
            this.fullScreenQuad = new FullScreenQuad(graphics);

#if SILVERLIGHT
            this.SurfaceFormat = SurfaceFormat.Color;
#else
            this.SurfaceFormat = SurfaceFormat.Single;
#endif
        }

        /// <summary>
        /// Gets all the passes that are going to be rendered.
        /// </summary>
        public override void GetActivePasses(IList<Pass> result)
        {
            if (enabled && light != null && light.enabled && light.castShadow)
                result.Add(this);
        }
        
        /// <summary>
        /// Begins the shadowmap generation process and clears the shadowmap to white.
        /// </summary>
        private void Begin()
        {
            if (hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            hasBegin = true;
            if (renderTarget == null || renderTarget.IsDisposed ||
#if !SILVERLIGHT
                                        renderTarget.IsContentLost ||
#endif
                renderTarget.Format != SurfaceFormat || renderTarget.Width != Size)
            {
                if (renderTarget != null)
                    renderTarget.Dispose();
                renderTarget = new RenderTarget2D(graphics, Size, Size, false, SurfaceFormat,
                                                  graphics.PresentationParameters.DepthStencilFormat);
            }
            renderTarget.Begin();
            graphics.Clear(Color.White);
        }

        /// <summary>
        /// Ends the shadowmap generation process and returns the result shadowmap texture.
        /// </summary>
        private Texture2D End(DrawingContext context)
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            hasBegin = false;
            Texture2D map = renderTarget.End();

            if (BlurEnabled)
            {
                if (depthBlur == null || depthBlur.IsDisposed ||
#if !SILVERLIGHT
                                         depthBlur.IsContentLost ||
#endif
                    depthBlur.Format != SurfaceFormat || depthBlur.Width != Size)
                {
                    if (depthBlur != null)
                        depthBlur.Dispose();
                    depthBlur = new RenderTarget2D(graphics, Size, Size, false, SurfaceFormat,
                                                   graphics.PresentationParameters.DepthStencilFormat);
                } 
                
                // Blur H
                depthBlur.Begin();
                blur.texture = map;
                blur.Direction = 0;
                fullScreenQuad.Draw(context, blur);
                map = depthBlur.End();

                // Blur V
                renderTarget.Begin();
                blur.texture = map;
                blur.Direction = MathHelper.PiOver2;
                fullScreenQuad.Draw(context, blur);
                map = renderTarget.End();
            }

            return Texture = map;
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (shadowCasterQuery == null)
            {
                shadowCasterQuery = context.CreateSpatialQuery<IDrawableObject>(drawable =>
                {
                    if (!drawable.Visible)
                        return false;
                    var lightable = drawable as ILightable;
                    return lightable != null && lightable.CastShadow;
                });
            }
            light.UpdateShadowFrustum(context, shadowCasterQuery);
            shadowCasterQuery.FindAll(light.ShadowFrustum, shadowCasters);

            Matrix oldView = context.matrices.view;
            Matrix oldProjection = context.matrices.projection;
            try
            {
                Begin();

                context.matrices.View = light.ShadowFrustum.Matrix;
                context.matrices.Projection = Matrix.Identity;

                context.graphics.SamplerStates[0] = SamplerState.PointClamp;
                context.graphics.BlendState = BlendState.Opaque;
                context.graphics.DepthStencilState = DepthStencilState.Default;

                for (int i = 0; i < shadowCasters.Count; ++i)
                {
                    var shadowCaster = shadowCasters[i];
                    var material = shadowCaster.Material;
                    if (material == null)
                        material = depthMaterial;
                    else
                        material = material.GetMaterialByUsage(MaterialUsage.Depth);

                    if (material != null)
                        shadowCaster.Draw(context, material);
                }
            }
            finally
            {
                shadowCasters.Clear();
                End(context);

                context.matrices.View = oldView;
                context.matrices.Projection = oldProjection;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (renderTarget != null)
                {
                    renderTarget.Dispose();
                    renderTarget = null;
                } 
                if (depthBlur != null)
                {
                    depthBlur.Dispose();
                    depthBlur = null;
                }
            }
        }

        ~ShadowMap()
        {
            Dispose(false);
        }
    }
}
