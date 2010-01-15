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


namespace Isles.Graphics.Illumination
{
    public class LightmapShrinker : IDisposable
    {
        private SpriteBatch sprite;
        private RenderTarget2D target;


        public GraphicsDevice GraphicsDevice { get; private set; }
        public Texture2D Texture { get; private set; }


        public LightmapShrinker(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            
            sprite = new SpriteBatch(GraphicsDevice);
        }
                
        public Texture2D Shink(Texture2D texture)
        {
            if (Texture == null)
                return Texture = texture;

            if (texture.Width != Texture.Width || texture.Height != Texture.Height)
                throw new ArgumentException();

            if (target == null || target.IsDisposed || target.IsContentLost)
                target = new RenderTarget2D(GraphicsDevice, texture.Width, texture.Height, 0, SurfaceFormat.Color);

                        
            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            GraphicsDevice.SetRenderTarget(0, target);
            GraphicsDevice.Clear(Color.Black);


            sprite.Begin(SpriteBlendMode.Additive);
            sprite.Draw(Texture, Vector2.Zero, new Color(240, 240, 240, 255));
            sprite.Draw(texture, Vector2.Zero, Color.White);
            sprite.End();

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            GraphicsDevice.SetRenderTarget(0, null);

            return Texture = target.GetTexture();
        }

        public void Dispose()
        {
            if (target != null)
                target.Dispose();
            if (sprite != null)
                sprite.Dispose();
        }
    }
}
