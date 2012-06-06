#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ObjectModel;
using System.Collections.Generic;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// Represents a drawing pass that relies on the previous drawing result as an input.
    /// </summary>
    public abstract class PostDrawingPass : DrawingPass
    {
        #region Properties
        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the surface format of the render target.
        /// Specify null to use the surface format of the current backbuffer.
        /// </summary>
        public SurfaceFormat? SurfaceFormat
        {
            get { return surfaceFormat; }
            set 
            {
                if (value != surfaceFormat)
                {
                    renderTargetChanged = true;
                    surfaceFormat = value; 
                }
            }
        }
        private SurfaceFormat? surfaceFormat;

        /// <summary>
        /// Gets or sets the render target size.
        /// Specify null to use current viewport size.
        /// </summary>
        public Vector2? RenderTargetSize
        {
            get { return renderTargetSize; }
            set
            {
                if (value != renderTargetSize)
                {
                    renderTargetChanged = true;
                    renderTargetSize = value;                    
                }
            }
        }
        private Vector2? renderTargetSize;


        /// <summary>
        /// Gets or sets the render target scale. This value is multiplied by
        /// <c>RenderTargetSize</c> to determine the final size of the render target.
        /// </summary>
        public float RenderTargetScale
        {
            get { return renderTargetScale; }
            set
            {
                if (value != renderTargetScale)
                {
                    renderTargetChanged = true;
                    renderTargetScale = value;
                }
            }
        }
        private float renderTargetScale = 1;

        /// <summary>
        /// Gets the result of the previous pass on the composition chain.
        /// </summary>
        public Texture2D PreviousTexture
        {
            get { return previousTexture; }
            set
            {
                if (value != previousTexture)
                {
                    previousTexture = value;                    
                    renderTargetChanged = true;
                }
            }
        }
        private Texture2D previousTexture;
        
        /// <summary>
        /// Gets the render target pool.
        /// </summary>
        protected RenderTargetPool RenderTargetPool
        {
            get
            {
                if (renderTargetChanged || renderTargetPool == null)
                {
                    if (renderTargetPool != null)
                        renderTargetPool.Release();
                    renderTargetPool = RenderTargetPool.AddRef(GraphicsDevice, PreviousTexture, renderTargetSize, renderTargetScale, surfaceFormat);
                }
                return renderTargetPool; 
            }
        }
        private bool renderTargetChanged;
        private RenderTargetPool renderTargetPool;
        #endregion

        #region Fields

        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="PostDrawingPass"/> class.
        /// </summary>
        public PostDrawingPass(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            
            this.GraphicsDevice = graphics;
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            RenderTarget2D renderTarget = null;

            try
            {
                renderTarget = RenderTargetPool.Lock();
                renderTarget.Begin();

                // Draw child passes
                base.Draw(context, drawables, startIndex, length);
            }
            finally
            {
                renderTarget.End();
                renderTargetPool.Unlock(renderTarget);

                context.textures[TextureUsage.Previous] = renderTarget;
            }
        }
        #endregion
    }
}