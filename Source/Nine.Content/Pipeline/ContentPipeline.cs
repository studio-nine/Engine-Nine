namespace Nine.Content.Pipeline
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Serialization;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.Text;
    using IImporter = Microsoft.Xna.Framework.Content.Pipeline.IContentImporter;
    using IProcessor = Microsoft.Xna.Framework.Content.Pipeline.IContentProcessor;

    /// <summary>
    /// Encapsulate commonly used XNA content pipeline functionalities.
    /// </summary>
    public static class ContentPipeline
    {
        static MD5 md5 = MD5.Create();
        static PipelineBuilder builder = new PipelineBuilder();
        static PipelineObjectReader reader = new PipelineObjectReader();
        static XamlSerializer xamlSerializer = new XamlSerializer();

        internal static readonly string OutputDirectory;
        internal static readonly string IntermediateDirectory;
        internal static readonly string CacheDirectory;

        /// <summary>
        /// Stores a mapping from the runtime model (E.g. Texture) to a cached .xnb file name.
        /// </summary>
        internal static Dictionary<WeakReference, string> ObjectCache = new Dictionary<WeakReference, string>(new WeakReferenceEqualtyComparer());

        static ContentPipeline()
        {
            // TODO: Make this cache directory configurable
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            var versionString = string.Format("v{0}.{1}", version.Major, version.Minor);
            var baseDirectory = Path.Combine(Path.GetTempPath(), "Engine Nine", versionString, "ContentPipeline");

            IntermediateDirectory = Path.Combine(baseDirectory, "Intermediate");
            OutputDirectory = Path.Combine(baseDirectory, "Bin");
            CacheDirectory = Path.Combine(baseDirectory, "Cache");

            if (!Directory.Exists(IntermediateDirectory))
                Directory.CreateDirectory(IntermediateDirectory);
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);
            if (!Directory.Exists(CacheDirectory))
                Directory.CreateDirectory(CacheDirectory);
        }

        public static void SaveContent<T>(string outputFilename, T content)
        {
            if (!Directory.Exists(Path.GetDirectoryName(outputFilename)))
                Directory.CreateDirectory(Path.GetDirectoryName(outputFilename));

            using (var stream = new FileStream(outputFilename, FileMode.Create))
            {
                SaveContent(stream, content);
            }
        }

        public static void SaveContent<T>(Stream output, T content)
        {
            var constructor = typeof(ContentCompiler).GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, null, new Type[] { }, null);
            var contentCompiler = (ContentCompiler)constructor.Invoke(null);
            var referenceDirectory = Path.GetDirectoryName(builder.LastFilename) + "\\";

            var method = contentCompiler.GetType().GetMethod("Compile", BindingFlags.NonPublic | BindingFlags.Instance);
            method.Invoke(contentCompiler, new object[] { output, content, TargetPlatform.Windows, GraphicsProfile.Reach, true, referenceDirectory, referenceDirectory });
            output.Flush();
        }

        public static T LoadContent<T>(Stream input, IImporter importer, IProcessor processor = null)
        {
            return LoadContent<T>(ValidateFileStream(input), importer, processor);
        }

        public static T LoadContent<T>(string fileName, IImporter importer, IProcessor processor = null)
        {
            return builder.BuildAndLoad<T>(fileName, importer, processor);
        }

        public static T Load<T>(Stream input, IImporter importer, IProcessor processor = null, IServiceProvider serviceProvider = null)
        {
            return Load<T>(ValidateFileStream(input), importer, processor, serviceProvider);
        }

        public static T Load<T>(string fileName, IImporter importer, IProcessor processor = null, IServiceProvider serviceProvider = null)
        {
            // TODO: Race conditions when 2 processes tried to access the file at the same time?
            var cachedFileName = GetCachedFileName(fileName, processor);
            if (!File.Exists(cachedFileName))
            {
                var content = LoadContent<object>(fileName, importer, processor);
                if (content == null)
                    return default(T);

                // TODO: Model does not save Tag, so using a processor is useless.
                SaveContent(cachedFileName, content);
            }

            using (var stream = File.OpenRead(cachedFileName))
            {
                var result = (T)(reader.Read(new BinaryReader(stream), null, serviceProvider));
                ObjectCache.Add(new WeakReference(result), cachedFileName);
                return result;
            }
        }

        public static string[] GetSupportedFileExtensions(Type importerType)
        {
            return importerType.GetCustomAttributes(true)
                               .OfType<ContentImporterAttribute>()
                               .SelectMany(x => x.FileExtensions).ToArray();
        }

        private static string GetCachedFileName(string fileName, IProcessor processor = null)
        {
            fileName = Extensions.CleanPath(fileName).ToLowerInvariant();

            var memoryStream = new MemoryStream(1024);
            var bytes = Encoding.UTF8.GetBytes(fileName);
            memoryStream.Write(bytes, 0, bytes.Length);
            bytes = BitConverter.GetBytes(File.GetLastWriteTimeUtc(fileName).Ticks);
            memoryStream.Write(bytes, 0, bytes.Length);

            if (processor != null)
            {
                xamlSerializer.Save(memoryStream, processor, null);
            }

            return Path.Combine(CacheDirectory, string.Concat(
                   Path.GetFileNameWithoutExtension(fileName), "-",
                   new Guid(md5.ComputeHash(memoryStream.ToArray())).ToString("N").ToUpper()));
        }

        private static string ValidateFileStream(Stream input)
        {
            var file = input as FileStream;
            if (file == null)
                throw new NotSupportedException("Only FileStream is supported.");
            file.Close();
            return file.Name;
        }
    }
}
