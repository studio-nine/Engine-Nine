namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [Nine.Serialization.NotBinarySerializable]
    partial class BloomMaterial
    {
        public float BloomIntensity;

        partial void OnCreated()
        {
            BloomIntensity = 1;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, BloomMaterial previousMaterial)
        {
            effect.BloomIntensity.SetValue(BloomIntensity);

            GraphicsDevice.Textures[0] = texture;            
            GraphicsDevice.SamplerStates[0] =
            GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            GraphicsDevice.Textures[1] = null;
            GraphicsDevice.SamplerStates[1] = context.SamplerState;
        }

        public override void SetTexture(TextureUsage textureUsage, Texture texture)
        {
            if (textureUsage == TextureUsage.Bloom)
                GraphicsDevice.Textures[1] = texture;
        }
    }
}