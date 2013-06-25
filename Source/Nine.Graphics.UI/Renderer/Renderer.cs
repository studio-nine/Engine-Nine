namespace Nine.Graphics.UI.Renderer
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Media;

    public abstract class Renderer
    {
        public GraphicsDevice GraphicsDevice { get; internal set; }
        public float ElapsedTime { get; internal set; }

        protected Renderer(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
        }

        public abstract void Begin(DrawingContext context);
        public abstract void End(DrawingContext context);

        public virtual void Draw(BoundingRectangle bound, Brush brush)
        {
            brush.OnRender(this, bound);
        }

        public abstract void Draw(BoundingRectangle bound, Color color);

        public abstract void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source);
        public abstract void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color);
        public abstract void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color, Flip flip);

        public abstract void DrawString(SpriteFont Font, string Text, Vector2 position, Color color);
    }
}
