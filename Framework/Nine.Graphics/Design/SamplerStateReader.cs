namespace Nine.Graphics
{
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    class SamplerStateReader : ContentTypeReader<SamplerState>
    {
        protected override SamplerState Read(ContentReader input, SamplerState existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new SamplerState();

            existingInstance.AddressU = (TextureAddressMode)input.ReadByte();
            existingInstance.AddressV = (TextureAddressMode)input.ReadByte();
            existingInstance.Filter = (TextureFilter)input.ReadByte();
            existingInstance.MaxAnisotropy = input.ReadByte();
            existingInstance.MaxMipLevel = input.ReadByte();
            existingInstance.MipMapLevelOfDetailBias = input.ReadByte();

            if (SamplerStateEquals(existingInstance, SamplerState.AnisotropicClamp))
                return SamplerState.AnisotropicClamp;
            if (SamplerStateEquals(existingInstance, SamplerState.AnisotropicWrap))
                return SamplerState.AnisotropicWrap;
            if (SamplerStateEquals(existingInstance, SamplerState.LinearClamp))
                return SamplerState.LinearClamp;
            if (SamplerStateEquals(existingInstance, SamplerState.LinearWrap))
                return SamplerState.LinearWrap;
            if (SamplerStateEquals(existingInstance, SamplerState.PointClamp))
                return SamplerState.PointClamp;
            if (SamplerStateEquals(existingInstance, SamplerState.PointWrap))
                return SamplerState.PointWrap;
            return existingInstance;
        }

        private static bool SamplerStateEquals(SamplerState a, SamplerState b)
        {
            return a.AddressU == b.AddressU &&
                   a.AddressV == b.AddressV &&
                   a.Filter == b.Filter &&
                   a.MaxAnisotropy == b.MaxAnisotropy &&
                   a.MaxMipLevel == b.MaxMipLevel &&
                   a.MipMapLevelOfDetailBias == b.MipMapLevelOfDetailBias;
        }
    }
}
