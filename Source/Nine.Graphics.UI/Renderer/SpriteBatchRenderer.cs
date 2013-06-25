namespace Nine.Graphics.UI.Renderer
{
    using System;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Internal;
    using Nine.Graphics.UI.Media;

    public class SpriteBatchRenderer : Renderer
    {
        private SpriteBatch spriteBatch;

        public SpriteBatchRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            spriteBatch = new SpriteBatch(graphics);
        }

        public override void Begin(DrawingContext context)
        {
            spriteBatch.Begin();
        }

        public override void End(DrawingContext context)
        {
            spriteBatch.End();
        }

        public override void Draw(BoundingRectangle bound, Color color)
        {
            spriteBatch.Draw(bound, color);
        }

        public override void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source)
        {
            this.Draw(texture, bound, Source, Color.White, Flip.None);
        }

        public override void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color)
        {
            this.Draw(texture, bound, Source, color, Flip.None);
        }

        public override void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color, Flip flip)
        {
            SpriteEffects Effects = SpriteEffects.None;
            if (flip != Flip.None & flip != Flip.Both)
                Effects = (SpriteEffects)flip;
            else if (flip == Flip.Both)
                Effects = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(texture, bound, Source, color, 0, Vector2.Zero, Effects, 0);

        }

        public override void DrawString(SpriteFont Font, string Text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(Font, Text, position, color);
        }
    }
}
