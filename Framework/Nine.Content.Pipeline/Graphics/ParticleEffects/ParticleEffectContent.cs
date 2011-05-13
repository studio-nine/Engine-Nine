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
    /// A base class for any LinkedEffectPart that can be Linked together.
    /// </summary>
    [ContentSerializerRuntimeType("Nine.Graphics.ParticleEffects.ParticleEffect, Nine")]
    public class ParticleEffectContent
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
        public int MaxParticles { get; set; }

        /// <summary>
        /// Gets or sets the type of each particle.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public ParticleType ParticleType { get; set; }

        /// <summary>
        /// Gets or sets whether this particle effect is enabled.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the lifetime or duration of this particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public float Lifetime { get; set; }

        /// <summary>
        /// Gets or sets the number of particles emitted per second.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public float Emission { get; set; }

        /// <summary>
        /// Gets or sets the duration of each particle.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<float> Duration { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start color and alpha. 
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<Color> Color { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start size.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<float> Size { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start rotation.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<float> Rotation { get; set; }

        /// <summary>
        /// Gets or sets the range of values controlling the particle start speed.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Range<float> Speed { get; set; }

        /// <summary>
        /// Gets or sets a scale factor along the forward axis when drawing this
        /// particle effect using constrained billboard.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public float Stretch { get; set; }

        /// <summary>
        /// Gets or sets the up axis of each particle.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Vector3 Up { get; set; }

        /// <summary>
        /// Gets or sets the texture used by this particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public ContentReference<Texture2DContent> Texture { get; set; }

        /// <summary>
        /// Gets or sets the source rectangle in the texture.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Rectangle? SourceRectangle { get; set; }

        /// <summary>
        /// Gets or sets the blend state used by this particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets the emitter of this particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public IParticleEmitter Emitter { get; set; }

        /// <summary>
        /// Gets a collection of controllers that defines the visual of this particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<IParticleController> Controllers { get; private set; }

        /// <summary>
        /// Gets a collection of particle effects that is used as the appareance of each
        /// particle spawned by this particle effect.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<ParticleEffectContent> ChildEffects { get; private set; }

        /// <summary>
        /// Gets a collection of particle effects that is fired when each particle spawned
        /// by this particle effect is about to die.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<ParticleEffectContent> EndingEffects { get; private set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        [ContentSerializer(Optional = true, SharedResource = true)]
        public object Tag { get; set; }

        /// <summary>
        /// Creates a new instance of ParticleEffectContent.
        /// </summary>
        public ParticleEffectContent()
        {
            Up = Vector3.UnitZ;
            TriggerOnStartup = true;
            MaxParticles = 1024;
            Lifetime = (float)(TimeSpan.MaxValue.TotalSeconds * 0.5);
            Enabled = true;
            Stretch = 1;
            Duration = 2;
            Color = Microsoft.Xna.Framework.Color.White;
            BlendState = BlendState.Additive;
            Emitter = new PointEmitter();
            Controllers = new List<IParticleController>();
            ChildEffects = new List<ParticleEffectContent>();
            EndingEffects = new List<ParticleEffectContent>();
        }
    }


    [ContentTypeWriter]
    internal class ParticleEffectContentWriter : ContentTypeWriter<ParticleEffectContent>
    {
        protected override void Write(ContentWriter output, ParticleEffectContent value)
        {
            output.Write(value.TriggerOnStartup);
            output.Write(value.MaxParticles);
            output.Write((byte)value.ParticleType);
            output.Write(value.Lifetime);
            output.WriteObject(value.Texture);
            output.WriteObject(value.BlendState);
            output.WriteObject(value.Color);
            output.WriteObject(value.Duration);
            output.Write(value.Emission);
            output.WriteObject(value.Emitter);
            output.Write(value.Enabled);
            output.WriteObject(value.Rotation);
            output.WriteObject(value.Size);
            output.WriteObject(value.SourceRectangle);
            output.WriteObject(value.Speed);
            output.Write(value.Stretch);
            output.Write(value.Up);
            output.WriteObject(value.Tag);

            output.Write(value.Controllers.Count);
            foreach (IParticleController controller in value.Controllers)
                output.WriteObject(controller);

            output.Write(value.ChildEffects.Count);
            foreach (ParticleEffectContent effect in value.ChildEffects)
                output.WriteObject(effect);

            output.Write(value.EndingEffects.Count);
            foreach (ParticleEffectContent effect in value.EndingEffects)
                output.WriteObject(effect);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Nine.Graphics.ParticleEffects.ParticleEffect).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(Nine.Graphics.ParticleEffects.ParticleEffectReader).AssemblyQualifiedName;
        }
    }
}
