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
    public sealed class ShadowMapEffect : IDisposable
    {
        private DepthStencilBuffer depthStencil;
        private DepthStencilBuffer storedDepthStencil;
        private RenderTarget2D renderTarget;

        public GraphicsDevice GraphicsDevice { get; private set; }
        public int TextureResolution { get; private set; }
        public Texture2D ShadowMap { get; private set; }
        

        public ShadowMapEffect(GraphicsDevice graphics) : this(graphics, 1024) { }

        public ShadowMapEffect(GraphicsDevice graphics, int resolution)
        {
            GraphicsDevice = graphics;
            TextureResolution = resolution;

            CreateRenderTarget();
        }

        private void CreateRenderTarget()
        {
            try
            {
                // Create a stencil buffer in case our screen is not large
                // enough to hold the render target.
                depthStencil = new DepthStencilBuffer(
                    GraphicsDevice, TextureResolution, TextureResolution,
                    GraphicsDevice.DepthStencilBuffer.Format);

                // Create textures
                renderTarget = new RenderTarget2D(
                    GraphicsDevice, TextureResolution, TextureResolution, 1, SurfaceFormat.Single);
            }
            catch (Exception e)
            {
                e.ToString();

                try
                {
                    renderTarget = new RenderTarget2D(
                        GraphicsDevice, TextureResolution, TextureResolution, 1, SurfaceFormat.Color);
                }
                catch (Exception ex)
                {
                    // Some device may not support 32-bit floating point texture
                    ex.ToString();
                }
            }
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
            GraphicsDevice.SetRenderTarget(0, renderTarget);
            GraphicsDevice.DepthStencilBuffer = depthStencil;

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.Clear(Color.White);

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
                GraphicsDevice.SetRenderTarget(0, null);
                GraphicsDevice.DepthStencilBuffer = storedDepthStencil;
                storedDepthStencil = null;

                // Resolve render target, retrieve our shadow map
                return ShadowMap = renderTarget.GetTexture();
            }

            return null;
        }

        public void Dispose()
        {
            if (depthStencil != null)
                depthStencil.Dispose();

            if (renderTarget != null)
                renderTarget.Dispose();

            if (ShadowMap != null)
                ShadowMap.Dispose();
        }
    }
}
