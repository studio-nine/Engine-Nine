#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

using System;
using System.IO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Components;
using Nine.Graphics;

namespace Nine
{
    [TestClass]
    public class GraphicsTest
    {
        public Random Random
        {
            get { return random; }
        }
        Random random = new Random();

        public GraphicsDevice GraphicsDevice 
        {
            get { return graphics; } 
        }
        static readonly GraphicsDevice graphics = GraphicsDeviceService.AddRef().GraphicsDevice;

        protected TimeSpan ElapsedTime
        {
            get { return TimeSpan.FromSeconds(1.0 / 60); }
        }

        protected Color RandomColor()
        {
            return new Color((float)Random.NextDouble(), (float)Random.NextDouble(), (float)Random.NextDouble());
        }

        protected Color RandomColor(float min, float max)
        {
            return new Color(RandomFloat(min, max), RandomFloat(min, max), RandomFloat(min, max));
        }

        protected float RandomFloat()
        {
            return (float)Random.NextDouble();
        }

        protected float RandomFloat(float min, float max)
        {
            return ((float)Random.NextDouble()) * (max - min) + min;
        }

        protected Texture2D SaveScreenShot(string filename, Action draw)
        {
            filename = filename ?? "Default";
            var backBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            backBuffer.Begin();
            GraphicsDevice.Clear(Color.Black);

            draw();

            backBuffer.End();
            using (var output = new FileStream(filename + "-" + Guid.NewGuid().ToString("B").ToUpper() + ".png", FileMode.Create))
            {
                backBuffer.SaveAsPng(output, backBuffer.Width, backBuffer.Height);
            }
            return backBuffer;
        }

        protected Texture2D SaveScreenShot(ref string filename, Action draw, Action drawOverlay)
        {
            filename = filename ?? "Default";
            filename = filename + "-" + Guid.NewGuid().ToString("B").ToUpper() + ".png";
            var backBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            backBuffer.Begin();
            GraphicsDevice.Clear(Color.Black);

            draw();

            backBuffer.End();
            
            var frontBuffer = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height);
            frontBuffer.Begin();

            GraphicsDevice.Clear(Color.Black);

            GraphicsDevice.DrawFullscreenQuad(backBuffer, null, null, Color.White, null);
            GraphicsDevice.Textures[0] = null;

            drawOverlay();

            frontBuffer.End();

            using (var output = new FileStream(filename, FileMode.Create))
            {
                frontBuffer.SaveAsPng(output, frontBuffer.Width, frontBuffer.Height);
            }
            frontBuffer.Dispose();
            return backBuffer;
        }
    }
}
