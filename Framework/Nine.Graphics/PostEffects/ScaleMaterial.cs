namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [NotContentSerializable]
    partial class ScaleMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            if ((GraphicsDevice.Textures[0] = texture) != null)
            {
                var halfPixel = new Vector2();
                halfPixel.X = 0.5f / texture.Width;
                halfPixel.Y = 0.5f / texture.Height;
                effect.HalfTexel.SetValue(halfPixel);
            }
        }
    }
}