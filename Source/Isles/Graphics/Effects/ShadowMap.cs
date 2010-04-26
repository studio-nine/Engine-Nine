#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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
using Microsoft.Xna.Framework.Content;
using Isles.Graphics.ScreenEffects;
#endregion


namespace Isles.Graphics.Effects
{
    public sealed class ShadowMap : IDisposable
    {
        BlurEffect blur;
        RenderToTextureEffect renderTarget;
        RenderToTextureEffect depthBlur;

        public GraphicsDevice GraphicsDevice { get; private set; }

        public int Width { get { return renderTarget.Width; } }
        public int Height { get { return renderTarget.Height; } }
        public SurfaceFormat SurfaceFormat { get { return renderTarget.SurfaceFormat; } }
        public bool BlurEnabled { get; set; }

        public BlurEffect Blur
        {
            get 
            {
                if (blur == null)
                {
                    blur = new BlurEffect(GraphicsDevice);
                    
                    // A 3x3 blur core will produce a good result while still retain framerate.
                    blur.SampleCount = BlurEffect.MinSampleCount;
                }

                return blur; 
            }
        }

        public ShadowMap(GraphicsDevice graphics, int resolution)
            : this(graphics, resolution, resolution)
        {
        }

        public ShadowMap(GraphicsDevice graphics, int width, int height)
        {
            GraphicsDevice = graphics;

            try
            {
                renderTarget = new RenderToTextureEffect(graphics, width, height, SurfaceFormat.Single);
            }
            catch (Exception ex)
            {
                renderTarget = new RenderToTextureEffect(graphics, width, height, SurfaceFormat.Color);
            }
        }

        public bool Begin()
        {
            return renderTarget.Begin();
        }

        public Texture2D End()
        {
            Texture2D map = renderTarget.End();

            if (BlurEnabled && map != null)
            {
                if (depthBlur == null)
                    depthBlur = new RenderToTextureEffect(GraphicsDevice, Width, Height, SurfaceFormat);

                // Blur H
                if (depthBlur.Begin())
                {
                    Blur.Direction = MathHelper.PiOver4;

                    GraphicsDevice.DrawSprite(map, null, null, Color.White, Blur);

                    map = depthBlur.End();

                    // Blur V
                    if (renderTarget.Begin())
                    {
                        Blur.Direction = -MathHelper.PiOver4;

                        GraphicsDevice.DrawSprite(map, null, null, Color.White, Blur);

                        map = renderTarget.End();
                    }
                }
            }

            return map;
        }

        public void Dispose()
        {
            if (renderTarget != null)
                renderTarget.Dispose();
        }
    }
}
