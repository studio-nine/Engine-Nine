namespace Nine.Graphics.UI
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.UI.Internal;

    public static class SpriteBatchExtensions
    {
        public static void Draw(this SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            var texture = GraphicsResources<BlankTexture>.GetInstance(spriteBatch.GraphicsDevice);
            spriteBatch.Draw(texture.Texture, rect, color);
        }
    }
}
