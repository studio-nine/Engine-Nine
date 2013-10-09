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
        internal SpriteBatch spriteBatch;

        public SpriteBatchRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            spriteBatch = GraphicsResources<SpriteBatch>.GetInstance(graphics);
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
            spriteBatch.Draw(new Vector2(bound.X, bound.Y), new Vector2(bound.Width, bound.Height), color);
        }

        public override void Draw(Vector2 from, Vector2 to, Color color)
        {
            spriteBatch.DrawLine(from, to, color, 1);
        }

        public override void Draw(System.Collections.Generic.IEnumerable<Vector2> poly, Color color, bool join)
        {

        }

        #region Texture

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

        #endregion

        #region Text

        public override void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(spriteFont, text, position, color);
        }
        
        public override void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
        {
            spriteBatch.DrawString(spriteFont, text, position, color, rotation, origin, scale, SpriteEffects.None, 0);
        }

        #endregion
    }
}
