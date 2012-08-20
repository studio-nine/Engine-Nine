namespace Nine.Content.Pipeline.Graphics
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;

    [ContentTypeWriter]
    class BlendStateWriter : ContentTypeWriter<BlendState>
    {
        protected override void Write(ContentWriter output, BlendState value)
        {
            output.Write((byte)value.AlphaBlendFunction);
            output.Write((byte)value.AlphaDestinationBlend);
            output.Write((byte)value.AlphaSourceBlend);
            output.Write(value.BlendFactor);
            output.Write((byte)value.ColorBlendFunction);
            output.Write((byte)value.ColorDestinationBlend);
            output.Write((byte)value.ColorSourceBlend);
            output.Write((byte)value.ColorWriteChannels);
            output.Write(value.MultiSampleMask);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(BlendState).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(Nine.Graphics.BlendStateReader).AssemblyQualifiedName;
        }
    }
}
