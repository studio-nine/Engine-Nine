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


namespace Isles.Graphics
{
    public sealed class RenderToTextureEffect : IDisposable
    {
        private RenderTarget2D renderTarget;
        private RenderTarget2D previousRenderTarget;

        public GraphicsDevice GraphicsDevice { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }        
        public SurfaceFormat SurfaceFormat { get; private set; }
        public Texture2D Texture { get; private set; }
                

        public RenderToTextureEffect(GraphicsDevice graphics, int width, int height, SurfaceFormat surfaceFormat)
        {
            GraphicsDevice = graphics;

            Width = width;
            Height = height;

            SurfaceFormat = surfaceFormat;


            CreateRenderTarget();
        }

        private void CreateRenderTarget()
        {
            // Create textures
            renderTarget = new RenderTarget2D(
                GraphicsDevice, Width, Height, 
                GraphicsAdapter.DefaultAdapter.IsProfileSupported(GraphicsProfile.HiDef),
                SurfaceFormat, GraphicsDevice.PresentationParameters.DepthStencilFormat);
        }

        
        /// <summary>
        /// Begins a shadow mapping generation process
        /// </summary>
        public bool Begin()
        {            
            if (renderTarget != null && (renderTarget.IsDisposed || renderTarget.IsContentLost))
                CreateRenderTarget();

            // Return false if the shadow mapping effect is not initialized
            if (renderTarget == null)
                return false;


            // Store current stencil buffer
            RenderTargetBinding[] bindings = GraphicsDevice.GetRenderTargets();

            if (bindings.Length > 0)
                previousRenderTarget = bindings[0].RenderTarget as RenderTarget2D;

            // Set shadow mapping targets
            GraphicsDevice.SetRenderTarget(renderTarget);

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
            // Restore everything
            GraphicsDevice.SetRenderTarget(previousRenderTarget);

            // Resolve render target, retrieve our shadow map
            return Texture = renderTarget;
        }

        public void Dispose()
        {
            if (renderTarget != null)
                renderTarget.Dispose();

            if (Texture != null)
                Texture.Dispose();
        }
    }
}
