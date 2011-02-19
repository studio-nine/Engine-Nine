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

        ScreenEffect linkedScreenEffect;
        ScreenEffect composedScreenEffect;


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

            linkedScreenEffect = new ScreenEffect(GraphicsDevice);

            // Create a bloom effect using two ScreenEffectPass.
            // This effect is created using LinkedEffect which is linked at
            // compile time into a single shader, thus it is likely to be faster.
            ScreenEffectPass basePass = new ScreenEffectPass(GraphicsDevice);
            basePass.Effects.Add(Content.Load<Effect>("BasePass"));
            basePass.BlendState = BlendState.Opaque;

            ScreenEffectPass brightPass = new ScreenEffectPass(GraphicsDevice);
            brightPass.Effects.Add(Content.Load<Effect>("BrightPass"));
            brightPass.BlendState = BlendState.Additive;
            brightPass.DownScaleEnabled = false;
            
            linkedScreenEffect.Passes.Add(basePass);
            linkedScreenEffect.Passes.Add(brightPass);

            // This effect is composes using basic primitive effects, it allows
            // for arbitrary combination but might be a little slower.
            composedScreenEffect = new ScreenEffect(GraphicsDevice);

            ScreenEffectPass composedBasePass = new ScreenEffectPass(GraphicsDevice);
            composedBasePass.Effects.Add(new ColorMatrixEffect(GraphicsDevice) { Matrix = ColorMatrix.CreateHue(0.7f) });
            composedBasePass.BlendState = BlendState.Opaque;

            ScreenEffectPass composedBrightPass = new ScreenEffectPass(GraphicsDevice);
            composedBrightPass.Effects.Add(new RadialBlurEffect(GraphicsDevice));
            composedBrightPass.Effects.Add(new ThresholdEffect(GraphicsDevice));
            composedBrightPass.BlendState = BlendState.Additive;
            composedBrightPass.DownScaleEnabled = false;

            composedScreenEffect.Passes.Add(composedBasePass);
            //composedScreenEffect.Passes.Add(composedBrightPass);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            ScreenEffect currentScreenEffect = !Keyboard.GetState().IsKeyDown(Keys.T) ? composedScreenEffect : linkedScreenEffect;

            if (currentScreenEffect == composedScreenEffect)
            {
                ((ColorMatrixEffect)(currentScreenEffect.Passes[0].Effects[0])).Matrix = ColorMatrix.CreateSaturation((float)gameTime.TotalGameTime.TotalSeconds / 10);
            }

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
