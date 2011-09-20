#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Xml;
using System.Text;
using System.Security.Cryptography;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Nine.Content.Pipeline.Processors;
#endregion

namespace Nine.Content.Pipeline.Graphics.Effects
{
    /// <summary>
    /// Enables a centralized place where LinkedEffect can be compiled and cached.
    /// Use this library to eliminated duplicated LinkedEffects.
    /// </summary>
    public static class LinkedEffectBuilder
    {
        public const string OutputDirectory = @"Effects\Library";

        /// <summary>
        /// Builds the specified linked effect.
        /// </summary>
        /// <param name="linkedEffect">The linked effect.</param>
        /// <param name="context">The context.</param>
        /// <returns>The asset name of the target file.</returns>
        public static ExternalReference<LinkedEffectContent> Build(LinkedEffectContent linkedEffect, ContentProcessorContext context)
        {
            if (context.TargetPlatform == TargetPlatform.WindowsPhone)
            {
                context.Logger.LogWarning(null, null, Strings.LinkedEffectNotSupported);
                return new ExternalReference<LinkedEffectContent>(Strings.LinkedEffectNotSupported);
            }

            // Process the effect
            new LinkedEffectProcessor().Process(linkedEffect, context);

            // Computes the hash of the input effect
            var sb = new StringBuilder();
            var hash = linkedEffect.Token;
            for (int i = 0; i < hash.Length; i++)
            {
                sb.Append(hash[i].ToString("X2"));
            }
            var name = sb.ToString().ToLower();
            var targetFile = Path.Combine(OutputDirectory, name);

            // Return if the file has already been build.
            if (File.Exists(Path.Combine(context.OutputDirectory, targetFile)))
                return new ExternalReference<LinkedEffectContent>(targetFile);

            var sourceFile = Path.Combine(context.IntermediateDirectory, name + ".xml");

            // There's currently no way to build from object, so we need to create a temperory file
            using (XmlWriter writer = XmlWriter.Create(sourceFile))
            {
                IntermediateSerializer.Serialize(writer, linkedEffect, ".");
            }

            // Build the source asset
            return context.BuildAsset<LinkedEffectContent, LinkedEffectContent>(linkedEffect, "LinkedEffectProcessor", null, targetFile);
        }
    }
}
