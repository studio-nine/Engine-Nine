#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
#endregion

namespace Nine.Content.Pipeline.Graphics.ParticleEffects
{
    using Nine.Graphics.ParticleEffects;

    /// <summary>
    /// Defines a merged particle effect. The is equivalent to ParticleEffect.CreateMerged.
    /// </summary>
    [ContentSerializerRuntimeType("Nine.Graphics.ParticleEffects.ParticleEffect, Nine.Graphics")]
    public class MergedParticleEffectContent
    {
        /// <summary>
        /// Gets or sets whether the particle system will be triggered
        /// automatically after been created.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool TriggerOnStartup { get; set; }

        /// <summary>
        /// Gets or sets the max particle count of the particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public int MaxParticleCount { get; set; }

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
            TriggerOnStartup = false;
            MaxParticleCount = 16;
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
            particleEffect.Emission = 1;
            particleEffect.TriggerCount = 1;
            particleEffect.Duration = float.MaxValue;
            particleEffect.MaxParticleCount = mergedParticleEffect.MaxParticleCount;
            particleEffect.TriggerOnStartup = mergedParticleEffect.TriggerOnStartup;
            particleEffect.ChildEffects.AddRange(mergedParticleEffect.ParticleEffects);

            writer.InternalWrite(output, particleEffect);
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
