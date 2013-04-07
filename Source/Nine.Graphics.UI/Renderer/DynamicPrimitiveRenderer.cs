namespace Nine.Graphics.UI.Renderer
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    public class DynamicPrimitiveRenderer : IRenderer
    {
        public GraphicsDevice GraphicsDevice { get { return dynamicPrimitive.GraphicsDevice; } }

        private DynamicPrimitive dynamicPrimitive;

        public DynamicPrimitiveRenderer(GraphicsDevice graphics)
        {
            dynamicPrimitive = new DynamicPrimitive(graphics);
        }

        public void Begin(DrawingContext context)
        {
            throw new NotImplementedException();
        }

        public void End(DrawingContext context)
        {
            dynamicPrimitive.Draw(context, null);
            dynamicPrimitive.Clear();
        }

        public void Draw(BoundingRectangle bound, Brush brush)
        {
            throw new NotImplementedException();
        }

        public void Draw(BoundingRectangle bound, Color color)
        {
            throw new NotImplementedException();
        }

        public void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture)
        {
            throw new NotImplementedException();
        }

        public void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture, Color color)
        {
            throw new NotImplementedException();
        }

        public void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture, Color color, Flip flip)
        {
            throw new NotImplementedException();
        }

        public void DrawString(SpriteFont Font, string Text, Vector2 position, Color color)
        {
            throw new NotImplementedException();
        }
    }
}
