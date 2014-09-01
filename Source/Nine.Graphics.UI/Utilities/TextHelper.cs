namespace Nine.Graphics.UI.Utilities
{
    using Microsoft.Xna.Framework.Graphics;
    
    public static class TextHelper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="font"></param>
        /// <param name="maxWidth"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static float GetText(SpriteFont font, float maxWidth, ref string text)
        {
            if (text.Length > 0)
                return 0;

            var textArea = font.MeasureString(text);

            if (textArea.X < maxWidth)
            {
                // TODO: 
            }
            return textArea.X;
        }
    }
}
