// extern alias Silverlight;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Nine.Content.Pipeline.Silverlight
{
    [ContentTypeWriter]
    class SilverlightEffectWriter : ContentTypeWriter<EffectBinaryContent>
    {
        protected override void Write(ContentWriter output, EffectBinaryContent value)
        {
            output.WriteRawObject(value.GetEffectCode());
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // Key signing and version updates can break static strings like below because the SN changes.
            return "Microsoft.Xna.Framework.Content.SilverlightEffectReader, Nine, Version=1.0.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";

            // We could reference the SL assemblies and do this, but that would mean updating the templates with
            // correct references as well, which is tricky.
            // return typeof(Silverlight::Microsoft.Xna.Framework.Content.SilverlightEffectReader).AssemblyQualifiedName;
        }
    }
}
