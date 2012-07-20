namespace Nine.Components
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;


    /// <summary>
    /// Frame rate profiler
    /// </summary>
    public class FrameRate : Nine.IDrawable
    {
        private int updateCount = 0;
        private int currentFrame = 0;
        private int counter = 0;
        private TimeSpan elapsedTimeSinceLastUpdate = TimeSpan.Zero;
        private float fps = 0;
        private float overallFps = 0;
        private SpriteBatch spriteBatch;
        
        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="FrameRate"/> is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Time needed to calculate FPS.
        /// </summary>
        public TimeSpan UpdateFrequency { get; set; }

        /// <summary>
        /// Gets or sets the sprite font used to draw FPS string
        /// </summary>
        public SpriteFont Font { get; set; }

        /// <summary>
        /// Gets or sets the color used to draw FPS string
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or set the frame rate position on the screen
        /// </summary>
        public Vector2 Position { get; set; }

        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        public float Scale { get; set; }

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
        public float OverallFramesPerSecond
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
        public FrameRate(GraphicsDevice graphics, SpriteFont font)
        {
            this.Font = font;
            this.GraphicsDevice = graphics;
            this.UpdateFrequency = TimeSpan.FromSeconds(1);
            this.Color = new Color(255, 255, 0, 255);
            this.Visible = true;
            this.Scale = 1;
            this.spriteBatch = GraphicsResources<SpriteBatch>.GetInstance(GraphicsDevice);
        }
        
        public void Draw(TimeSpan elapsedTime)
        {
            UpdateFPS(elapsedTime);

            // Draw FPS text
            if (Visible && Font != null && GraphicsDevice != null)
            {
                spriteBatch.Begin(SpriteSortMode.Immediate, null);
                spriteBatch.DrawString(Font, string.Format("FPS: {0:00.00}", fps), Position, Color, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                spriteBatch.End();

                GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            }
        }

        private void UpdateFPS(TimeSpan elapsedTime)
        {
            counter++;
            currentFrame++;

            elapsedTimeSinceLastUpdate += elapsedTime;

            if (elapsedTimeSinceLastUpdate >= UpdateFrequency)
            {
                fps = (float)(1000 * counter / elapsedTimeSinceLastUpdate.TotalMilliseconds);
                counter = 0;
                elapsedTimeSinceLastUpdate -= UpdateFrequency;

                overallFps = (overallFps * updateCount + fps) / (updateCount + 1);
                updateCount++;
            }
        }
    }
}