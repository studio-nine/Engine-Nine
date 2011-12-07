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
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Nine;
using Nine.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.ScreenEffects;
using Nine.Animations;
using System.ComponentModel;
using Nine.Components;
#endregion

namespace SpriteAnimationGame
{
    [Category("Graphics, Animation")]
    [DisplayName("Skinned Animation")]
    [Description("This sample demenstrates how to create 2D graphics and animation.")]
    public class SpriteAnimationGame : Microsoft.Xna.Framework.Game
    {
        SpriteBatch spriteBatch;
        SpriteAnimation run;
        SpriteAnimation fireball;
        
        ColorMatrixEffect gray;
        ColorMatrixEffect highlight;
        BlurEffect blur;
        RadialBlurEffect radialBlur;
        PixelateEffect pixelate;


        public SpriteAnimationGame()
        {
            GraphicsDeviceManager graphics = new GraphicsDeviceManager(this);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreferredBackBufferWidth = 900;
            graphics.PreferredBackBufferHeight = 600;

            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
            IsMouseVisible = true;
        }


        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            Components.Add(new FrameRate(GraphicsDevice, Content.Load<SpriteFont>("Consolas")));
            Components.Add(new InputComponent(Window.Handle));

            spriteBatch = new SpriteBatch(GraphicsDevice);


            // Load animations
            //
            // We support 2 sprite animation types,
            // This run animation is processed using TextureListProcessor.
            run = new SpriteAnimation(Content.Load<TextureList>("Run"));
            run.FramesPerSecond = 24;
            run.AutoReverse = false;
            run.Play();

            // This animation is a sequense of image files.
            // It's been imported by SequentiaTextureListImporter and processed by SequentialTextureListProcessor.
            // You can choose if the images will be packed into a single large texture.
            fireball = new SpriteAnimation(Content.Load<TextureList>("fireball"));
            fireball.Play();

            // Create effects
            gray = new ColorMatrixEffect(GraphicsDevice);
            gray.Transform = ColorMatrix.CreateGrayscale();

            highlight = new ColorMatrixEffect(GraphicsDevice);
            highlight.Transform = ColorMatrix.CreateBrightness(0.2f);

            blur = new BlurEffect(GraphicsDevice);
            blur.BlurAmount = 2.0f;

            radialBlur = new RadialBlurEffect(GraphicsDevice);
            radialBlur.BlurAmount = 3.0f;

            pixelate = new PixelateEffect(GraphicsDevice);
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkSlateGray);

            // Update animations
            run.Update(gameTime.ElapsedGameTime);
            fireball.Update(gameTime.ElapsedGameTime);

            Vector2 position = new Vector2(100, 300);

            // Normal
            spriteBatch.Begin();            
            spriteBatch.Draw(fireball.Texture, position, fireball.SourceRectangle, Color.White);
            position.X += 100;
            spriteBatch.Draw(run.Texture, position, run.SourceRectangle, Color.White);
            position.X += 100;
            spriteBatch.End();


            // Saturate
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, gray);
            spriteBatch.Draw(run.Texture, position, run.SourceRectangle, Color.White);
            spriteBatch.End();
            position.X += 100;


            // Highlight
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, highlight);
            spriteBatch.Draw(run.Texture, position, run.SourceRectangle, Color.White);
            spriteBatch.End();
            position.X += 100;


            // Blur
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, blur);
            spriteBatch.Draw(run.Texture, position, run.SourceRectangle, Color.White);
            spriteBatch.End();
            position.X += 100;


            // Radial blur
            Vector2 center = new Vector2();
            
            center.X = 1.0f * (run.SourceRectangle.X + run.SourceRectangle.Width / 2) / run.Texture.Width;
            center.Y = 1.0f * (run.SourceRectangle.Y + run.SourceRectangle.Height / 2) / run.Texture.Height;
            
            radialBlur.Center = center;

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, radialBlur);
            spriteBatch.Draw(run.Texture, position, run.SourceRectangle, Color.White);
            spriteBatch.End();
            position.X += 100;


            // Blur
            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, pixelate);
            spriteBatch.Draw(run.Texture, position, run.SourceRectangle, Color.White);
            spriteBatch.End();
            position.X += 100;


            base.Draw(gameTime);
        }
    }
}
