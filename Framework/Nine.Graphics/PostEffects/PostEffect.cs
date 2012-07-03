﻿#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Graphics.PostEffects
{
    /// <summary>
    /// Represents a post processing effect.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("Material")]
    public class PostEffect : Pass, ISceneObject, IPostEffect
    {
        #region Properties
        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the material used by this post effect.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets the input texture to be processed.
        /// </summary>
        public Texture2D InputTexture { get; set; }

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

        internal BlendState BlendState = BlendState.Opaque;

        private int textureWidth;
        private int textureHeight;
        private bool renderTargetChanged;

        private Material vertexPassThrough;
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
            fullScreenQuad = new FullScreenQuad(graphics);
            vertexPassThrough = GraphicsResources<VertexPassThroughMaterial>.GetInstance(graphics);
        }

        public override void GetActivePasses(IList<Pass> result)
        {
            if (Enabled && Material != null)
                result.Add(this);
        }

        public override RenderTarget2D PrepareRenderTarget(DrawingContext context, Texture2D input)
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
                    textureWidth = w;
                    textureHeight = h;
                    renderTargetChanged = true;
                }
            }

            if (renderTargetChanged)
            {
                if (renderTargetSize.HasValue)
                {
                    w = (int)renderTargetSize.Value.X;
                    h = (int)renderTargetSize.Value.Y;
                }
            }

            var format = surfaceFormat.HasValue ? surfaceFormat.Value
                                                : input != null ? input.Format
                                                : GraphicsDevice.PresentationParameters.BackBufferFormat;

            return RenderTargetPool.GetRenderTarget(GraphicsDevice
                                                 , (int)(textureWidth * renderTargetScale)
                                                 , (int)(textureHeight * renderTargetScale)
                                                 , format
                                                 , DepthFormat.None);
        }

        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            try
            {
                RenderTargetPool.Lock(InputTexture as RenderTarget2D);

                // Apply a vertex pass through material in case the specified material does
                // not have a vertex shader.
                vertexPassThrough.BeginApply(context);
                vertexPassThrough.EndApply(context);

                context.GraphicsDevice.BlendState = BlendState;
                context.GraphicsDevice.Textures[0] = InputTexture;

                Material.Texture = InputTexture;
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