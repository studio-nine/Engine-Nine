#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Content model for CustomEffect.
    /// </summary>
    [ContentSerializerRuntimeType("Nine.Graphics.Effects.CustomEffect, Nine.Graphics")]
    public class CustomEffectContent
    {
        /// <summary>
        /// Gets or sets the effect byte code.
        /// </summary>
        [ContentSerializer]
        public byte[] EffectCode { get; set; }

        /// <summary>
        /// Gets or sets all the parameters of this custom effect.
        /// </summary>
        [ContentSerializer]
        public Dictionary<string, object> Parameters { get; set; }
    }
    
    [ContentTypeWriter]
    class CustomEffectContentWriter : ContentTypeWriter<CustomEffectContent>
    {
        protected override void Write(ContentWriter output, CustomEffectContent value)
        {
            output.Write(value.EffectCode.Length);
            output.Write(value.EffectCode);
            output.WriteObject(value.Parameters);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            return typeof(Nine.Graphics.Effects.CustomEffect).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            return typeof(Nine.Graphics.Effects.CustomEffectReader).AssemblyQualifiedName;
        }
    }
}
