#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Materials;
using System.Windows.Markup;
#endregion

namespace Nine.Graphics.PostEffects
{
    /// <summary>
    /// Represents post processing effects.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("Materials")]
    public class PostEffect : DrawingPass, ISceneObject
    {
        #region Properties
        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the materials used to draw this post effect.
        /// </summary>
        public IList<Material> Materials
        {
            get { return materials; }
        }
        private List<Material> materials = new List<Material>();

        /// <summary>
        /// Gets or sets the state of the blend of this post effect.
        /// </summary>
        public BlendState BlendState { get; set; }

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


        private int textureWidth;
        private int textureHeight;
        private bool renderTargetChanged;
        private RenderTargetPool renderTargetPool;
        #endregion

        #region Methods
        
        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        public PostEffect(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");
            
            GraphicsDevice = graphics;
            BlendState = BlendState.Opaque;
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            if (renderTargetChanged || renderTargetPool == null)
            {
                if (renderTargetPool != null)
                    renderTargetPool.Release();
                ;// renderTargetPool = RenderTargetPool.AddRef(GraphicsDevice, PreviousTexture, renderTargetSize, renderTargetScale, surfaceFormat);
            }
            ;// return renderTargetPool; 

            RenderTarget2D renderTarget = null;

            try
            {
                renderTarget = renderTargetPool.Lock();
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

            var count = materials.Count;
            if (count <= 0)
                return;

            for (int i = 0; i < count; i++)
            {
                materials[i].BeginApply(context);

                ;// GraphicsDevice.DrawFullscreenQuad(PreviousTexture, SamplerState.PointClamp, Color.White, null);

                materials[i].EndApply(context);
            }
        }

        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        void ISceneObject.OnAdded(DrawingContext context)
        {
            context.Passes.Add(this);
        }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void ISceneObject.OnRemoved(DrawingContext context)
        {
            context.Passes.Remove(this);
        }
        #endregion
    }
}

