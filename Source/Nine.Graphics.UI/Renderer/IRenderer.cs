namespace Nine.Graphics.UI.Renderer
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Media;

    public interface IRenderer
    {
        GraphicsDevice GraphicsDevice { get; }
        float ElapsedTime { get; set; }

        void Begin(DrawingContext context);
        void End(DrawingContext context);

        void Draw(BoundingRectangle bound, Brush brush);
        void Draw(BoundingRectangle bound, Color color);

        void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source);
        void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color);
        void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color, Flip flip);

        void DrawString(SpriteFont Font, string Text, Vector2 position, Color color);
    }
}
