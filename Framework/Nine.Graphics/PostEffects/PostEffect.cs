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
    using Nine.Graphics;

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

        /// <summary>
        /// Gets all the pass types that are required by this pass.
        /// </summary>
        protected internal override void GetDependentPasses(ICollection<Type> passTypes)
        {
            if (Material != null)
                Material.GetDependentPasses(passTypes);
        }

        /// <summary>
        /// Gets all the passes that are going to be rendered.
        /// </summary>
        public override void GetActivePasses(IList<Pass> result)
        {
            if (Enabled && Material != null)
                result.Add(this);
        }

        /// <summary>
        /// Prepares a render target to hold the result of this pass.
        /// </summary>
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
                    w = context.graphics.Viewport.Width;
                    h = context.graphics.Viewport.Height;
                }
            }

            var format = surfaceFormat ?? preferredFormat ?? (input != null ? input.Format : context.graphics.PresentationParameters.BackBufferFormat);

            return RenderTargetPool.GetRenderTarget(context.graphics
                                                 , (int)(w * renderTargetScale)
                                                 , (int)(h * renderTargetScale)
                                                 , format
                                                 , DepthFormat.None);
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            try
            {
                RenderTargetPool.Lock(InputTexture as RenderTarget2D);

                if (fullScreenQuad == null)
                    fullScreenQuad = new FullScreenQuad(context.graphics);
                
                context.graphics.BlendState = BlendState;
                context.graphics.DepthStencilState = DepthStencilState.None;

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
            context.rootPass.Passes.Add(this);
            AddDependency(context.mainPass);
        }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void ISceneObject.OnRemoved(DrawingContext context)
        {
            context.rootPass.Passes.Remove(this);
        }
        #endregion
    }
}