namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ToneMappingMaterial
    {
        public float Exposure { get; set; }

        partial void OnCreated()
        {
            Exposure = 0.5f;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, ToneMappingMaterial previousMaterial)
        {
            effect.Exposure.SetValue(Exposure);
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
            GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            GraphicsDevice.Textures[1] = null;
            GraphicsDevice.Textures[2] = null;
        }

        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            if (textureUsage == TextureUsage.Luminance)
                effect.LuminanceTexture.SetValue(texture as Texture2D);
            else if (textureUsage == TextureUsage.Bloom)
                effect.BloomTexture.SetValue(texture as Texture2D);
        }
    }
}