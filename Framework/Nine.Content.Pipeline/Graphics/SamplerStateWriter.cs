namespace Nine.Content.Pipeline.Graphics
{    
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
    using Microsoft.Xna.Framework.Graphics;

    [ContentTypeWriter]
    class SamplerStateWriter : ContentTypeWriter<SamplerState>
    {
        protected override void Write(ContentWriter output, SamplerState value)
        {
            output.Write((byte)value.AddressU);
            output.Write((byte)value.AddressV);
            output.Write((byte)value.AddressW);
            output.Write((byte)value.Filter);
            output.Write((byte)value.MaxAnisotropy);
            output.Write((byte)value.MaxMipLevel);
            output.Write((byte)value.MipMapLevelOfDetailBias);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(SamplerState).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(Nine.Graphics.SamplerStateReader).AssemblyQualifiedName;
        }
    }
}
