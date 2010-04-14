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
using Isles;
using Isles.Graphics.Filters;
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
            // add several fullscreen effects from Isles.Graphics.Filters namespace.
            screenEffect = new ScreenEffect(GraphicsDevice);

            screenEffect.Effects.Add(new SaturationEffect(GraphicsDevice) { Saturation = 0.0f });
            screenEffect.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Matrix = ColorMatrixEffect.CreateBrightness(1.0f) });
        }

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// </summary>
        protected override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
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
                spriteBatch.Begin();
                spriteBatch.Draw(background, GraphicsDevice.Viewport.TitleSafeArea, Color.White);
                spriteBatch.End();
            }
            screenEffect.End();


            base.Draw(gameTime);
        }
    }
}
