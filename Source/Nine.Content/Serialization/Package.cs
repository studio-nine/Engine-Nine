namespace Nine.Serialization
{
    using Nine.Content.Pipeline;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Linq;
    using System.Text.RegularExpressions;
    using System.Windows.Markup;

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

            IgnoredFileNamePatterns = new[] { "*.importer", "*.ignore", "*.tmp" };
        }

        static BinarySerializer BinarySerializer = new BinarySerializer();
        static FileSystemResolver Resolver = new FileSystemResolver();
        static ContentLoader Loader = new ContentLoader(new PipelineGraphicsDeviceService());

        /// <summary>
        /// Gets a list of file name patterns to ignore during package build.
        /// </summary>
        public static IList<string> IgnoredFileNamePatterns { get; private set; }

        /// <summary>
        /// Build the input file to an output file using the BinarySerializer.
        /// </summary>
        public static void BuildFile(string fileName, string outputFileName)
        {
            var outputDirectory = Path.GetDirectoryName(outputFileName);
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            try
            {
                using (var output = new FileStream(outputFileName, FileMode.Create))
                {
                    BuildFile(fileName, output);
                }
            }
            catch
            {
                File.Delete(outputFileName);
            }
        }

        /// <summary>
        /// Build the input file to an output stream using the BinarySerializer.
        /// </summary>
        private static void BuildFile(string fileName, Stream output)
        {
            var content = Loader.Load<object>(fileName);
            BinarySerializer.Save(output, content, Loader);
        }
        
        /// <summary>
        /// Build the input files to an output binary package file using the BinarySerializer.        
        /// </summary>
        /// <remarks>
        /// The input files has to resides in the same directory.
        /// </remarks>
        private static void BuildFiles(IEnumerable<string> fileName, string outputFileName)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Build the input directory to an output binary package file using the BinarySerializer.
        /// </summary>
        public static void BuildDirectory(string directoryName, string outputFileName, Action<string, int, int> onProgress = null, Action<string, Exception> onError = null)
        {
            directoryName = Path.GetFullPath(directoryName);
            outputFileName = Path.GetFullPath(outputFileName);

            var outputDirectory = outputFileName + "-" + Guid.NewGuid().ToString("N").ToUpper();
            Directory.CreateDirectory(outputDirectory);

            try
            {
                var files = Directory.EnumerateFiles(directoryName, "*.*", SearchOption.AllDirectories).Except(
                    IgnoredFileNamePatterns.SelectMany(x => Directory.EnumerateFiles(directoryName, x, SearchOption.AllDirectories))).ToArray();

                for (int i = 0; i < files.Length; i++)
                {
                    var file = Path.GetFullPath(files[i]);

                    if (onProgress != null)
                        onProgress(file, i, files.Length);

                    try
                    {
                        BuildFile(file, Path.Combine(outputDirectory, Extensions.MakeRelativePath(directoryName, file)));
                    }
                    catch (Exception e)
                    {
                        if (onError != null)
                            onError(file, e);
                        else
                            throw;
                    }
                }

                if (onProgress != null)
                    onProgress("", files.Length, files.Length);

                if (File.Exists(outputFileName))
                    File.Delete(outputFileName);

                using (var archive = new Ionic.Zip.ZipFile(outputFileName, Encoding.UTF8))
                {
                    archive.CompressionMethod = Ionic.Zip.CompressionMethod.Deflate;
                    archive.AddDirectory(outputDirectory);
                    archive.Save();
                }
            }
            catch
            {
                Directory.Delete(outputDirectory, true);
                throw;
            }
            finally
            {
                Directory.Delete(outputDirectory, true);
            }
        }
    }
}