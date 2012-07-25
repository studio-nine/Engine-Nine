namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ScaleMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            var halfPixel = new Vector2();
            var viewport = context.GraphicsDevice.Viewport;
            halfPixel.X = 0.5f / viewport.Width;
            halfPixel.Y = 0.5f / viewport.Height;

            effect.HalfPixel.SetValue(halfPixel);
        }
    }
}