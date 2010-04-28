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
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Components
{
    /// <summary>
    /// Frame rate profiler
    /// </summary>
    public class FrameRate : DrawableGameComponent
    {
        private int updateCount = 0;
        private int currentFrame = 0;
        private int counter = 0;
        private double storedTime = 0;
        private float fps = 0;
        private float fpsInterpolated = 60.0f;
        private float overallFps = 0;
        private string fontFile = null;


        /// <summary>
        /// Gets or set the frame rate position on the screen
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Time needed to calculate FPS, Measured in milliseconds
        /// </summary>
        public float UpdateFrequency { get; set; }

        /// <summary>
        /// Gets or sets the sprite batch used to draw FPS string
        /// </summary>
        public SpriteBatch SpriteBatch { get; set; }
        
        /// <summary>
        /// Gets or sets the sprite font used to draw FPS string
        /// </summary>
        public SpriteFont Font { get; set; }

        /// <summary>
        /// Gets or sets the color used to draw FPS string
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets the total number of frames since profiler started
        /// </summary>
        public int CurrentFrame
        {
            get { return currentFrame; }
        }

        /// <summary>
        /// Gets the average frame rate up until now
        /// </summary>
        public float OverallFPS
        {
            get { return overallFps; }
        }

        /// <summary>
        /// Gets the current Frame Per Second for the game
        /// </summary>
        public float FramesPerSecond
        {
            get { return fpsInterpolated; }
        }

        /// <summary>
        /// The main constructor for the class.
        /// </summary>
        /// <param name="game">The <see cref="Microsoft.Xna.Framework.Game" /> instance for this <see cref="DrawableGameComponent"/> to use.</param>
        /// <remarks>Sets the <see cref="_gameWindowTitle"/> data member to the value of <see cref="Microsoft.Xna.Framework.GameWindow.Title"/>.</remarks>
        public FrameRate(Game game) : base(game)
        {
            if (game == null)
                throw new ArgumentNullException();

            UpdateFrequency = 1000;
            Color = Color.Yellow;
        }
        
        /// <param name="fontFile">
        /// If specified, framerate automatically loads the font file.
        /// otherwise you have to set Font property manually to see the result.
        /// </param>
        public FrameRate(Game game, string fontFile) : base(game)
        {
            if (game == null)
                throw new ArgumentNullException();

            UpdateFrequency = 1000;
            Color = Color.Yellow;

            this.fontFile = fontFile;
        }


        protected override void LoadContent()
        {
            if (SpriteBatch == null)
                SpriteBatch = new SpriteBatch(GraphicsDevice);

            if (!string.IsNullOrEmpty(fontFile))
                Font = Game.Content.Load<SpriteFont>(fontFile);

            base.LoadContent();
        }


        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            if (!this.Visible || !this.Enabled)
                return;

            UpdateFPS(gameTime);

            // Draw FPS text
            if (SpriteBatch != null && Font != null)
            {
                SpriteBatch.Begin();
                SpriteBatch.DrawString(Font, "FPS: " + fpsInterpolated, Position, Color);
                SpriteBatch.End();
            }
        }


        private void UpdateFPS(GameTime gameTime)
        {
            if (!this.Enabled)
                return;

            counter++;
            currentFrame++;

            float elapsed = (float)(gameTime.TotalGameTime.TotalMilliseconds - storedTime);

            if (elapsed > UpdateFrequency)
            {
                fps = 1000 * counter / elapsed;
                counter = 0;
                storedTime = gameTime.TotalGameTime.TotalMilliseconds;

                fpsInterpolated =
                    MathHelper.Lerp(fpsInterpolated, fps, 0.5f);

                overallFps = (overallFps * updateCount + fps) / (updateCount + 1);
                updateCount++;
            }
        }

    }
}