#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Statements
using System;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Graphics.Filters
{
    public sealed class ScreenEffect
    {
        public Collection<Effect> Effects { get; set; }

        public GraphicsDevice GraphicsDevice { get; private set; }


        SpriteBatch spriteBatch;

        RenderToTextureEffect renderToTexture;


        public ScreenEffect(GraphicsDevice graphics)
            : this(graphics, (Effect[])null)
        { }

        public ScreenEffect(GraphicsDevice graphics, Effect effect)
            : this(graphics, new Effect[] { effect })
        { }


        public ScreenEffect(GraphicsDevice graphics, Effect[] effects)
        {
            GraphicsDevice = graphics;

            spriteBatch = new SpriteBatch(graphics);

            renderToTexture = new RenderToTextureEffect(
                                              graphics,
                                              graphics.PresentationParameters.BackBufferWidth,
                                              graphics.PresentationParameters.BackBufferHeight,                                              
                                              graphics.PresentationParameters.BackBufferFormat);

            if (effects != null)
                Effects = new Collection<Effect>(effects);
            else
                Effects = new Collection<Effect>();
        }


        public bool Begin()
        {
            if (Effects.Count < 1)
                throw new InvalidOperationException("Must contain at least one effect");

            return renderToTexture.Begin();
        }


        public void End()
        {
            Rectangle screen;

            screen.X = 0;
            screen.Y = 0;
            screen.Width = GraphicsDevice.PresentationParameters.BackBufferWidth;
            screen.Height = GraphicsDevice.PresentationParameters.BackBufferHeight;


            Texture2D backbuffer = renderToTexture.End();


            // Draw first n - 1 effects to render target
            for (int i = 0; i < Effects.Count - 1; i++)
            {
                renderToTexture.Begin();

                Draw(screen, backbuffer, Effects[i]);

                backbuffer = renderToTexture.End();
            }

            // Draw last effect to the backbuffer
            Draw(screen, backbuffer, Effects[Effects.Count - 1]);
        }

        private void Draw(Rectangle screen, Texture2D backbuffer, Effect effect)
        {
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();

            spriteBatch.Draw(backbuffer, screen, Color.White);

            effect.CurrentTechnique.Passes[0].End();
            effect.End();

            spriteBatch.End();
        }
    }
}

