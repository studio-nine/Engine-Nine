namespace Nine.Graphics.UI.Internal
{
    using Microsoft.Xna.Framework;
    using System;

    internal static class BoundingRectangleExtensions
    {
        public static BoundingRectangle Deflate(this BoundingRectangle rect, Thickness thickness)
        {
            var sizeDeflated = rect.Size.Deflate(thickness);
            return new BoundingRectangle(
                rect.X + thickness.Left, rect.Y + thickness.Top, sizeDeflated.X, sizeDeflated.Y);
        }
    }
}
