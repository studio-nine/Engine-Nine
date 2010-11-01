#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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

namespace Nine
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
        private float overallFps = 0;


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
            get { return fps; }
        }

        /// <summary>
        /// The main constructor for the class.
        /// </summary>
        /// <remarks>Sets the <see cref="_gameWindowTitle"/> data member to the value of <see cref="Microsoft.Xna.Framework.GameWindow.Title"/>.</remarks>
        public FrameRate(Game game) : base(game)
        {
            UpdateFrequency = 1000;
            Color = Color.Yellow;
        }

        protected override void LoadContent()
        {
            base.LoadContent();
        }

        public override void Update(GameTime gameTime)
        {
            UpdateFPS(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            // Draw FPS text
            if (SpriteBatch != null && Font != null)
            {
                SpriteBatch.Begin();
                SpriteBatch.DrawString(Font, "FPS: " + fps, Position, Color);
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

                overallFps = (overallFps * updateCount + fps) / (updateCount + 1);
                updateCount++;
            }
        }
    }
}