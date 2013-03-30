namespace Nine.Content.Pipeline
{
    using Microsoft.Xna.Framework.Content.Pipeline;
    using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Compiler;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Serialization;
    using System;
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

        internal static readonly string OutputDirectory;
        internal static readonly string IntermediateDirectory;
        internal static readonly string CacheDirectory;

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
            method.Invoke(contentCompiler, new object[] 
            {
                output, content, TargetPlatform.Windows, GraphicsProfile.Reach, false, referenceDirectory, referenceDirectory
            });
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
            fileName = Extensions.CleanPath(fileName).ToLowerInvariant();
            
            var fileNameHash = new Guid(md5.ComputeHash(Encoding.UTF8.GetBytes(fileName))).ToString("N").ToUpper();
            var lastModified = File.GetLastWriteTimeUtc(fileName);
            var fileSignature = fileNameHash + "-" + lastModified.Ticks;
            var cachedFileName = Path.Combine(CacheDirectory, fileSignature);
            
            if (!File.Exists(cachedFileName))
            {
                var content = LoadContent<object>(fileName, importer, processor);
                if (content == null)
                    return default(T);

                SaveContent(cachedFileName, content);
            }
            
            foreach (var invalidCache in Directory.GetFiles(CacheDirectory, fileNameHash + "-*"))
            {
                if (!string.Equals(Path.GetFileName(invalidCache), fileSignature, StringComparison.OrdinalIgnoreCase))
                    File.Delete(invalidCache);
            }

            using (var stream = File.OpenRead(cachedFileName))
            {
                return (T)(reader.Read(new BinaryReader(stream), null, serviceProvider));
            }
        }

        public static string[] GetSupportedFileExtensions(Type importerType)
        {
            return importerType.GetCustomAttributes(true)
                               .OfType<ContentImporterAttribute>()
                               .SelectMany(x => x.FileExtensions).ToArray();
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
