// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

// extern alias Silverlight;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;

namespace Nine.Content.Pipeline.Silverlight
{
    [ContentTypeWriter]
    public class SilverlightEffectWriter : ContentTypeWriter<EffectBinary>
    {
        protected override void Write(ContentWriter output, EffectBinary value)
        {
            output.Write(value.GetEffectCode());
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return "Microsoft.Xna.Framework.Content.SilverlightEffectReader, Nine.Graphics, Version=1.6.0.0, Culture=neutral, PublicKeyToken=ed8336b5652212a9";
        }
    }
}
