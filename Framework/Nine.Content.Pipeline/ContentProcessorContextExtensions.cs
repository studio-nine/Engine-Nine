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
using System.Xml.Linq;
using System.Linq;
using System.Text;
using System.ComponentModel;
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

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// Enables a centralized place where LinkedEffect can be compiled and cached.
    /// Use this library to eliminated duplicated LinkedEffects.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContentProcessorContextExtensions
    {
        // This value will be used by ContentReference when serialize using
        // reference relocation path.
        internal static string ContentReferenceBasePath;

        const string DefaultOutputDirectory = @"Misc";

        public static ExternalReference<TOutput> BuildAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName)
        {
            return BuildAsset<TInput, TOutput>(context, input, processorName, null, null);
        }

        public static ExternalReference<TOutput> BuildAsset<TInput, TOutput>(this ContentProcessorContext context, TInput input, string processorName, OpaqueDataDictionary processorParameters, string assetName)
        {
            ContentReferenceBasePath = Path.GetDirectoryName(context.OutputFilename);

            // There's currently no way to build from object, so we need to create a temperory file
            using (var stream = new MemoryStream())
            {
                using (XmlWriter writer = XmlWriter.Create(stream))
                {
                    try
                    {
                        IntermediateSerializer.Serialize(writer, input, Directory.GetCurrentDirectory() + "\\");
                    }
                    finally
                    {
                        ContentReferenceBasePath = null;
                    }
                }

                FixExternalReference(context, stream);

                var sourceFile = "";

                if (string.IsNullOrEmpty(assetName))
                {
                    var hashString = new StringBuilder();
                    stream.Seek(0, SeekOrigin.Begin);
                    var hash = MD5.Create().ComputeHash(stream);
                    for (int i = 0; i < hash.Length; i++)
                    {
                        hashString.Append(hash[i].ToString("X2"));
                    }

                    var name = hashString.ToString().ToUpperInvariant();
                    assetName = Path.Combine(DefaultOutputDirectory, name);
                    sourceFile = Path.Combine(context.IntermediateDirectory, input.GetType().Name + "-" + name + ".xml");
                }
                else
                {
                    sourceFile = Path.Combine(context.IntermediateDirectory, Path.GetFileName(assetName) + ".xml");
                }

                using (var assetFile = new FileStream(sourceFile, FileMode.Create))
                {
                    stream.Seek(0, SeekOrigin.Begin);
                    stream.WriteTo(assetFile);
                }

                // Build the source asset
                return context.BuildAsset<TInput, TOutput>(new ExternalReference<TInput>(sourceFile), processorName, processorParameters, null, assetName);
            }
        }

        private static void FixExternalReference(ContentProcessorContext context, MemoryStream stream)
        {
            var outputFile = new Uri(Path.GetFullPath(context.OutputFilename));
            var outputDirectory = new Uri(Path.GetFullPath(context.OutputDirectory));
            var relativePath = Path.GetDirectoryName(outputDirectory.MakeRelativeUri(outputFile).OriginalString);
            
            stream.Seek(0, SeekOrigin.Begin);            
            var xml = XDocument.Load(stream);

            foreach (var externalReference in xml.Descendants("ExternalReferences").SelectMany(e => e.Descendants("ExternalReference")))
            {
                externalReference.Value = Path.GetFullPath(Path.Combine(relativePath, externalReference.Value));
            }

            stream.Seek(0, SeekOrigin.Begin);
            xml.Save(stream);
        }
    }
}
