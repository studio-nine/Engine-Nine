#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.ScreenEffects;
using System.ComponentModel;
#endregion

namespace ScreenEffects
{
    [Category("Graphics")]
    [DisplayName("Screen Effects")]
    [Description("This sample demenstrates how to create full screen post processing effects.")]
    public class ScreenEffectsGame : Microsoft.Xna.Framework.Game
    {
        SpriteBatch spriteBatch;
        Texture2D background;
        ScreenEffect screenEffect;


        public ScreenEffectsGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            // ScreenEffect requires the backbuffer to preserve contents.
            graphics.PreparingDeviceSettings += (o, e) =>
            {
                e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            };

            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            IsMouseVisible = true;
            Window.AllowUserResizing = true;
            Components.Add(new FrameRate(this, "Consolas"));
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            background = Content.Load<Texture2D>("glacier");

            screenEffect = ScreenEffect.CreateBloom(GraphicsDevice, 0.5f, 0.6f, 1.0f);
            screenEffect = new ScreenEffect(GraphicsDevice);
            screenEffect.Effects.Add(new WiggleEffect(GraphicsDevice));
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            ((IUpdateObject)screenEffect.Effects[0]).Update(gameTime);

            GraphicsDevice.Clear(Color.DarkSlateGray);

            screenEffect.Begin();
            {
                // Draw the scene between screen effect Begin/End
                spriteBatch.Begin();
                spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, null, Color.White);
                spriteBatch.End();
            }
            screenEffect.End();

            if (Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                spriteBatch.Begin();
                spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, null, Color.White);
                spriteBatch.End();
            }

            base.Draw(gameTime);
        }
    }
}
