namespace Nine.Content.Pipeline
{
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using IImporter = Microsoft.Xna.Framework.Content.Pipeline.IContentImporter;
    using IProcessor = Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor;
    
    class PipelineBuilder
    {
        public string OutputFilename
        {
            get { return outputFilenames.Count > 0 ? outputFilenames.Peek() : null; }
        }
        private Stack<string> outputFilenames = new Stack<string>();

        public string LastFilename { get; private set; }

        public ContentImporterContext ImporterContext { get; private set; }
        public ContentProcessorContext ProcessorContext { get; private set; }

        public PipelineBuilder()
        {
            ImporterContext = new PipelineImporterContext(this);
            ProcessorContext = new PipelineProcessorContext(this);
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

        public TContent BuildAndLoad<TContent>(string sourceAssetFile, IImporter contentImporter, IProcessor contentProcessor)
        {
            return BuildAndLoad<TContent>(sourceAssetFile, contentImporter, contentProcessor, null);
        }

        public TContent BuildAndLoad<TContent>(string sourceAssetFile, IImporter contentImporter, IProcessor contentProcessor, string fileName)
        {
            contentImporter = contentImporter ?? FindImporter(null, sourceAssetFile);
            if (contentImporter == null)
                throw new InvalidOperationException(string.Format("Cannot find content importer for {0}", sourceAssetFile));
            var content = contentImporter.Import(sourceAssetFile, ImporterContext);
            
            contentProcessor = contentProcessor ?? FindDefaultProcessor(contentImporter);
            if (contentProcessor != null)
            {
                try
                {
                    outputFilenames.Push(sourceAssetFile);
                    content = contentProcessor.Process(content, ProcessorContext);
                }
                finally
                {
                    LastFilename = outputFilenames.Pop();
                }
            }
            return (TContent)content;
        }

        public TOutput Convert<TInput, TOutput>(TInput input, string processorName, OpaqueDataDictionary processorParameters)
        {
            var processor = ContentProcessors.FirstOrDefault(p => p.GetType().Name == processorName);
            if (processor == null)
                throw new InvalidOperationException(string.Format("Cannot find processor {0}", processorName));

            ApplyParameters(processor, processorParameters);
            return (TOutput)processor.Process(input, ProcessorContext);
        }
        
        public event Func<string, string> ExternalReferenceResolve;

        internal string ResolveExternalReference(string reference)
        {
            string result = reference;
            if (ExternalReferenceResolve != null)
            {
                foreach (Func<string, string> resolve in ExternalReferenceResolve.GetInvocationList())
                {
                    var current = resolve(reference);
                    if (!string.IsNullOrEmpty(current))
                        result = current;
                }
            }
            return result;
        }

        public static ICollection<IImporter> ContentImporters { get; private set; }
        public static ICollection<IProcessor> ContentProcessors { get; private set; }
        public static ICollection<ContentTypeWriter> ContentWriters { get; private set; }
                        
        static PipelineBuilder()
        {
            ContentImporters = new HashSet<IImporter>();
            ContentProcessors = new HashSet<IProcessor>();
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

        private static IProcessor FindDefaultProcessor(IImporter importer)
        {
            if (importer == null)
                return null;
            return FindProcessor(importer.GetType().GetCustomAttributes(false).OfType<ContentImporterAttribute>().First().DefaultProcessor);
        }

        private static IImporter FindImporter(string importerName, string fileName)
        {
            if (!string.IsNullOrEmpty(importerName))
                return PipelineBuilder.ContentImporters.FirstOrDefault(i => i.GetType().Name == importerName);

            return ContentImporters.FirstOrDefault(i => ImporterCompatibleWithFile(i, fileName));
        }

        private static IProcessor FindProcessor(string processorName)
        {
            if (string.IsNullOrEmpty(processorName))
                return null;
            return ContentProcessors.First(p => p.GetType().Name == processorName);
        }

        private static bool ImporterCompatibleWithFile(IImporter importer, string sourceAssetFile)
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
                    if (typeof(IImporter).IsAssignableFrom(type) &&
                        type.GetCustomAttributes(typeof(ContentImporterAttribute), false).Any() &&
                        !ContentImporters.Any(i => i.GetType() == type))
                    {
                        ContentImporters.Add((IImporter)Activator.CreateInstance(type));
                    }
                    if (typeof(IProcessor).IsAssignableFrom(type) &&
                        type.GetCustomAttributes(typeof(ContentProcessorAttribute), false).Any() &&
                        !ContentProcessors.Any(i => i.GetType() == type))
                    {
                        ContentProcessors.Add((IProcessor)Activator.CreateInstance(type));
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
                catch (Exception)
                {
                    Trace.TraceWarning("Error converting {0} to type {1}", param.Value, property.PropertyType);
                }

                try
                {
                    property.SetValue(value, targetValue, null);
                }
                catch (Exception)
                {
                    Trace.TraceWarning("Error setting {0} to property {1} on {2}", param.Value, param.Key, property.PropertyType);
                }
            }
        }
    }
}
