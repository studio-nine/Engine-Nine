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
    public class PostEffect : DrawingPassChain, ISceneObject
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

        private FullScreenQuad fullScreenQuad;
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
            fullScreenQuad = new FullScreenQuad(graphics);
        }

        public override RenderTarget2D PrepareRenderTarget(Texture2D input)
        {
            EnsureRenderTargetPool(input);
            return renderTargetPool.Create();
        }

        private void EnsureRenderTargetPool(Texture2D input)
        {
            int w, h;
            if (!renderTargetSize.HasValue)
            {
                if (input != null)
                {
                    w = input.Width;
                    h = input.Height;
                }
                else
                {
                    w = GraphicsDevice.Viewport.Width;
                    h = GraphicsDevice.Viewport.Height;
                }

                if (w != textureWidth || h != textureHeight)
                {
                    textureWidth = w; ;
                    textureHeight = h;
                    renderTargetChanged = true;
                }
            }

            if (renderTargetChanged || renderTargetPool == null)
            {
                if (renderTargetSize.HasValue)
                {
                    w = (int)renderTargetSize.Value.X;
                    h = (int)renderTargetSize.Value.Y;
                }

                var format = surfaceFormat.HasValue ? surfaceFormat.Value
                                                    : input != null ? input.Format
                                                    : GraphicsDevice.PresentationParameters.BackBufferFormat;

                renderTargetPool = RenderTargetPool.AddRef(GraphicsDevice
                                                        , (int)(textureWidth * renderTargetScale)
                                                        , (int)(textureHeight * renderTargetScale)
                                                        , false
                                                        , format
                                                        , DepthFormat.None);
            }
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            var count = materials.Count;
            if (count <= 0)
                return;

            Material material;
            RenderTarget2D intermediate = null;
            Texture2D input = context.textures[TextureUsage.Previous] as Texture2D;
            EnsureRenderTargetPool(input);
                        
            for (int i = 0; i < count - 1; i++)
            {
                try
                {
                    RenderTargetPool.Lock(input as RenderTarget2D);
                    intermediate = renderTargetPool.Create();
                    intermediate.Begin();

                    material = materials[i];
                    material.Texture = input;
                    fullScreenQuad.Draw(context, material);
                }
                finally
                {
                    intermediate.End();
                    RenderTargetPool.Unlock(input as RenderTarget2D);
                    input = intermediate;
                }
            }

            material = materials[count - 1];
            material.Texture = input;
            fullScreenQuad.Draw(context, material);
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