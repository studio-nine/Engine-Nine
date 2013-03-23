namespace Nine.Graphics.UI.Media
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Paints an area with an image.
    /// </summary>
    public class ImageBrush : TileBrush
    {
        public Texture2D Source;

        public ImageBrush() { }
        public ImageBrush(Texture2D Source)
        {
            this.Source = Source;
        }
    }
}
