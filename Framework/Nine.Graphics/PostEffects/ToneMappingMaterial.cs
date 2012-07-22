namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Nine.Graphics.Drawing;
    using Microsoft.Xna.Framework.Graphics;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ToneMappingMaterial
    {
        public float Exposure { get; set; }
        public float MaxLuminance { get; set; }

        partial void OnCreated()
        {
            Exposure = 0.6f;
            MaxLuminance = 16;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, ToneMappingMaterial previousMaterial)
        {
            effect.Exposure.SetValue(Exposure);
            effect.MaxLuminanceSq.SetValue(MaxLuminance * MaxLuminance);
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