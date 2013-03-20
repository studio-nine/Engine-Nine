namespace Nine.Serialization
{
    using Nine.Content.Pipeline;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Windows.Markup;

    /// <summary>
    /// Interface used to build a source asset into a binary package.
    /// </summary>
    public interface IPackageBuilder
    {
        void Build(Stream input, Stream output);
    }

    /// <summary>
    /// Contains operations to create binary packages.
    /// </summary>
    public class Package
    {
        static Package()
        {
            Embedded.EnsureAssembliesInitialized();

            Loader.Resolvers.Clear();
            Loader.Resolvers.Add(Resolver);
            Loader.Services.Add(new SerializationOverride());
        }

        static BinarySerializer Writer = new BinarySerializer();
        static FileSystemResolver Resolver = new FileSystemResolver();
        static ContentLoader Loader = new ContentLoader(new PipelineGraphicsDeviceService());
        
        public static void BuildFile(string fileName, string outputFileName)
        {
            var outputDirectory = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            using (var output = new FileStream(outputFileName, FileMode.Create))
            {
                BuildItem(fileName, output);
            }
        }

        private static void BuildItem(string fileName, Stream output)
        {
            var content = Loader.Load<object>(fileName);
            Writer.Save(output, content, Loader);
        }


        public static void BuildDirectory(string directoryName, string outputFileName)
        {
            directoryName = Path.GetFullPath(directoryName);
            outputFileName = Path.GetFullPath(outputFileName);

            var outputDirectory = outputFileName + "-" + Guid.NewGuid().ToString("N").ToUpper();
                Directory.CreateDirectory(outputDirectory);

            try
            {
                foreach (var file in Directory.EnumerateFiles(directoryName, "*.*", SearchOption.AllDirectories))
                    {
                    var extension = Path.GetExtension(file);
                    if (string.Equals(extension, ".importer", StringComparison.OrdinalIgnoreCase))
                        continue;
                    BuildFile(file, Path.Combine(outputDirectory, file.Substring(directoryName.Length + 1)));
                }

                if (File.Exists(outputFileName))
                    File.Delete(outputFileName);

                using (var archive = new Ionic.Zip.ZipFile(outputFileName, Encoding.UTF8))
                {
                    archive.AddDirectory(outputDirectory);
                    archive.Save();
                }
            }
            finally
            {
                Directory.Delete(outputDirectory, true);
            }
    }
    }
}