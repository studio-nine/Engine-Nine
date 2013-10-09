namespace Nine.Graphics.UI
{
    using System;
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

        public static void Draw(this SpriteBatch spriteBatch, Vector2 position, Vector2 scale, Color color)
        {
            Draw(spriteBatch, position, scale, 0, color);
        }

        public static void Draw(this SpriteBatch spriteBatch, Vector2 position, Vector2 scale, float rotation, Color color)
        {
            var texture = GraphicsResources<BlankTexture>.GetInstance(spriteBatch.GraphicsDevice);
            spriteBatch.Draw(texture.Texture, position, null, color, rotation, Vector2.Zero, scale, SpriteEffects.None, 0);
        }

        public static void DrawLine(this SpriteBatch spriteBatch, Vector2 from, Vector2 to, Color color, float thickness)
        {
            float distance = Vector2.Distance(from, to);
            float angle = (float)Math.Atan2(to.Y - from.Y, to.X - from.X);
            Draw(spriteBatch, from, new Vector2(distance, thickness), angle, color);
        }

        public static void DrawRectangle(this SpriteBatch spriteBatch, BoundingRectangle rect, Color color, float thickness)
        {
            DrawLine(spriteBatch, new Vector2(rect.X, rect.Y), new Vector2(rect.X + rect.Width, rect.Y), color, thickness);
            DrawLine(spriteBatch, new Vector2(rect.X + rect.Width, rect.Y), new Vector2(rect.X + rect.Width, rect.Y + rect.Height), color, thickness);
            DrawLine(spriteBatch, new Vector2(rect.X + rect.Width, rect.Y + rect.Height), new Vector2(rect.X, rect.Y + rect.Height), color, thickness);
            DrawLine(spriteBatch, new Vector2(rect.X, rect.Y + rect.Height), new Vector2(rect.X, rect.Y), color, thickness);
        }
    }
}
