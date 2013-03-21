namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [Nine.Serialization.NotBinarySerializable]
    partial class AntiAliasMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            var graphics = context.graphics;
            var viewport = graphics.Viewport;
            var pixelSize = new Vector4();
            pixelSize.X = 1.0f / viewport.Width;
            pixelSize.Y = 1.0f / viewport.Height;
            pixelSize.Z = -pixelSize.X;
            pixelSize.W = pixelSize.Y;
            effect.PixelSize.SetValue(pixelSize);

            graphics.Textures[0] = texture;
            if (texture != null)
            {
                graphics.SamplerStates[0] = texture.Format == SurfaceFormat.Color ?
                    SamplerState.LinearClamp : SamplerState.PointClamp;
            }
        }
    }
}
