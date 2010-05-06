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
using Microsoft.Xna.Framework.Input;
using Isles;
using Isles.Graphics;
using Isles.Graphics.Effects;
using Isles.Graphics.ScreenEffects;
#endregion

namespace SpriteAnimationGame
{
    /// <summary>
    /// Demonstrates how to create 2D graphics.
    /// </summary>
    public class SpriteAnimationGame : Microsoft.Xna.Framework.Game
    {
        SpriteBatch spriteBatch;
        SpriteAnimation run;
        SpriteAnimation ghost;
        ColorMatrixEffect gray;
        ColorMatrixEffect highlight;
        BlurEffect blur;
        RadialBlurEffect radialBlur;
        PixelateEffect pixelate;


        public SpriteAnimationGame()
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


            // Load animations
            //
            // We support 2 sprite animation types,
            // This run animation is processed using ImageListProcessor.
            run = new SpriteAnimation(Content.Load<ImageList>("Run"));
            run.FramesPerSecond = 10;

            // This animation is a sequense of image files.
            // It's been imported by ImageListImporter and processed by SequentialImageListProcessor.
            // You can choose if the images will be packed into a single large texture.
            ghost = new SpriteAnimation(Content.Load<ImageList>("GhostGate"));


            // Create effects
            gray = new ColorMatrixEffect(GraphicsDevice);
            gray.Matrix = ColorMatrixEffect.CreateGrayscale();

            highlight = new ColorMatrixEffect(GraphicsDevice);
            highlight.Matrix = ColorMatrixEffect.CreateBrightness(0.2f);

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
            run.Update(gameTime);
            ghost.Update(gameTime);

            Vector2 position = new Vector2(100, 300);

            // Normal
            spriteBatch.Begin();
            
            spriteBatch.Draw(ghost.Texture, position, ghost.SourceRectangle, Color.White);
            position.X += 100;

            spriteBatch.Draw(run.Texture, position, run.SourceRectangle, Color.White);
            position.X += 100;

            spriteBatch.End();


            // Saturate
            GraphicsDevice.DrawSprite(run.Texture, position, run.SourceRectangle, Color.White, gray);
            position.X += 100;

            
            // Highlight
            GraphicsDevice.DrawSprite(run.Texture, position, run.SourceRectangle, Color.White, highlight);
            position.X += 100;


            // Blur
            GraphicsDevice.DrawSprite(run.Texture, position, run.SourceRectangle, Color.White, blur);
            position.X += 100;


            // Radial blur
            Vector2 center;
            
            center.X = 1.0f * (run.SourceRectangle.X + run.SourceRectangle.Width / 2) / run.Texture.Width;
            center.Y = 1.0f * (run.SourceRectangle.Y + run.SourceRectangle.Height / 2) / run.Texture.Height;
            
            radialBlur.Center = center;

            GraphicsDevice.DrawSprite(run.Texture, position, run.SourceRectangle, Color.White, radialBlur);
            position.X += 100;


            // Blur
            GraphicsDevice.DrawSprite(run.Texture, position, run.SourceRectangle, Color.White, pixelate);
            position.X += 100;


            base.Draw(gameTime);
        }
    }
}
