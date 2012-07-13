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
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Content.Pipeline
{
    /// <summary>
    /// Enables Xna framework content build without using MSBuild
    /// </summary>
    public class PipelineBuilder
    {
        internal string OutputFilename { get; private set; }

        private PipelineImporterContext importerContext;
        private PipelineProcessorContext processorContext;

        public ContentImporterContext ImporterContext
        {
            get { return importerContext ?? (importerContext = new PipelineImporterContext()); }
        }

        public ContentProcessorContext ProcessorContext
        {
            get { return processorContext ?? (processorContext = new PipelineProcessorContext(this)); }
        }

        public string Build(string sourceAssetFile, string processorName, OpaqueDataDictionary processorParameters, string importerName, string assetName)
        {
            var importer = FindImporter(importerName, sourceAssetFile);
            if (importer == null)
                throw new InvalidContentException(string.Format("Cannot find importer {0} for file {1}", importerName, sourceAssetFile));

            var processor = PipelineBuilder.ContentProcessors.FirstOrDefault(p => p.GetType().Name == processorName);
            ApplyParameters(processor, processorParameters);
            return Build(sourceAssetFile, importer, processor, assetName);
        }

        public string Build(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor)
        {
            return Build(sourceAssetFile, contentImporter, contentProcessor, null);
        }

        public string Build(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor, string assetName)
        {
            var content = BuildAndLoad<object>(sourceAssetFile, contentImporter, contentProcessor, assetName);
            Compile(OutputFilename, content);
            return OutputFilename;
        }

        public TContent BuildAndLoad<TContent>(string sourceAssetFile, string processorName, OpaqueDataDictionary processorParameters, string importerName)
        {
            var importer = FindImporter(importerName, sourceAssetFile);
            if (importer == null)
                throw new InvalidContentException(string.Format("Cannot find importer {0} for file {1}", importerName, sourceAssetFile));

            var processor = PipelineBuilder.ContentProcessors.FirstOrDefault(p => p.GetType().Name == processorName);
            ApplyParameters(processor, processorParameters);
            return BuildAndLoad<TContent>(sourceAssetFile, importer, processor);
        }

        public TContent BuildAndLoad<TContent>(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor)
        {
            return BuildAndLoad<TContent>(sourceAssetFile, contentImporter, contentProcessor, null);
        }

        public TContent BuildAndLoad<TContent>(string sourceAssetFile, IContentImporter contentImporter, IContentProcessor contentProcessor, string assetName)
        {
            try
            {
                OutputFilename = FindNextValidAssetName(assetName ?? Path.GetFileNameWithoutExtension(sourceAssetFile), ".xnb");

                contentImporter = contentImporter ?? FindImporter(null, sourceAssetFile);
                if (contentImporter == null)
                    throw new InvalidOperationException(string.Format("Cannot find content importer for {0}", sourceAssetFile));

                contentProcessor = contentProcessor ?? FindDefaultProcessor(contentImporter);

                Trace.TraceInformation("Building {0} -> {1} with {2} and {3}", 
                        sourceAssetFile, OutputFilename,
                        contentImporter != null ? contentImporter.GetType().Name : "<No Importer>",
                        contentProcessor != null ? contentProcessor.GetType().Name : "<No Processor>");
                                
                object content = contentImporter.Import(sourceAssetFile, ImporterContext);

                if (contentProcessor != null)
                {
                    content = contentProcessor.Process(content, ProcessorContext);
                }
                return (TContent)content;
            }
            catch (Exception e)
            {
                Trace.TraceError("Error importing {0} with asset name {1}", sourceAssetFile, assetName ?? "[Unspecified]");
                Trace.WriteLine(e);
                throw;
            }
        }



        public static ICollection<IContentImporter> ContentImporters { get; private set; }
        public static ICollection<IContentProcessor> ContentProcessors { get; private set; }
        public static ICollection<ContentTypeWriter> ContentWriters { get; private set; }
        
        public static ContentManager Content
        {
            get { return contentManager ?? (contentManager = new PipelineContentManager(PipelineConstants.GraphicsDevice)); }
        }
        private static PipelineContentManager contentManager;
                
        static PipelineBuilder()
        {
            ContentImporters = new HashSet<IContentImporter>();
            ContentProcessors = new HashSet<IContentProcessor>();
            ContentWriters = new HashSet<ContentTypeWriter>();

            FindImportersProcessorAndContentWriters();
            FindImportersProcessorAndContentWriters(GetExecutableLocation());
        }

        private static string GetExecutableLocation()
        {
            try
            {
                return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            }
            catch (Exception)
            {
                return ".";
            }
        }
        
        public static TRuntime Load<TRuntime>(string sourceAssetFile)
        {
            return Load<TRuntime>(sourceAssetFile, null, null);
        }

        public static TRuntime Load<TRuntime>(string sourceAssetFile, IContentImporter importer, IContentProcessor processor)
        {
            var builder = new PipelineBuilder();
            var content = builder.BuildAndLoad<object>(sourceAssetFile, importer, processor, null);
            if (content == null)
                return default(TRuntime);
            //if (FindRuntimeType(content) == content.GetType().AssemblyQualifiedName)
            //    return (TRuntime)content;
            Compile(builder.OutputFilename, content);
            return Content.Load<TRuntime>(builder.OutputFilename);
        }

        private static string FindRuntimeType(object content)
        {
            if (content == null)
                return null;
            var contentWriter = ContentWriters.FirstOrDefault(writer => writer.TargetType == content.GetType());
            return contentWriter != null ? contentWriter.GetRuntimeType(PipelineConstants.TargetPlatform) : content.GetType().AssemblyQualifiedName;
        }

        private static IContentProcessor FindDefaultProcessor(IContentImporter importer)
        {
            if (importer == null)
                return null;
            return FindProcessor(importer.GetType().GetCustomAttributes(false).OfType<ContentImporterAttribute>().First().DefaultProcessor);
        }

        private static IContentImporter FindImporter(string importerName, string assetName)
        {
            if (!string.IsNullOrEmpty(importerName))
                return PipelineBuilder.ContentImporters.FirstOrDefault(i => i.GetType().Name == importerName);

            return ContentImporters.FirstOrDefault(i => ImporterCompatibleWithFile(i, assetName));
        }

        private static IContentProcessor FindProcessor(string processorName)
        {
            if (string.IsNullOrEmpty(processorName))
                return null;
            return ContentProcessors.First(p => p.GetType().Name == processorName);
        }

        private static bool ImporterCompatibleWithFile(IContentImporter importer, string sourceAssetFile)
        {
            return importer.GetType().GetCustomAttributes(false)
                                     .OfType<ContentImporterAttribute>().First()
                                     .FileExtensions.Any(ext => FileExtensionEquals(ext, Path.GetExtension(sourceAssetFile)));
        }

        private static bool FileExtensionEquals(string ext1, string ext2)
        {
            if (!ext1.StartsWith("."))
                ext1 += ".";
            if (!ext2.StartsWith("."))
                ext2 += ".";
            return StringComparer.OrdinalIgnoreCase.Equals(ext1, ext2);
        }

        private static string FindNextValidAssetName(string assetName, string extension)
        {
            int i = 0;
            string assetFilename;
            while (File.Exists(assetFilename = GetAssetFilename(assetName, i++, extension))) ;
            return assetFilename;
        }

        private static string GetAssetFilename(string assetName, int i, string extension)
        {
            if (!Path.IsPathRooted(assetName))
                assetName = Path.Combine(PipelineConstants.OutputDirectory, assetName);

            if (i > 0)
                return assetName + i.ToString() + extension;
            return assetName + extension;
        }

        public TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            try
            {
                var processor = ContentProcessors.FirstOrDefault(p => p.GetType().Name == processorName);
                if (processor == null)
                    throw new InvalidOperationException(string.Format("Cannot find processor {0}", processorName));

                ApplyParameters(processor, processorParameters);
                processorContext = processorContext ?? new PipelineProcessorContext(this);
                return (TOutput)processor.Process(input, processorContext);
            }
            catch (Exception e)
            {
                Trace.TraceError("Error converting {0} with processor {1}", input.GetType().Name, processorName);
                Trace.WriteLine(e);
                throw new InvalidContentException("", e);
            }
        }

        public static void Compile<T>(string outputFilename, T content)
        {
            if (!Directory.Exists(Path.GetDirectoryName(outputFilename)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilename));

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
                    output, content, TargetPlatform.Windows, GraphicsProfile.Reach, false, PipelineConstants.OutputDirectory, ".",
                });
            }
            catch (Exception e)
            {
                Trace.TraceError("Error compiling {0}", content.GetType());
                Trace.WriteLine(e);
                throw new InvalidContentException("", e);
            }
        }

        private static void FindImportersProcessorAndContentWriters()
        {
            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                FindImportersProcessorAndContentWriters(assembly);
            }
        }

        private static void FindImportersProcessorAndContentWriters(string path)
        {
            path = Path.GetFullPath(path);
            if (!Directory.Exists(path))
                return;

            foreach (var file in Directory.GetFiles(path, "*.dll", SearchOption.TopDirectoryOnly))
            {
                try
                {
                    FindImportersProcessorAndContentWriters(Assembly.LoadFrom(file));
                }
                catch (Exception e)
                {
                    Trace.TraceWarning("Error loading assembly from {0}", file);
                    Trace.WriteLine(e.ToString());
                }
            }
        }

        private static void FindImportersProcessorAndContentWriters(Assembly assembly)
        {
            if (assembly.FullName.StartsWith("System") ||
                assembly.FullName.StartsWith("mscorlib") ||
               (assembly.FullName.StartsWith("Microsoft.") && !assembly.FullName.StartsWith("Microsoft.Xna.Framework.Content.Pipeline")))
            {
                return;
            }

            foreach (var type in assembly.GetTypes())
            {
                try
                {
                    if (typeof(IContentImporter).IsAssignableFrom(type) &&
                        type.GetCustomAttributes(typeof(ContentImporterAttribute), false).Any() &&
                        !ContentImporters.Any(i => i.GetType() == type))
                    {
                        ContentImporters.Add((IContentImporter)Activator.CreateInstance(type));
                    }
                    if (typeof(IContentProcessor).IsAssignableFrom(type) &&
                        type.GetCustomAttributes(typeof(ContentProcessorAttribute), false).Any() &&
                        !ContentProcessors.Any(i => i.GetType() == type))
                    {
                        ContentProcessors.Add((IContentProcessor)Activator.CreateInstance(type));
                    }
                    if (typeof(ContentTypeWriter).IsAssignableFrom(type) &&
                        type.GetCustomAttributes(typeof(ContentTypeWriterAttribute), false).Any() &&
                        !ContentWriters.Any(i => i.GetType() == type) && !type.IsGenericTypeDefinition)
                    {
                        ContentWriters.Add((ContentTypeWriter)Activator.CreateInstance(type));
                    }
                }
                catch (Exception e)
                {
                    Trace.TraceWarning("Error load type {0} from assembly {1}", type.AssemblyQualifiedName, assembly.FullName);
                    Trace.WriteLine(e.ToString());
                }
            }
        }

        private static void ApplyParameters(object value, OpaqueDataDictionary parameters)
        {
            if (value == null || parameters == null)
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
