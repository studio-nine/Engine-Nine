#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Content.Pipeline.Graphics
{    
    [ContentTypeSerializer]
    class BlendStateSerializer : ContentTypeSerializer<BlendState>
    {
        protected override BlendState Deserialize(IntermediateReader input, ContentSerializerAttribute format, BlendState existingInstance)
        {
            string value = input.Xml.ReadContentAsString();
            if (value == "Additive")
                return BlendState.Additive;
            if (value == "AlphaBlend")
                return BlendState.AlphaBlend;
            if (value == "NonPremultiplied")
                return BlendState.NonPremultiplied;
            if (value == "Opaque")
                return BlendState.Opaque;

            throw new InvalidContentException("Unknown BlendState: " + value);
        }

        protected override void Serialize(IntermediateWriter output, BlendState value, ContentSerializerAttribute format)
        {
            if (BlendStateEquals(value, BlendState.Additive))
                output.Xml.WriteString("Additive");
            else if (BlendStateEquals(value, BlendState.AlphaBlend))
                output.Xml.WriteString("AlphaBlend");
            else if (BlendStateEquals(value, BlendState.NonPremultiplied))
                output.Xml.WriteString("NonPremultiplied");
            else if (BlendStateEquals(value, BlendState.Opaque))
                output.Xml.WriteString("Opaque");
            else
                throw new InvalidContentException("Unknown BlendState: " + value);
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

    [ContentTypeWriter]
    internal class BlendStateWriter : ContentTypeWriter<BlendState>
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
            output.Write((byte)value.ColorWriteChannels1);
            output.Write((byte)value.ColorWriteChannels2);
            output.Write((byte)value.ColorWriteChannels3);
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
