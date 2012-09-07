namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    partial class VertexPassThrough2Material
    {
        partial void ApplyGlobalParameters(Drawing.DrawingContext context)
        {
            var pixelSize = new Vector2();
            var viewport = context.graphics.Viewport;
            pixelSize.X = -1f / viewport.Width;
            pixelSize.Y = 1f / viewport.Height;

            // After projection transform, viewport transform will map vertices
            // from [-1, 1] to [0, 1], this mapping will scale the offset by 2,
            // so we need to offset the input quad by a full pixel.
            effect.PixelSize.SetValue(pixelSize);
        }
    }

    partial class VertexPassThrough3Material
    {
        partial void ApplyGlobalParameters(Drawing.DrawingContext context)
        {
            var pixelSize = new Vector2();
            var viewport = context.graphics.Viewport;
            pixelSize.X = -1f / viewport.Width;
            pixelSize.Y = 1f / viewport.Height;

            // After projection transform, viewport transform will map vertices
            // from [-1, 1] to [0, 1], this mapping will scale the offset by 2,
            // so we need to offset the input quad by a full pixel.
            effect.PixelSize.SetValue(pixelSize);
        }
    }
}