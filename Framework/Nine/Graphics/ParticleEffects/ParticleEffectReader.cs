#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    class ParticleEffectReader : ContentTypeReader<ParticleEffect>
    {
        protected override ParticleEffect Read(ContentReader input, ParticleEffect existingInstance)
        {
            ParticleEffect effect = new ParticleEffect(input.ReadBoolean(), input.ReadInt32());

            effect.ParticleType = (ParticleType)input.ReadByte();
            effect.Lifetime = TimeSpan.FromSeconds(input.ReadSingle());
            effect.Texture = input.ReadObject<Texture2D>();
            effect.BlendState = input.ReadObject<BlendState>();
            effect.Color = input.ReadObject<Range<Color>>();
            effect.Duration = input.ReadObject<Range<float>>();
            effect.Emission = input.ReadSingle();
            effect.Emitter = input.ReadObject<IParticleEmitter>();
            effect.Enabled = input.ReadBoolean();
            effect.Rotation = input.ReadObject<Range<float>>();
            effect.Size = input.ReadObject<Range<float>>();
            effect.SourceRectangle = input.ReadObject<Rectangle?>();
            effect.Speed = input.ReadObject<Range<float>>();
            effect.Stretch = input.ReadSingle();
            effect.Up = input.ReadVector3();
            effect.Tag = input.ReadObject<object>();

            int count = input.ReadInt32();
            for (int i = 0; i < count; i++)
                effect.Controllers.Add(input.ReadObject<IParticleController>());

            count = input.ReadInt32();
            for (int i = 0; i < count; i++)
                effect.SiblingEffects.Add(input.ReadObject<ParticleEffect>());

            count = input.ReadInt32();
            for (int i = 0; i < count; i++)
                effect.ChildEffects.Add(input.ReadObject<ParticleEffect>());

            count = input.ReadInt32();
            for (int i = 0; i < count; i++)
                effect.EndingEffects.Add(input.ReadObject<ParticleEffect>());

            return effect;
        }
    }
}
