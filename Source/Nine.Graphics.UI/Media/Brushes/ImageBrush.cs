namespace Nine.Graphics.UI.Media
{
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Paints an area with an image.
    /// </summary>
    [ContentProperty("Source")]
    public class ImageBrush : TileBrush
    {
        public Texture2D Source { get; set; }

        public ImageBrush() { }
        public ImageBrush(Texture2D Source)
        {
            this.Source = Source;
        }
    }
}
