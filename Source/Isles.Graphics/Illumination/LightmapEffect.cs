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
    public sealed class LightmapEffect : IDisposable
    {
        private Texture2D lightmask;
        private SpriteBatch sprite;
        private RenderTarget2D target;


        public GraphicsDevice GraphicsDevice { get; private set; }
        public Texture2D Lightmap { get; set; }
        public List<PointLight> LightSource { get; set; }


        public LightmapEffect(GraphicsDevice graphics) : this(graphics, null, 512, 512)
        { }

        public LightmapEffect(GraphicsDevice graphics, Texture2D texture, int width, int height)
        {
            GraphicsDevice = graphics;
            Lightmap = texture;
            LightSource = new List<PointLight>();
            
            sprite = new SpriteBatch(GraphicsDevice);            
            lightmask = InternalContents.LightCircleTexture(GraphicsDevice);
            target = new RenderTarget2D(GraphicsDevice, width, height, 0, SurfaceFormat.Color);
        }
                
        public Texture2D GetTexture()
        {
            Rectangle rect;

            // Update light map
            GraphicsDevice.RenderState.DepthBufferEnable = false;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            GraphicsDevice.SetRenderTarget(0, target);
            GraphicsDevice.Clear(Color.Black);


            sprite.Begin(SpriteBlendMode.Additive);

            if (Lightmap != null)
            {
                rect.X = rect.Y = 0;
                rect.Width = target.Width;
                rect.Height = target.Height;

                sprite.Draw(Lightmap, rect, Color.White);
            }


            foreach (PointLight light in LightSource)
            {
                if (light.Enabled &&
                    GetLightRectangle(light, out rect))
                {
                    sprite.Draw(lightmask, rect, new Color(light.DiffuseColor));
                }
            }

            sprite.End();

            GraphicsDevice.RenderState.DepthBufferEnable = true;
            GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            GraphicsDevice.SetRenderTarget(0, null);

            return target.GetTexture();
        }

        private bool GetLightRectangle(PointLight light, out Rectangle rect)
        {
            if (light.Position.Z < 0 || light.Position.Z > light.Radius)
            {
                rect = Rectangle.Empty;
                return false;
            }

            int r = (int)Math.Sqrt(light.Radius * light.Radius - light.Position.Z * light.Position.Z);

            rect.X = (int)(light.Position.X - r);
            rect.Y = (int)(light.Position.Y - r);
            rect.Width = r * 2;
            rect.Height = r * 2;
            
            return true;
        }
        
        public void Dispose()
        {
            if (sprite != null)
                sprite.Dispose();
            if (target != null)
                target.Dispose();
        }
    }
}
