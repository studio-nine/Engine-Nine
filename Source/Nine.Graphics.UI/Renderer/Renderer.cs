namespace Nine.Graphics.UI.Renderer
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Media;

    // I would like to rename this to something better.
    public abstract class Renderer
    {
        public GraphicsDevice GraphicsDevice { get; private set; }

        public float ElapsedTime { get { return elapsedTime; } }
        internal float elapsedTime = 0;

        public bool IsRendering
        {
            get { return isRendering; }
        }
        private bool isRendering = false;

        protected Renderer(GraphicsDevice graphics)
        {
            this.GraphicsDevice = graphics;
        }

        public virtual void Begin(DrawingContext context)
        {
            if (isRendering)
                throw new ArgumentException();
            isRendering = true;
        }

        public virtual void End(DrawingContext context)
        {
            if (!isRendering)
                throw new ArgumentException();
            isRendering = false;
        }
        
        public virtual void Draw(BoundingRectangle bound, Brush brush)
        {
            brush.OnRender(this, bound);
        }

        /// <summary>
        /// Draws a rectangle
        /// </summary>
        /// <param name="bound"></param>
        /// <param name="color">Color of the Rectangle</param>
        public abstract void Draw(BoundingRectangle bound, Color color);


        /// <summary>
        /// Draws a line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="color"></param>
        public virtual void Draw(LineSegment line, Color color)
        {
            Draw(line.Start, line.End, color);
        }

        /// <summary>
        /// Draws a line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="color"></param>
        public abstract void Draw(Vector2 from, Vector2 to, Color color);

/*
        /// <summary>
        /// Draws a polygon
        /// </summary>
        /// <param name="poly"></param>
        /// <param name="color"></param>
        public abstract void Draw(IEnumerable<Vector2> poly, Color color, bool join);

*/

        /// <summary>
        /// Draws a Texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="bound"></param>
        /// <param name="Source">Source Rectangle of the Texture</param>
        public abstract void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source);

        /// <summary>
        /// Draws a Texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="bound"></param>
        /// <param name="Source">Source Rectangle of the Texture</param>
        /// <param name="color"></param>
        public abstract void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color);

        /// <summary>
        /// Draws a Texture
        /// </summary>
        /// <param name="texture"></param>
        /// <param name="bound"></param>
        /// <param name="Source">Source Rectangle of the Texture</param>
        /// <param name="color"></param>
        /// <param name="flip"></param>
        public abstract void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color, Flip flip);

        #region Text

        /// <summary>
        /// Draws a string of text
        /// </summary>
        /// <param name="Font"></param>
        /// <param name="Text">Text to draw</param>
        /// <param name="position">Location</param>
        /// <param name="color">Text Color</param>
        public virtual void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            this.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, Vector2.One);
        }

        public virtual void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, float scale)
        {
            this.DrawString(spriteFont, text, position, color, 0, Vector2.Zero, new Vector2(scale, scale));
        }

        public abstract void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale);

        #endregion
    }
}
