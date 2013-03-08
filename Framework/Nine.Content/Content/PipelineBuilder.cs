namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;
    using IImporter = Microsoft.Xna.Framework.Content.Pipeline.IContentImporter;
    using IProcessor = Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor;
    
    /// <summary>
    /// Enables Xna framework content build without using Visual Studio
    /// </summary>
    public class PipelineBuilder
    {
        internal string OutputFilename;
        internal PipelineConstants Constants;

        private PipelineImporterContext importerContext;
        private PipelineProcessorContext processorContext;

        public ContentImporterContext ImporterContext
        {
            get { return importerContext ?? (importerContext = new PipelineImporterContext(this)); }
        }

        public ContentProcessorContext ProcessorContext
        {
            get { return processorContext ?? (processorContext = new PipelineProcessorContext(this)); }
        }

        public PipelineBuilder(GraphicsDevice graphics = null) : this(graphics, TargetPlatform.Windows)
        {
            
        }

        public PipelineBuilder(GraphicsDevice graphics, TargetPlatform targetPlatform, 
                               string intermediateDirectory = null, string outputDirectory = null)
        {
            Constants = new PipelineConstants(intermediateDirectory, outputDirectory, targetPlatform, graphics);
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

        public string Build(string sourceAssetFile, IImporter contentImporter, IProcessor contentProcessor)
        {
            return Build(sourceAssetFile, contentImporter, contentProcessor, null);
        }

        public string Build(string sourceAssetFile, IImporter contentImporter, IProcessor contentProcessor, string assetName)
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

        public TContent BuildAndLoad<TContent>(string sourceAssetFile, IImporter contentImporter, IProcessor contentProcessor)
        {
            return BuildAndLoad<TContent>(sourceAssetFile, contentImporter, contentProcessor, null);
        }

        public TContent BuildAndLoad<TContent>(string sourceAssetFile, IImporter contentImporter, IProcessor contentProcessor, string assetName)
        {
            try
            {
                OutputFilename = FindNextValidAssetName(assetName ?? Path.GetFileNameWithoutExtension(sourceAssetFile), ".xnb");
                
                var outputDirectory = Path.GetDirectoryName(OutputFilename);
                if (!Directory.Exists(outputDirectory))
                    Directory.CreateDirectory(outputDirectory);

                // Create a dummy file in case a nested build uses the same name
                File.WriteAllText(OutputFilename, "");

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

        public ContentManager Content
        {
            get { return contentManager ?? (contentManager = new PipelineContentManager(Constants.GraphicsDevice)); }
        }
        private PipelineContentManager contentManager;

        public TRuntime Load<TRuntime>(string sourceAssetFile)
        {
            return Load<TRuntime>(sourceAssetFile, null, null);
        }

        public TRuntime Load<TRuntime>(string sourceAssetFile, IImporter importer, IProcessor processor)
        {
            var content = BuildAndLoad<object>(sourceAssetFile, importer, processor, null);
            if (content == null)
                return default(TRuntime);
            Compile(OutputFilename, content);
            return Content.Load<TRuntime>(OutputFilename);
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

        public PipelineBuilder Clone()
        {
            var result = new PipelineBuilder(Constants.GraphicsDevice, Constants.TargetPlatform, Constants.IntermediateDirectory, Constants.OutputDirectory);
            result.ExternalReferenceResolve = this.ExternalReferenceResolve;
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

        private string FindRuntimeType(object content)
        {
            if (content == null)
                return null;
            var contentWriter = ContentWriters.FirstOrDefault(writer => writer.TargetType == content.GetType());
            return contentWriter != null ? contentWriter.GetRuntimeType(Constants.TargetPlatform) : content.GetType().AssemblyQualifiedName;
        }

        private static IProcessor FindDefaultProcessor(IImporter importer)
        {
            if (importer == null)
                return null;
            return FindProcessor(importer.GetType().GetCustomAttributes(false).OfType<ContentImporterAttribute>().First().DefaultProcessor);
        }

        private static IImporter FindImporter(string importerName, string assetName)
        {
            if (!string.IsNullOrEmpty(importerName))
                return PipelineBuilder.ContentImporters.FirstOrDefault(i => i.GetType().Name == importerName);

            return ContentImporters.FirstOrDefault(i => ImporterCompatibleWithFile(i, assetName));
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

        private string FindNextValidAssetName(string assetName, string extension)
        {
            int i = 0;
            string assetFilename;
            while (File.Exists(assetFilename = GetAssetFilename(assetName, i++, extension))) ;
            return assetFilename;
        }

        private string GetAssetFilename(string assetName, int i, string extension)
        {
            if (!Path.IsPathRooted(assetName))
                assetName = Path.Combine(Constants.OutputDirectory, assetName);

            if (i > 0)
                return assetName + i.ToString() + extension;
            return assetName + extension;
        }

        public void Compile<T>(string outputFilename, T content)
        {
            if (!Directory.Exists(Path.GetDirectoryName(outputFilename)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilename));

            using (var stream = new FileStream(outputFilename, FileMode.Create))
            {
                Compile(stream, content);
            }
        }

        public void Compile<T>(Stream output, T content)
        {
            try
            {
                var constructor = typeof(ContentCompiler).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
                var contentCompiler = (ContentCompiler)constructor.Invoke(null);

                var method = contentCompiler.GetType().GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Instance);
                method.Invoke(contentCompiler, new object[] 
                {
                    output, content, TargetPlatform.Windows, GraphicsProfile.Reach, false, Constants.OutputDirectory, ".",
                });
                output.Flush();
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
