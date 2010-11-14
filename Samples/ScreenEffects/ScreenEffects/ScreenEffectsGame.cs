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
        ScreenEffect testScreenEffect;


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


            // Create a screen effect instance, 
            // add several fullscreen effects from Nine.Graphics.ScreenEffects namespace.
            screenEffect = new ScreenEffect(GraphicsDevice);
            //screenEffect.Effects.Add(new PixelateEffect(GraphicsDevice));
            //screenEffect.Effects.Add(new RadialBlurEffect(GraphicsDevice));
            //screenEffect.Effects.Add(new BlurEffect(GraphicsDevice));
            //screenEffect.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Matrix = ColorMatrix.CreateHue(0.9f) });

            // Create a bloom effect using two ScreenEffectPass.
            ScreenEffectPass basePass = new ScreenEffectPass(GraphicsDevice);
            basePass.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Matrix = ColorMatrix.CreateSaturation(0.9f) });
            basePass.BlendState = BlendState.Opaque;

            ScreenEffectPass brightPass = new ScreenEffectPass(GraphicsDevice);
            brightPass.Effects.Add(new RadialBlurEffect(GraphicsDevice));
            brightPass.Effects.Add(new ThresholdEffect(GraphicsDevice));
            //brightPass.Effects.Add(new BlurEffect(GraphicsDevice) { Direction = MathHelper.ToRadians(45) });
            //brightPass.Effects.Add(new BlurEffect(GraphicsDevice) { Direction = MathHelper.ToRadians(-45) });
            brightPass.BlendState = BlendState.Additive;            
            brightPass.DownScaleEnabled = true;
            
            screenEffect.Passes.Add(basePass);
            screenEffect.Passes.Add(brightPass);

            // Setup a test effect to show that combining primitive effect into a single
            // shader does not improve much performance, if this test effect doesn't
            // require any intermediate render targets, but the above screen effect requires
            // 3 render targets, so chaining primitive effect in the above way provides
            // maximum flexiablity without impact performance.
            // 
            // Enable down scale really boost performance, try set brightPass.DownSaleEnabled to true.
            testScreenEffect = new ScreenEffect(GraphicsDevice);
            testScreenEffect.Effects.Add(new TestEffect(GraphicsDevice));
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            ScreenEffect currentScreenEffect = Keyboard.GetState().IsKeyDown(Keys.T) ? testScreenEffect : screenEffect;

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
