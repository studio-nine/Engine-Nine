namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class DepthOfFieldMaterial
    {
        public float FocalLength { get; set; }
        public float FocalPlane { get; set; }
        public float FocalDistance { get; set; }

        partial void OnCreated()
        {
            FocalDistance = 0.5f;
        }

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            
        }

        partial void BeginApplyLocalParameters(DrawingContext context, DepthOfFieldMaterial previousMaterial)
        {
            effect.FocalDistance.SetValue(FocalDistance);
            effect.FocalLength.SetValue(FocalLength);
            effect.FocalPlane.SetValue(FocalPlane);
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            GraphicsDevice.Textures[1] = null;
            GraphicsDevice.Textures[2] = null;
        }

        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            if (textureUsage == TextureUsage.Blur)
                effect.BlurTexture.SetValue(texture);
            else if (textureUsage == TextureUsage.DepthBuffer)
                effect.DepthTexture.SetValue(texture);
        }
    }
}