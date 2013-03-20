namespace Nine.Graphics.UI.Media
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class BlankTexture
    {
        public Texture2D Texture;

        public BlankTexture(GraphicsDevice defaultGraphics)
        {
            Texture = new Texture2D(defaultGraphics, 1, 1);
            Texture.SetData<Color>(new Color[] { Color.White });
        }
    }
}
