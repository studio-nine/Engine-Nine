namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [NotContentSerializable]
    partial class AntiAliasMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            var graphics = context.GraphicsDevice;
            var viewport = graphics.Viewport;
            var pixelSize = new Vector4();
            pixelSize.X = 1.0f / viewport.Width;
            pixelSize.Y = 1.0f / viewport.Height;
            pixelSize.Z = -pixelSize.X;
            pixelSize.W = pixelSize.Y;
            effect.PixelSize.SetValue(pixelSize);

            graphics.Textures[0] = texture;
            graphics.SamplerStates[0] = SamplerState.LinearClamp;
        }
    }
}
