namespace Nine.Graphics.UI.Media
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    // TODO: Rename to something better? :)

    /// <summary>
    /// This brush separate a texture into 9 parts and render them in a layout.
    /// </summary>
    public class SpecialBrush : Brush
    {
        public Texture2D Texture { get; set; }

        [TypeConverter(typeof(Nine.Design.RectangleConverter))]
        public Rectangle SourceArea { get; set; }

        protected internal override void OnRender(Renderer.Renderer renderer, BoundingRectangle bound)
        {
            // TODO: Optimize calculations

            var color = Color.White;

            int ssw = (int)(SourceArea.Width / 3);
            int ssh = (int)(SourceArea.Height / 3);
            
            var topleft = new Rectangle(SourceArea.X, SourceArea.Y, ssw, ssh);
            var topright = new Rectangle(SourceArea.X + (ssw * 2), SourceArea.Y, ssw, ssh);
            var bottomleft = new Rectangle(SourceArea.X, SourceArea.Y + (ssh * 2), ssw, ssh);
            var bottomright = new Rectangle(SourceArea.X + (ssw * 2), SourceArea.Y + (ssh * 2), ssw, ssh);

            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X,bound.Y,
                    topleft.Width, topleft.Height), topleft, color);
            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X + bound.Width - topright.Width, bound.Y,
                    topright.Width, topright.Height), topright, color);
            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X, bound.Y + bound.Height - bottomleft.Height,
                    bottomleft.Width, bottomleft.Height), bottomleft, color);
            renderer.Draw(Texture,
                new BoundingRectangle(bound.X + bound.Width - bottomright.Width,bound.Y + bound.Height - bottomright.Height,
                    bottomright.Width, bottomright.Height), bottomright, color);
            
            var top = new Rectangle(SourceArea.X + ssw, SourceArea.Y, ssw, ssh);
            var right = new Rectangle(SourceArea.X + (ssw * 2), SourceArea.Y + ssh, ssw, ssh);
            var bottom = new Rectangle(SourceArea.X + ssw, SourceArea.Y + (ssh * 2), ssw, ssh);
            var left = new Rectangle(SourceArea.X, SourceArea.Y + ssh, ssw, ssh);

            float tmw, lmh;

            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X + topleft.Width, bound.Y,
                    tmw = (bound.Width - topleft.Width - topright.Width), top.Height), top, color);
            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X + bound.Width - right.Width, bound.Y + topright.Height,
                    right.Width, lmh = (bound.Height - topright.Height - bottomright.Height)), right, color);
            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X + bottomleft.Width, bound.Y + bound.Height - bottom.Height,
                    bound.Width - bottomleft.Width - bottomright.Width, bottom.Height), bottom, color);
            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X, bound.Y + topleft.Height,
                    left.Width, bound.Height - topleft.Height - bottomleft.Height), left, color);

            var middle = new Rectangle(SourceArea.X + ssw, SourceArea.Y + ssh, ssw, ssh);

            renderer.Draw(Texture,
                new BoundingRectangle(
                    bound.X + left.Width, bound.Y + top.Height,
                    tmw, lmh), middle, color);
        }
    }
}
