#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// Defines commonly used statistics of the renderer.
    /// </summary>
    public class DrawingStatistics
    {
        public int VisibleLightCount { get; internal set; }
        public int VisibleObjectCount { get; internal set; }
        public int VisibleDrawableCount { get; internal set; }

        public int VertexCount { get; internal set; }
        public int PrimitiveCount { get; internal set; }

        internal DrawingStatistics()
        {
            Reset();
        }

        internal void Reset()
        {
            VisibleLightCount = 0;
            VisibleDrawableCount = 0;
            VisibleObjectCount = 0;
            VertexCount = 0;
            PrimitiveCount = 0;
        }

        internal void Draw(SpriteBatch spriteBatch, SpriteFont font, Color color)
        {
            var height = font.MeasureString("X").Y;
            Vector2 position = new Vector2(50, 50);

            foreach (var property in GetType().GetProperties())
            {
                string text = string.Format("{0}: {1}", property.Name, property.GetValue(this, null).ToString());
                spriteBatch.DrawString(font, text, position + Vector2.One, Color.Black);
                spriteBatch.DrawString(font, text, position, color);
                position += new Vector2(0, height + 5);
            }
        }
    }
}