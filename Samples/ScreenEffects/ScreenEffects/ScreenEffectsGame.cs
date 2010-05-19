#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using Nine.Graphics.ScreenEffects;
#endregion

namespace ScreenEffects
{
    /// <summary>
    /// Sample game for full screen post processing effects.
    /// </summary>
    public class ScreenEffectsGame : Microsoft.Xna.Framework.Game
    {
        SpriteBatch spriteBatch;

        Texture2D background;

        ScreenEffect screenEffect;


        public ScreenEffectsGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";

            IsMouseVisible = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>("glacier");


            // Create a screen effect instance, 
            // add several fullscreen effects from Nine.Graphics.ScreenEffects namespace.
            screenEffect = new ScreenEffect(GraphicsDevice);


            screenEffect.Effects.Add(new PixelateEffect(GraphicsDevice));
            screenEffect.Effects.Add(new RadialBlurEffect(GraphicsDevice));
            screenEffect.Effects.Add(new BloomEffect(GraphicsDevice));
            screenEffect.Effects.Add(new BlurEffect(GraphicsDevice));
            screenEffect.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Matrix = ColorMatrixEffect.CreateHue(0.9f) });
            //screenEffect.Effects.Add(new SilouetteEffect(GraphicsDevice));                
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            if (screenEffect.Begin())
            {
                // Draw the scene between screen effect Begin/End
                GraphicsDevice.DrawSprite(background, GraphicsDevice.Viewport.TitleSafeArea, null, Color.White, null);

                // Draw screen effect
                screenEffect.End();
            }

            if (Microsoft.Xna.Framework.Input.Keyboard.GetState().IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Space))
                GraphicsDevice.DrawSprite(background, GraphicsDevice.Viewport.TitleSafeArea, null, Color.White, null);


            base.Draw(gameTime);
        }
    }
}
