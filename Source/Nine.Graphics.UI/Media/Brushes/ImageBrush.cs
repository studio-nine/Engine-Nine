namespace Nine.Graphics.UI.Media
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.AttachedProperty;

    /// <summary>
    /// A Brush that draws a Image.
    /// </summary>
    [ContentProperty("Source")]
    public sealed class ImageBrush : TileBrush
    {
        /// <summary>
        /// Gets or sets the represented image.
        /// </summary>
        public Texture2D Source { get; set; }

        /// <summary>
        /// Initializes a new instance without Source set.
        /// </summary>
        public ImageBrush() { }

        /// <summary>
        /// Initializes a new instance with Source set.
        /// </summary>
        /// <param name="Source"></param>
        public ImageBrush(Texture2D Source)
        {
            this.Source = Source;
        }

        protected internal override void OnRender(Renderer.Renderer renderer, BoundingRectangle bound)
        {
            // TODO: Image Size Calculation

            SpriteEffects Effects = SpriteEffects.None;
            if (Flip != Flip.None & Flip != Flip.Both)
                Effects = (SpriteEffects)Flip;
            else if (Flip == Flip.Both)
                Effects = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;

            renderer.Draw(Source, bound, SourceRectangle, Color.White);
        }
    }
}
