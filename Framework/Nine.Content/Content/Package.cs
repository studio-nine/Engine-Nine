namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Text;
    using System.Windows.Markup;

    [ContentProperty("Items")]
    public class Package
    {
        internal static readonly string FileExtension = ".nine";

        public string Name { get; set; }
        public string Version { get; set; }
        public ICollection<string> AssemblyReferences { get; private set; }
        public ICollection<PackageReference> References { get; private set; }
        public ICollection<PackageItem> Items { get; private set; }

        public Package()
        {
            References = new List<PackageReference>();
            Items = new HashSet<PackageItem>();
        }

        static Package()
        {
            Embedded.EnsureAssembliesInitialized();
        }

        public static void Build(string inputFileName, string outputFileName)
        {
            Build(inputFileName, outputFileName, new PipelineGraphicsDeviceService { GraphicsDevice = Nine.Graphics.PipelineGraphics.GraphicsDevice });
        }

        public static void Build(string inputFileName, string outputFileName, IServiceProvider serviceProvider)
        {
            var reader = new XamlSerializer();
            var writer = new BinarySerializer();
            var package = reader.Load<Package>(inputFileName, serviceProvider);

            var inputDiectory = Path.GetDirectoryName(inputFileName);
            var outputDirectory = Path.Combine(Path.GetDirectoryName(outputFileName), Path.GetFileNameWithoutExtension(inputFileName) + "-" + Guid.NewGuid().ToString("N").ToUpper());

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            try
            {
                foreach (var item in package.Items)
                {
                    if (Path.IsPathRooted(item.Source))
                        throw new NotSupportedException("Absolute path not supported: " + item.Source);

                    var itemDirectory = Path.Combine(outputDirectory, Path.GetDirectoryName(item.Source));
                    if (!Directory.Exists(itemDirectory))
                        Directory.CreateDirectory(itemDirectory);

                    var name = Path.GetFileNameWithoutExtension(item.Source);
                    using (var output = new FileStream(Path.Combine(itemDirectory, name), FileMode.Create))
                    {
                        writer.Save(output, item.Loader.Load<object>(Path.Combine(inputDiectory, package.Name, item.Source), serviceProvider), serviceProvider);
                    }
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

    [ContentProperty("Loader")]
    [System.Diagnostics.DebuggerDisplay("Source")]
    public class PackageItem
    {
        public string Source
        {
            get { return source; }
            set { source = ContentLoader.NormalizePath(value); }
        }
        private string source;

        public IContentImporter Loader { get; set; }
    }

    [ContentProperty("Source")]
    public class PackageReference
    {
        public string Source { get; set; }
    }
}