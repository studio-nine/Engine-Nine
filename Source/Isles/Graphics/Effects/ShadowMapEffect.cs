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
        public GraphicsDevice GraphicsDevice { get; private set; }
        public int Resolution { get; private set; }
        public Texture2D ShadowMap { get; private set; }
        public SurfaceFormat SurfaceFormat { get; private set; }


        private RenderToTextureEffect renderToTexture;
        

        public ShadowMapEffect(GraphicsDevice graphics) : this(graphics, 1024, SurfaceFormat.Single) { }

        public ShadowMapEffect(GraphicsDevice graphics, int resolution, SurfaceFormat surfaceFormat)
        {
            GraphicsDevice = graphics;

            Resolution = resolution;
            SurfaceFormat = surfaceFormat;

            CreateRenderTarget();
        }

        private void CreateRenderTarget()
        {
            try
            {
                renderToTexture = new RenderToTextureEffect(GraphicsDevice,
                                                            Resolution,
                                                            Resolution,
                                                            SurfaceFormat);
            }
            catch (Exception e)
            {
                e.ToString();

                try
                {
                    // Some device may not support 32-bit floating point texture
                    renderToTexture = new RenderToTextureEffect(GraphicsDevice,
                                                                Resolution,
                                                                Resolution,
                                                                SurfaceFormat.Color);
                }
                catch (Exception ex)
                {
                    ex.ToString();
                }
            }
        }

        
        /// <summary>
        /// Begins a shadow mapping generation process
        /// </summary>
        public bool Begin()
        {
            if (!renderToTexture.Begin())
                return false;

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
            return ShadowMap = renderToTexture.End();
        }

        public void Dispose()
        {
            renderToTexture.Dispose();
        }
    }
}
