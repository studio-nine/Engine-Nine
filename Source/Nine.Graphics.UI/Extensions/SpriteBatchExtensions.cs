namespace Nine.Graphics
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Media;

    public static class SpriteBatchExtensions
    {
        public static void Draw(this SpriteBatch spriteBatch, Rectangle rect, Color c)
        {
            var Texture = Nine.Graphics.GraphicsResources<BlankTexture>.GetInstance(spriteBatch.GraphicsDevice);
            spriteBatch.Draw(Texture.Texture, rect, c);
        }
    }
}
