#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Windows.Markup;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Content.Pipeline.Graphics.ParticleEffects
{

    /// <summary>
    /// Defines a merged particle effect. The is equivalent to ParticleEffect.CreateMerged.
    /// </summary>
    [ContentProperty("ParticleEffects")]
    [ContentSerializerRuntimeType("Nine.Graphics.ParticleEffects.ParticleEffect, Nine.Graphics")]
    public class MergedParticleEffectContent
    {
        /// <summary>
        /// Gets a collection of particle effects to be merged.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<ParticleEffectContent> ParticleEffects { get; private set; }

        /// <summary>
        /// Creates a new instance of ParticleEffectContent.
        /// </summary>
        public MergedParticleEffectContent()
        {
            ParticleEffects = new List<ParticleEffectContent>();
        }
    }


    [ContentTypeWriter]
    class MergedParticleEffectContentWriter : ContentTypeWriter<MergedParticleEffectContent>
    {
        ParticleEffectContentWriter writer = new ParticleEffectContentWriter();

        protected override void Write(ContentWriter output, MergedParticleEffectContent value)
        {
            var mergedParticleEffect = value as MergedParticleEffectContent;
            if (mergedParticleEffect == null)
                throw new InvalidOperationException("value");

            var particleEffect = new ParticleEffectContent();
            particleEffect.Emitter = new PointEmitterContent { EmitCount = 1, Duration = float.MaxValue };
            particleEffect.ChildEffects.AddRange(mergedParticleEffect.ParticleEffects);

            output.WriteRawObject<ParticleEffectContent>(particleEffect, writer);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return writer.GetRuntimeReader(targetPlatform);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return writer.GetRuntimeType(targetPlatform);
        }
    }
}
