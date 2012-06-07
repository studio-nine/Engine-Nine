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
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Studio.Content
{
    public static class PipelineBuilder
    {
        const bool TraceIntermediateContent = false;

        public static ICollection<IContentImporter> ContentImporters { get; private set; }
        public static ICollection<IContentProcessor> ContentProcessors { get; private set; }

        private static PipelineImporterContext importerContext;
        private static PipelineProcessorContext processorContext;
        private static PipelineContentManager contentManager;

        private static HashSet<string> outputAssetNames = new HashSet<string>();
        private static Dictionary<object, string> contentToBinaries = new Dictionary<object, string>();
        
        static PipelineBuilder()
        {
            ContentImporters = new HashSet<IContentImporter>();
            ContentProcessors = new HashSet<IContentProcessor>();

            FindImportersAndProcessors(".");
            if (Global.ExtensionDirectory != ".")
                FindImportersAndProcessors(Global.ExtensionDirectory);
        }

        public static string Build(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor)
        {
            return Build(sourceAssetFile, contentImporter, contentProcessor, null);
        }

        public static string Build(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor, string assetName)
        {
            object content = BuildAndLoad<object>(sourceAssetFile, contentImporter, contentProcessor, assetName);
            return contentToBinaries[content];
        }

        public static string Build(string sourceAssetFile, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            var importer = FindImporter(importerName, sourceAssetFile);
            if (importer == null)
                throw new InvalidContentException(string.Format("Cannot find importer {0} for file {1}", importerName, sourceAssetFile));

            var processor = PipelineBuilder.ContentProcessors.FirstOrDefault(p => p.GetType().Name == processorName);
            ApplyParameters(processor, processorParameters);
            return Build(sourceAssetFile, importer, processor, assetName);
        }

        public static TContent BuildAndLoad<TContent>(string sourceAssetFile, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            var importer = FindImporter(importerName, sourceAssetFile);
            if (importer == null)
                throw new InvalidContentException(string.Format("Cannot find importer {0} for file {1}", importerName, sourceAssetFile));

            var processor = PipelineBuilder.ContentProcessors.FirstOrDefault(p => p.GetType().Name == processorName);
            ApplyParameters(processor, processorParameters);
            return BuildAndLoad<TContent>(sourceAssetFile, importer, processor);
        }

        public static TContent BuildAndLoad<TContent>(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor)
        {
            return BuildAndLoad<TContent>(sourceAssetFile, contentImporter, contentProcessor, null);
        }

        public static TContent BuildAndLoad<TContent>(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor, string assetName)
        {
            try
            {
                string outputAssetName = Path.GetFileName(assetName ?? Path.GetFileNameWithoutExtension(sourceAssetFile));
                outputAssetName = Global.NextName(outputAssetNames, outputAssetName);
                outputAssetNames.Add((outputAssetName));
                string outputAssetFile = Path.Combine(Global.OutputDirectory, outputAssetName + ".xnb");


                Trace.TraceInformation("Building {0} -> {1} with {2} and {3}", sourceAssetFile, outputAssetName,
                        contentImporter.GetType().Name, contentProcessor != null ? contentProcessor.GetType().Name : "");

                importerContext = importerContext ?? new PipelineImporterContext();
                object content = contentImporter.Import(sourceAssetFile, importerContext);

                if (TraceIntermediateContent)
                {
                    using (var writer = XmlWriter.Create(Path.Combine(Global.IntermediateDirectory, outputAssetName + ".Imported.Xml")))
                    {
                        IntermediateSerializer.Serialize(writer, content, ".");
                    }
                }

                if (contentProcessor != null)
                {
                    processorContext = processorContext ?? new PipelineProcessorContext();
                    content = contentProcessor.Process(content, processorContext);
                    
                    if (TraceIntermediateContent)
                    {
                        using (var writer = XmlWriter.Create(Path.Combine(Global.IntermediateDirectory, outputAssetName + ".Processed.Xml")))
                        {
                            IntermediateSerializer.Serialize(writer, content, ".");
                        }
                    }
                }
                
                Compile(outputAssetFile, content);
                contentToBinaries[content] = outputAssetFile;
                return (TContent)content;
            }
            catch (Exception e)
            {
                Trace.TraceError("Error importing {0} with asset name {1}", sourceAssetFile, assetName ?? "[Unspecified]");
                Trace.WriteLine(e);
                throw new InvalidContentException("", e);
            }
        }

        public static TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            try
            {
                var processor = ContentProcessors.FirstOrDefault(p => p.GetType().Name == processorName);
                if (processor == null)
                    throw new InvalidOperationException(string.Format("Cannot find processor {0}", processorName));

                ApplyParameters(processor, processorParameters);
                processorContext = processorContext ?? new PipelineProcessorContext();
                return (TOutput)processor.Process(input, processorContext);
            }
            catch (Exception e)
            {
                Trace.TraceError("Error converting {0} with processor {1}", input.GetType().Name, processorName);
                Trace.WriteLine(e);
                throw new InvalidContentException("", e);
            }
        }

        public static TRunTime Convert<TContent, TRunTime>(GraphicsDevice graphics, TContent content)
        {
            if (typeof(TRunTime).IsAssignableFrom(typeof(TContent)))
                return (TRunTime)(object)content;

            if (contentManager == null)
                contentManager = new PipelineContentManager(graphics);

            return contentManager.Load<TRunTime>(contentToBinaries[content]);
        }

        public static void Compile<T>(string outputFilename, T content)
        {
            using (var stream = new FileStream(outputFilename, FileMode.Create))
            {
                Compile(stream, content);
            }
        }

        public static void Compile<T>(Stream output, T content)
        {
            try
            {
                var constructor = typeof(ContentCompiler).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
                var contentCompiler = (ContentCompiler)constructor.Invoke(null);

                var method = contentCompiler.GetType().GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(contentCompiler, new object[] 
                {
                    output, content, TargetPlatform.Windows, GraphicsProfile.Reach, false, Global.OutputDirectory, ".",
                });
            }
            catch (Exception e)
            {
                Trace.TraceError("Error compiling {0}", content.GetType());
                Trace.WriteLine(e);
                throw new InvalidContentException("", e);
            }
        }
        
        private static void FindImportersAndProcessors(string dir)
        {
            string fullPath = Path.GetFullPath(dir);
            if (!Directory.Exists(fullPath))
                return;

            var files = Directory.GetFiles(fullPath, "*.dll", SearchOption.TopDirectoryOnly);
            if (files != null && files.Length > 0)
            {
                foreach (var file in files)
                {
                    try
                    {
                        Assembly asm = Assembly.LoadFrom(file);
                        foreach (var type in asm.GetTypes())
                        {
                            try
                            {
                                if (typeof(IContentImporter).IsAssignableFrom(type) &&
                                    type.GetCustomAttributes(typeof(ContentImporterAttribute), false).Count() == 1)
                                {
                                    ContentImporters.Add((IContentImporter)Activator.CreateInstance(type));
                                }
                                if (typeof(IContentProcessor).IsAssignableFrom(type) &&
                                    type.GetCustomAttributes(typeof(ContentProcessorAttribute), false).Count() == 1)
                                {
                                    ContentProcessors.Add((IContentProcessor)Activator.CreateInstance(type));
                                }
                            }
                            catch (Exception e)
                            {
                                Trace.TraceWarning("Error load type {0} from assembly {1}", type.AssemblyQualifiedName, file);
                                Trace.WriteLine(e.ToString());
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        Trace.TraceWarning("Error loading assembly from {0}", file);
                        Trace.WriteLine(e.ToString());
                    }
                }
            }
        }

        private static IContentImporter FindImporter(string importerName, string assetName)
        {
            if (!string.IsNullOrEmpty(importerName))
                return PipelineBuilder.ContentImporters.FirstOrDefault(i => i.GetType().Name == importerName);

            string ext = Path.GetExtension(assetName).ToLowerInvariant();
            return PipelineBuilder.ContentImporters.FirstOrDefault(i =>
            {
                var attrib = (ContentImporterAttribute)i.GetType().GetCustomAttributes(typeof(ContentImporterAttribute), false).First();
                if (attrib.FileExtensions == null)
                    return false;
                return attrib.FileExtensions.Any(str => str.ToLowerInvariant() == ext);
            });
        }

        private static void ApplyParameters(object value, OpaqueDataDictionary parameters)
        {
            if (value == null)
                return;

            var type = value.GetType();
            foreach (var param in parameters)
            {
                object targetValue = null;
                var property = type.GetProperty(param.Key, BindingFlags.FlattenHierarchy | BindingFlags.Public | BindingFlags.Instance);
                if (property == null)
                    continue;
                try
                {
                    TypeConverter converter = TypeDescriptor.GetConverter(property.PropertyType);
                    if (!converter.CanConvertFrom(param.Value.GetType()))
                        continue;
                    targetValue = converter.ConvertFrom(null, CultureInfo.InvariantCulture, param.Value);
                }
                catch (Exception e)
                {
                    Trace.TraceWarning("Error converting {0} to type {1}", param.Value, property.PropertyType);
                }

                try
                {
                    property.SetValue(value, targetValue, null);
                }
                catch (Exception e)
                {
                    Trace.TraceWarning("Error setting {0} to property {1} on {2}", param.Value, param.Key, property.PropertyType);
                }
            }
        }
    }
}
