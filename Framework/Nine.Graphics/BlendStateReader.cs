namespace Nine.Graphics
{
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;

    class BlendStateReader : ContentTypeReader<BlendState>
    {
        /// <summary>
        /// Reads the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <param name="existingInstance">The existing instance.</param>
        /// <returns></returns>
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
#if !SILVERLIGHT
            existingInstance.ColorWriteChannels1 = (ColorWriteChannels)input.ReadByte();
            existingInstance.ColorWriteChannels2 = (ColorWriteChannels)input.ReadByte();
            existingInstance.ColorWriteChannels3 = (ColorWriteChannels)input.ReadByte();
#endif
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
                   a.ColorWriteChannels1 == b.ColorWriteChannels1 &&
                   a.ColorWriteChannels2 == b.ColorWriteChannels2 &&
                   a.ColorWriteChannels3 == b.ColorWriteChannels3 &&
                   a.MultiSampleMask == b.MultiSampleMask;
        }
    }
}
