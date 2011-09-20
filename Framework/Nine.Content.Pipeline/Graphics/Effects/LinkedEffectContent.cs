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
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// A base class for any LinkedEffectPart that can be Linked together.
    /// </summary>
    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public abstract class LinkedEffectPartContent
    {
        /// <summary>
        /// Gets the EffectParts from the parent LinkedEffectContent.
        /// </summary>
        protected internal IList<LinkedEffectPartContent> EffectParts { get; internal set; }

        /// <summary>
        /// Validates this LinkedEffectPartContent with the context.
        /// </summary>
        protected internal virtual void Validate(ContentProcessorContext context)
        {

        }

        /// <summary>
        /// Gets whether the EffectParts contains a LinkedEffectPartContent of the
        /// specified type.
        /// </summary>
        public bool Contains(Type type)
        {
            foreach (LinkedEffectPartContent part in EffectParts)
                if (type.IsAssignableFrom(part.GetType()))
                    return true;

            return false;
        }

        /// <summary>
        /// Gets the code fragment for this LinkedEffectPartContent.
        /// </summary>
        public abstract string Code { get; }
    }

    /// <summary>
    /// Represents a LinkedEffectPartContent loaded from file.
    /// </summary>
    [ContentSerializerRuntimeType("Nine.Graphics.Effects.LinkedEffectPart, Nine.Graphics")]
    public class LinkedEffectPartFileContent : LinkedEffectPartContent
    {
        /// <summary>
        /// Gets or sets the path of the file.
        /// </summary>
        [ContentSerializerIgnore]
        public string Path { get; set; }

        public override string Code { get { return System.IO.File.ReadAllText(Path); } }
    }

    /// <summary>
    /// Content type for a LinkedEffect.
    /// </summary>
    public class LinkedEffectContent
    {
        internal byte[] Token;
        internal byte[] EffectCode;
        internal string[] UniqueNames;

        public LinkedEffectContent()
        {
            EffectParts = new List<LinkedEffectPartContent>();
        }

        public LinkedEffectContent(IEnumerable<LinkedEffectPartContent> parts)
        {
            EffectParts = new List<LinkedEffectPartContent>(parts);
        }

        /// <summary>
        /// Gets all the content representations for LinkedEffectParts.
        /// </summary>
        [ContentSerializer]
        public List<LinkedEffectPartContent> EffectParts { get; private set; }
    }

    /// <summary>
    /// ContentTypeWriter for LinkedEffectContent.
    /// </summary>
    [ContentTypeWriter]
    internal class LinkedEffectContentWriter : ContentTypeWriter<LinkedEffectContent>
    {
        protected override void Write(ContentWriter output, LinkedEffectContent value)
        {
            // Windows Phone doesn't support custom shaders.
            if (output.TargetPlatform == TargetPlatform.WindowsPhone)
                return;

            if (value.EffectCode == null || value.UniqueNames == null || value.Token == null)
                throw new InvalidContentException("LinkedEffectContent must be processed with LinkedEffectProcessor.");

            output.WriteObject<byte[]>(value.Token);
            output.WriteObject<byte[]>(value.EffectCode);
            output.WriteObject<string[]>(value.UniqueNames);
            output.Write(value.EffectParts.Count);

            foreach (LinkedEffectPartContent part in value.EffectParts)
                output.WriteObject<LinkedEffectPartContent>(part);
        }

        public override string GetRuntimeType(TargetPlatform targetPlatform)
        {
            // Set this to avoid the complaining from the content pipeline,
            // so the build process won't fall if you happened to include
            // this in a Windows Phone project.
            if (targetPlatform == TargetPlatform.WindowsPhone)
                return typeof(System.String).AssemblyQualifiedName;

            return typeof(Nine.Graphics.Effects.LinkedEffect).AssemblyQualifiedName;
        }

        public override string GetRuntimeReader(TargetPlatform targetPlatform)
        {
            // Set this to avoid the complaining from the content pipeline,
            // so the build process won't fall if you happened to include
            // this in a Windows Phone project.
            if (targetPlatform == TargetPlatform.WindowsPhone)
                return typeof(System.String).AssemblyQualifiedName;

            return typeof(Nine.Graphics.Effects.LinkedEffectReader).AssemblyQualifiedName;
        }
    }
}
