namespace Nine.Serialization
{
    using Microsoft.Xna.Framework.Content;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Reflection;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class BinaryPackageResolver : IContentResolver, IDisposable
    {
        internal static readonly string FileExtension = ".n";

        private BinarySerializer binaryLoader;

        public BinaryPackageResolver() { }
        public BinaryPackageResolver(BinarySerializer binarySerializer) { this.binaryLoader = binarySerializer; }

        private Dictionary<string, ZipArchive> cachedPackages = new Dictionary<string, ZipArchive>(StringComparer.OrdinalIgnoreCase);

        public bool TryResolveContent(string fileName, IServiceProvider serviceProvider, out Stream stream, out IContentImporter contentLoader)
        {
            ZipArchive package;
            ZipArchiveEntry entry;
            string packageFilename;
            string packageDirectory = fileName;

            while (!string.IsNullOrEmpty(packageDirectory = Path.GetDirectoryName(packageDirectory)))
            {
                if (!cachedPackages.TryGetValue(packageDirectory, out package))
                {
                    if (!Directory.Exists(packageDirectory))
                        continue;
                    if (!File.Exists(packageFilename = packageDirectory + FileExtension))
                        continue;
                    cachedPackages[packageDirectory] = package = new ZipArchive(File.OpenRead(packageFilename));
                }

                if ((entry = package.GetEntry(fileName)) != null)
                {
                    stream = entry.Open();
                    contentLoader = binaryLoader;
                    return true;
                }
            }
            stream = null;
            contentLoader = null;
            return false;
        }

        public void Dispose()
        {
            foreach (var package in cachedPackages.Values)
                package.Dispose();
            cachedPackages.Clear();
        }
    }
}