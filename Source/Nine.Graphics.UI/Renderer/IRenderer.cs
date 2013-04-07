namespace Nine.Graphics.UI.Renderer
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Media;

    public interface IRenderer
    {
        GraphicsDevice GraphicsDevice { get; }

        void Begin(DrawingContext context);
        void End(DrawingContext context);

        void Draw(BoundingRectangle bound, Brush brush);
        void Draw(BoundingRectangle bound, Color color);

        void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture);
        void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture, Color color);
        void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture, Color color, Flip flip);

        void DrawString(SpriteFont Font, string Text, Vector2 position, Color color);
    }
}
