namespace Nine.Graphics.UI.Media
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    // TODO: Rename to something better?

    /// <summary>
    /// This brush separate a texture into 9 parts and render them in a layout.
    /// </summary>
    /// <remarks>
    ///      __ ____ __
    ///     |__|____|__|
    ///     |__|____|__|
    ///     |__|____|__|
    ///     
    /// </remarks>
    public class SpecialBrush : Brush
    {
        public Texture2D Texture { get; set; }

        [TypeConverter(typeof(Nine.Design.RectangleConverter))]
        public Rectangle SourceArea 
        {
            get { return sourceArea; }
            set 
            {
                needsRebuild = true;
                sourceArea = value;
            } 
        }
        private Rectangle sourceArea;

        private bool needsRebuild;
        private BoundingRectangle prevBound;
        private Sprite[] rectangles = new Sprite[9];

        void Build(BoundingRectangle bound)
        {
            int ssw = (int)(SourceArea.Width / 3);
            int ssh = (int)(SourceArea.Height / 3);

            var topleft = new Rectangle(SourceArea.X, SourceArea.Y, ssw, ssh);
            var topright = new Rectangle(SourceArea.X + (ssw * 2), SourceArea.Y, ssw, ssh);
            var bottomleft = new Rectangle(SourceArea.X, SourceArea.Y + (ssh * 2), ssw, ssh);
            var bottomright = new Rectangle(SourceArea.X + (ssw * 2), SourceArea.Y + (ssh * 2), ssw, ssh);

            rectangles[0] = new Sprite { 
                Location = new BoundingRectangle(bound.X, bound.Y, topleft.Width, topleft.Height),
                Source = topleft
            };
            rectangles[1] = new Sprite {
                Location = new BoundingRectangle(bound.X + bound.Width - topright.Width, bound.Y, topright.Width, topright.Height),
                Source = topright
            };
            rectangles[2] = new Sprite {
                Location = new BoundingRectangle(bound.X, bound.Y + bound.Height - bottomleft.Height, bottomleft.Width, bottomleft.Height),
                Source = bottomleft
            };
            rectangles[3] = new Sprite {
                Location = new BoundingRectangle(bound.X + bound.Width - bottomright.Width, bound.Y + bound.Height - bottomright.Height, bottomright.Width, bottomright.Height),
                Source = bottomright
            };

            var top = new Rectangle(SourceArea.X + ssw, SourceArea.Y, ssw, ssh);
            var right = new Rectangle(SourceArea.X + (ssw * 2), SourceArea.Y + ssh, ssw, ssh);
            var bottom = new Rectangle(SourceArea.X + ssw, SourceArea.Y + (ssh * 2), ssw, ssh);
            var left = new Rectangle(SourceArea.X, SourceArea.Y + ssh, ssw, ssh);

            float tmw = bound.Width - topleft.Width - topright.Width, 
                  lmh = bound.Height - topright.Height - bottomright.Height;

            rectangles[4] = new Sprite { 
                Location = new BoundingRectangle(bound.X + topleft.Width, bound.Y, tmw, top.Height),
                Source = top
            };
            rectangles[5] = new Sprite { 
                Location = new BoundingRectangle(bound.X + bound.Width - right.Width, bound.Y + topright.Height, right.Width, lmh),
                Source = right
            };
            rectangles[6] = new Sprite { 
                Location = new BoundingRectangle(bound.X + bottomleft.Width, bound.Y + bound.Height - bottom.Height, bound.Width - bottomleft.Width - bottomright.Width, bottom.Height),
                Source = bottom
            };
            rectangles[7] = new Sprite { 
                Location = new BoundingRectangle(bound.X, bound.Y + topleft.Height, left.Width, bound.Height - topleft.Height - bottomleft.Height),
                Source = left
            };
            var middle = new Rectangle(SourceArea.X + ssw, SourceArea.Y + ssh, ssw, ssh);

            rectangles[8] = new Sprite {
                Location = new BoundingRectangle(bound.X + left.Width, bound.Y + top.Height, tmw, lmh),
                Source = middle
            };
        }

        protected internal override void OnRender(Renderer.Renderer renderer, BoundingRectangle bound)
        {
            if (prevBound != null && prevBound != bound)
                needsRebuild = true;
            prevBound = bound;

            if (needsRebuild)
            {
                Build(bound);
                needsRebuild = false;
            }

            foreach (var sprite in rectangles)
            {
                renderer.Draw(Texture, sprite.Location, sprite.Source, Color.White);
            }
        }

        struct Sprite
        {
            public BoundingRectangle Location;
            public Rectangle Source;
        }
    }
}
