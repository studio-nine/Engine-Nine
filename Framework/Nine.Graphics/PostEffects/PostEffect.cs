namespace Nine.Graphics.PostEffects
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// Defines an interface for post processing effect
    /// </summary>
    public interface IPostEffect
    {
        /// <summary>
        /// Gets or sets the input texture to be processed.
        /// </summary>
        Texture2D InputTexture { get; set; }

        /// <summary>
        /// Gets the preferred surface format for the input texture.
        /// </summary>
        SurfaceFormat? InputFormat { get; }
    }

    /// <summary>
    /// Represents a post processing effect.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("Material")]
    public class PostEffect : Pass, ISceneObject, IPostEffect
    {
        #region Properties
        /// <summary>
        /// Gets or sets the material used by this post effect.
        /// </summary>
        public virtual Material Material { get; set; }

        /// <summary>
        /// Gets or sets the input texture to be processed.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D InputTexture { get; set; }

        /// <summary>
        /// Gets the preferred surface format for the input texture.
        /// </summary>
        public virtual SurfaceFormat? InputFormat
        {
            get { return null; }
        }

        /// <summary>
        /// Gets or sets the surface format of the render target.
        /// Specify null to use the surface format of the current backbuffer.
        /// </summary>
        public SurfaceFormat? SurfaceFormat
        {
            get { return surfaceFormat; }
            set { surfaceFormat = value; }
        }
        private SurfaceFormat? surfaceFormat;

        /// <summary>
        /// Gets or sets the render target size.
        /// Specify null to use current viewport size.
        /// </summary>
        public Vector2? RenderTargetSize
        {
            get { return renderTargetSize; }
            set { renderTargetSize = value; }
        }
        private Vector2? renderTargetSize;
        
        /// <summary>
        /// Gets or sets the render target scale. This value is multiplied by
        /// <c>RenderTargetSize</c> to determine the final size of the render target.
        /// </summary>
        public float RenderTargetScale
        {
            get { return renderTargetScale; }
            set { renderTargetScale = value; }
        }
        private float renderTargetScale = 1;

        internal BlendState BlendState = BlendState.Opaque;

        private Material vertexPassThrough;
        private FullScreenQuad fullScreenQuad;
        #endregion

        #region Methods
        /// <summary>
        /// Creates a new instance of PostEffect for post processing.
        /// </summary>
        public PostEffect()
        {

        }

        /// <summary>
        /// Creates a new instance of PostEffect for post processing.
        /// </summary>
        public PostEffect(Material material)
        {
            Material = material;
        }

        protected internal override void GetDependentPasses(ICollection<Type> passTypes)
        {
            if (Material != null)
                Material.GetDependentPasses(passTypes);
        }

        public override void GetActivePasses(IList<Pass> result)
        {
            if (Enabled && Material != null)
                result.Add(this);
        }

        public override RenderTarget2D PrepareRenderTarget(DrawingContext context, Texture2D input, SurfaceFormat? preferredFormat)
        {
            int w, h;
            if (renderTargetSize.HasValue)
            {
                w = (int)renderTargetSize.Value.X;
                h = (int)renderTargetSize.Value.Y;
            }
            else
            {
                if (input != null)
                {
                    w = input.Width;
                    h = input.Height;
                }
                else
                {
                    w = context.GraphicsDevice.Viewport.Width;
                    h = context.GraphicsDevice.Viewport.Height;
                }
            }

            var format = surfaceFormat ?? preferredFormat ?? (input != null ? input.Format : context.GraphicsDevice.PresentationParameters.BackBufferFormat);

            return RenderTargetPool.GetRenderTarget(context.GraphicsDevice
                                                 , (int)(w * renderTargetScale)
                                                 , (int)(h * renderTargetScale)
                                                 , format
                                                 , DepthFormat.None);
        }

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            try
            {
                RenderTargetPool.Lock(InputTexture as RenderTarget2D);

                if (fullScreenQuad == null)
                {
                    fullScreenQuad = new FullScreenQuad(context.GraphicsDevice);
                    vertexPassThrough = new VertexPassThroughMaterial(context.GraphicsDevice);
                }

                // Apply a vertex pass through material in case the specified material does
                // not have a vertex shader.
                vertexPassThrough.BeginApply(context);

                context.GraphicsDevice.BlendState = BlendState;
                context.GraphicsDevice.DepthStencilState = DepthStencilState.None;

                Material.texture = InputTexture;
                fullScreenQuad.Draw(context, Material);
            }
            finally
            {
                RenderTargetPool.Unlock(InputTexture as RenderTarget2D);
            }
        }

        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        void ISceneObject.OnAdded(DrawingContext context)
        {
            context.RootPass.Passes.Add(this);
        }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void ISceneObject.OnRemoved(DrawingContext context)
        {
            context.RootPass.Passes.Remove(this);
        }
        #endregion
    }
}