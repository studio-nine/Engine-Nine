namespace Nine.Graphics.UI
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Internal;

    // I don't know about you, but I want something like this to be public. 
    // (Instead of internal then) Should this be with BoundingRectangle?
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BoundingRectangleExtensions
    {
        public static BoundingRectangle Deflate(this BoundingRectangle rect, float left, float top)
        {
            return rect.Deflate(new Thickness(left, top));
        }

        public static BoundingRectangle Deflate(this BoundingRectangle rect, Thickness thickness)
        {
            var sizeDeflated = rect.Size.Deflate(thickness);
            return new BoundingRectangle(
                rect.X + thickness.Left, rect.Y + thickness.Top, sizeDeflated.X, sizeDeflated.Y);
        }
    }
}
