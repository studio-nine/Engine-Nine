namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    partial class SpriteMaterial
    {
        partial void ApplyGlobalParameters(Drawing.DrawingContext context)
        {
            var pixelSizeAndViewport = new Vector4();
            var viewport = context.graphics.Viewport;
            
            pixelSizeAndViewport.X = -1f / viewport.Width;
            pixelSizeAndViewport.Y = 1f / viewport.Height;

            pixelSizeAndViewport.Z = -pixelSizeAndViewport.X;
            pixelSizeAndViewport.W = pixelSizeAndViewport.Y;

            // After projection transform, viewport transform will map vertices
            // from [-1, 1] to [0, 1], this mapping will scale the offset by 2,
            // so we need to offset the input quad by a full pixel.
            effect.PixelSizeAndViewport.SetValue(pixelSizeAndViewport);
        }
    }
}