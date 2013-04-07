namespace Nine.Graphics.UI.Media
{
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// A Brush that draws a Image.
    /// </summary>
    [ContentProperty("Source")]
    public class ImageBrush : TileBrush
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
    }
}
