#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Graphics.Effects
{
    public sealed class RenderToTextureEffect : IDisposable
    {
        private DepthStencilBuffer depthStencil;
        private DepthStencilBuffer storedDepthStencil;
        private RenderTarget2D renderTarget;
        private RenderTarget2D previousRenderTarget;

        public GraphicsDevice GraphicsDevice { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public Texture2D Texture { get; private set; }
                

        public RenderToTextureEffect(GraphicsDevice graphics, int width, int height)
        {
            GraphicsDevice = graphics;
            Width = width;
            Height = height;


            CreateRenderTarget();
        }

        private void CreateRenderTarget()
        {
            try
            {
                // Create a stencil buffer in case our screen is not large
                // enough to hold the render target.
                depthStencil = new DepthStencilBuffer(
                    GraphicsDevice, Width, Height,
                    GraphicsDevice.DepthStencilBuffer.Format);

                // Create textures
                renderTarget = new RenderTarget2D(
                    GraphicsDevice, Width, Height, 0, SurfaceFormat.Color);
            }
            catch (Exception e) { e.ToString(); }
        }

        
        /// <summary>
        /// Begins a shadow mapping generation process
        /// </summary>
        public bool Begin()
        {            
            if (renderTarget != null && (renderTarget.IsDisposed || renderTarget.IsContentLost))
                CreateRenderTarget();

            // Return false if the shadow mapping effect is not initialized
            if (renderTarget == null || depthStencil == null)
                return false;


            // Store current stencil buffer
            storedDepthStencil = GraphicsDevice.DepthStencilBuffer;

            // Set shadow mapping targets
            previousRenderTarget = GraphicsDevice.GetRenderTarget(0) as RenderTarget2D;
            GraphicsDevice.SetRenderTarget(0, renderTarget);
            GraphicsDevice.DepthStencilBuffer = depthStencil;

            GraphicsDevice.RenderState.DepthBufferEnable = true;

            return true;
        }
        

        /// <summary>
        /// Ends shadow mapping generation and produce the generated shadow map
        /// </summary>
        /// <returns>
        /// Shadow map created.
        /// </returns>
        public Texture2D End()
        {
            // Begin must be called first
            if (storedDepthStencil != null)
            {
                // Restore everything
                GraphicsDevice.SetRenderTarget(0, previousRenderTarget);
                GraphicsDevice.DepthStencilBuffer = storedDepthStencil;
                storedDepthStencil = null;

                // Resolve render target, retrieve our shadow map
                return Texture = renderTarget.GetTexture();
            }

            return null;
        }

        public void Dispose()
        {
            if (depthStencil != null)
                depthStencil.Dispose();

            if (renderTarget != null)
                renderTarget.Dispose();

            if (Texture != null)
                Texture.Dispose();
        }
    }
}
