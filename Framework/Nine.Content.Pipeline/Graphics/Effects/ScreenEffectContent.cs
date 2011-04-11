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
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.ScreenEffects;
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Content type for ScreenEffectPass.
    /// </summary>
    public class ScreenEffectPassContent
    {
        [ContentSerializer(Optional = true)]
        public BlendState BlendState { get; set; }

        [ContentSerializer(Optional = true)]
        public Color Color { get; set; }

        [ContentSerializer(Optional = true)]
        public bool DownScaleEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public bool FilteringEnabled { get; set; }

        [ContentSerializer(Optional = true)]
        public List<object> Effects { get; set; }

        [ContentSerializer(Optional = true)]
        public List<ScreenEffectPassContent> Passes { get; set; }

        public ScreenEffectPassContent()
        {
            BlendState = BlendState.Additive;
            Color = Color.White;
            DownScaleEnabled = false;
            FilteringEnabled = true;
            Effects = new List<object>();
            Passes = new List<ScreenEffectPassContent>();
        }
    }

    /// <summary>
    /// Content type for ScreenEffect.
    /// </summary>
    public class ScreenEffectContent : ScreenEffectPassContent
    {

    }

    [ContentTypeWriter]
    class ScreenEffectPassContentWriter : ContentTypeWriter<ScreenEffectPassContent>
    {
        protected override void Write(ContentWriter output, ScreenEffectPassContent value)
        {
            InternalWrite(output, value);
        }

        internal static void InternalWrite(ContentWriter output, ScreenEffectPassContent value)
        {
            output.WriteObject<BlendState>(value.BlendState);
            output.Write(value.Color);
            output.Write(value.DownScaleEnabled);
            output.Write(value.FilteringEnabled);
            if (value.Effects != null)
            {
                output.Write(value.Effects.Count);
                for (int i = 0; i < value.Effects.Count; i++)
                    output.WriteObject<object>(value.Effects[i]);
            }
            else
            {
                output.Write(0);
            }
            if (value.Passes != null)
            {
                output.Write(value.Passes.Count);
                for (int i = 0; i < value.Passes.Count; i++)
                    output.WriteObject<ScreenEffectPassContent>(value.Passes[i]);
            }
            else
            {
                output.Write(0);
            }
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(ScreenEffectPassReader).AssemblyQualifiedName;
        }
    }

    [ContentTypeWriter]
    class ScreenEffectContentWriter : ContentTypeWriter<ScreenEffectContent>
    {
        protected override void Write(ContentWriter output, ScreenEffectContent value)
        {
            ScreenEffectPassContentWriter.InternalWrite(output, value);
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(ScreenEffectReader).AssemblyQualifiedName;
        }
    }
}
