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
        ScreenEffect screenEffectTest;


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
            Components.Add(new FrameRate(this, "Consolas"));
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

            screenEffect = new ScreenEffect(GraphicsDevice);

            // Create a bloom effect using two ScreenEffectPass.
            ScreenEffectPass basePass = new ScreenEffectPass(GraphicsDevice);
            basePass.Effects.Add(Content.Load<Effect>("BasePass"));
            //basePass.Color = Color.Yellow;
            basePass.BlendState = BlendState.Opaque;

            ScreenEffectPass brightPass = new ScreenEffectPass(GraphicsDevice);
            brightPass.Effects.Add(Content.Load<Effect>("BrightPass"));
            brightPass.BlendState = BlendState.Additive;
            brightPass.DownScaleEnabled = false;
            
            screenEffect.Passes.Add(basePass);
            screenEffect.Passes.Add(brightPass);


            screenEffectTest = new ScreenEffect(GraphicsDevice);

            ScreenEffectPass basePassTest = new ScreenEffectPass(GraphicsDevice);
            basePassTest.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Matrix = ColorMatrix.CreateSaturation(0.9f) });
            basePassTest.BlendState = BlendState.Opaque;

            ScreenEffectPass brightPassTest = new ScreenEffectPass(GraphicsDevice);
            brightPassTest.Effects.Add(new RadialBlurEffect(GraphicsDevice));
            brightPassTest.Effects.Add(new ThresholdEffect(GraphicsDevice));
            brightPassTest.BlendState = BlendState.Additive;
            brightPassTest.DownScaleEnabled = false;

            screenEffectTest.Passes.Add(basePassTest);
            screenEffectTest.Passes.Add(brightPassTest);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            ScreenEffect currentScreenEffect = Keyboard.GetState().IsKeyDown(Keys.T) ? screenEffectTest : screenEffect;
            
            currentScreenEffect.Begin();
            {
                // Draw the scene between screen effect Begin/End
                spriteBatch.Begin();
                spriteBatch.Draw(background, GraphicsDevice.Viewport.Bounds, null, Color.White);
                spriteBatch.End();
            }
            currentScreenEffect.End();

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
