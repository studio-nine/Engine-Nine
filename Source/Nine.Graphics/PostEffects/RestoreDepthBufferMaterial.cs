namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics;

    [Nine.Serialization.NotBinarySerializable]
    partial class RestoreDepthBufferMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            GraphicsDevice.Textures[0] = context.textures[Nine.Graphics.TextureUsage.DepthBuffer];
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }
    }
}