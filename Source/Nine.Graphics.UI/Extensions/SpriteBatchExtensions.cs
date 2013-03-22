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

        public static void Draw(this SpriteBatch spriteBatch, Rectangle rect, Brush c)
        {
            // Optimize, Tho I would say this is a temporarily way of drawing
            switch (c.GetType().Name)
            {
                case "SolidColorBrush":
                    var Texture = Nine.Graphics.GraphicsResources<BlankTexture>.GetInstance(spriteBatch.GraphicsDevice);
                    spriteBatch.Draw(Texture.Texture, rect, (c as SolidColorBrush).Color);
                    break;

                case "ImageBrush":
                    spriteBatch.Draw((c as ImageBrush).Source, rect, Color.White);
                    break;


                case "Brush":
                case "TileBrush":
                    throw new System.NotSupportedException();
            }
        }
    }
}
