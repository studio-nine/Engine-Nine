namespace Nine.Graphics
{
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    class BlendStateReader : ContentTypeReader<BlendState>
    {
        protected override BlendState Read(ContentReader input, BlendState existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new BlendState();

            existingInstance.AlphaBlendFunction = (BlendFunction)input.ReadByte();
            existingInstance.AlphaDestinationBlend = (Blend)input.ReadByte();
            existingInstance.AlphaSourceBlend = (Blend)input.ReadByte();
            existingInstance.BlendFactor = input.ReadColor();
            existingInstance.ColorBlendFunction = (BlendFunction)input.ReadByte();
            existingInstance.ColorDestinationBlend = (Blend)input.ReadByte();
            existingInstance.ColorSourceBlend = (Blend)input.ReadByte();
            existingInstance.ColorWriteChannels = (ColorWriteChannels)input.ReadByte();
            existingInstance.MultiSampleMask = input.ReadInt32();

            if (BlendStateEquals(existingInstance, BlendState.Opaque))
                return BlendState.Opaque;
            if (BlendStateEquals(existingInstance, BlendState.Additive))
                return BlendState.Additive;
            if (BlendStateEquals(existingInstance, BlendState.AlphaBlend))
                return BlendState.AlphaBlend;
            if (BlendStateEquals(existingInstance, BlendState.NonPremultiplied))
                return BlendState.NonPremultiplied;
            return existingInstance;
        }

        private static bool BlendStateEquals(BlendState a, BlendState b)
        {
            return a.AlphaBlendFunction == b.AlphaBlendFunction &&
                   a.AlphaDestinationBlend == b.AlphaDestinationBlend &&
                   a.AlphaSourceBlend == b.AlphaSourceBlend &&
                   a.BlendFactor == b.BlendFactor &&
                   a.ColorBlendFunction == b.ColorBlendFunction &&
                   a.ColorDestinationBlend == b.ColorDestinationBlend &&
                   a.ColorSourceBlend == b.ColorSourceBlend &&
                   a.ColorWriteChannels == b.ColorWriteChannels &&
                   a.MultiSampleMask == b.MultiSampleMask;
        }
    }
}
