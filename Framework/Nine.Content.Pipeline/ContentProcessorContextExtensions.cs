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
using System.ComponentModel;
#endregion

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// Enables a centralized place where LinkedEffect can be compiled and cached.
    /// Use this library to eliminated duplicated LinkedEffects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContentProcessorContextExtensions
    {
        const string DefaultOutputDirectory = @"Misc\Build";

        public static ExternalReference<TOutput> BuildAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName)
        {
            return BuildAsset<TInput, TOutput>(context, input, processorName, null, null);
        }

        public static ExternalReference<TOutput> BuildAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName, OpaqueDataDictionary processorParameters, string assetName)
        {
            if (string.IsNullOrEmpty(assetName))
            {
                assetName = Path.Combine(DefaultOutputDirectory, Guid.NewGuid().ToString("N"));
            }

            var name = Path.GetFileName(assetName);
            var sourceFile = Path.Combine(context.IntermediateDirectory, name + ".xml");

            // There's currently no way to build from object, so we need to create a temperory file
            using (XmlWriter writer = XmlWriter.Create(sourceFile))
            {
                IntermediateSerializer.Serialize(writer, input, ".");
            }

            // Build the source asset
            return context.BuildAsset<TInput, TOutput>(new ExternalReference<TInput>(sourceFile), processorName, processorParameters, null, assetName);
        }
    }
}
