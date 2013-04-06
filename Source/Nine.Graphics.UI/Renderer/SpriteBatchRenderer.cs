namespace Nine.Graphics.UI.Renderer
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Internal;
    using Nine.Graphics.UI.Media;

    public class SpriteBatchRenderer : IRenderer
    {
        public GraphicsDevice GraphicsDevice { get { return spriteBatch.GraphicsDevice; } }

        private SpriteBatch spriteBatch;

        public SpriteBatchRenderer(GraphicsDevice graphics)
        {
            spriteBatch = new SpriteBatch(graphics);
        }

        public void Begin(DrawingContext context)
        {
            spriteBatch.Begin();
        }

        public void End(DrawingContext context)
        {
            spriteBatch.End();
        }

        public void Draw(BoundingRectangle bound, Brush brush)
        {
            var solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var color = solidColorBrush.ToColor();
                
                var texture = GraphicsResources<BlankTexture>.GetInstance(spriteBatch.GraphicsDevice);
                spriteBatch.Draw(texture.Texture, bound, color);
                return;
            }

            var imageBrush = brush as ImageBrush;
            if (imageBrush != null)
            {
                var texture = imageBrush.Source;
                // This don't really work, make own math function
                var Scale = Nine.Graphics.UI.Internal.Controls.Viewbox.ComputeScaleFactor(
                        new Vector2(bound.Width, bound.Height), new Vector2(texture.Width, texture.Height), imageBrush.Stretch, imageBrush.StretchDirection);

                SpriteEffects Effects = SpriteEffects.None;
                if (imageBrush.Flip != Flip.None & imageBrush.Flip != Flip.Both)
                    Effects = (SpriteEffects)imageBrush.Flip;
                else if (imageBrush.Flip == Flip.Both)
                    Effects = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;

                spriteBatch.Draw(texture, new Vector2(bound.X, bound.Y), imageBrush.SourceRectangle, Color.White, 0, Vector2.Zero, Scale, Effects, 0);
                return;
            }

            var gradientBrush = brush as GradientBrush;
            if (gradientBrush != null)
            {
                return;
            }

            var visualBrush = brush as VisualBrush;
            if (visualBrush != null)
            {
                return;
            }

            throw new NotSupportedException("brush");
        }

        public void Draw(BoundingRectangle bound, Color color)
        {
            var texture = GraphicsResources<BlankTexture>.GetInstance(spriteBatch.GraphicsDevice);
            spriteBatch.Draw(texture.Texture, bound, color);
        }

        public void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture)
        {
            this.Draw(bound, Source, texture, Color.White, Flip.None);
        }

        public void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture, Color color)
        {
            this.Draw(bound, Source, texture, color, Flip.None);
        }

        public void Draw(BoundingRectangle bound, Rectangle? Source, Texture2D texture, Color color, Flip flip)
        {
            SpriteEffects Effects = SpriteEffects.None;
            if (flip != Flip.None & flip != Flip.Both)
                Effects = (SpriteEffects)flip;
            else if (flip == Flip.Both)
                Effects = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;

            spriteBatch.Draw(texture, bound, Source, color, 0, Vector2.Zero, Effects, 0);

        }

        public void DrawString(SpriteFont Font, string Text, Vector2 position, Color color)
        {
            spriteBatch.DrawString(Font, Text, position, color);
        }
    }
}
