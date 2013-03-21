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
    public class PackageResolver : IContentResolver, IDisposable
    {
        public const string FileExtension = ".n";

        private BinarySerializer binaryLoader = new BinarySerializer();
        private Dictionary<string, ZipArchive> cachedPackages = new Dictionary<string, ZipArchive>(StringComparer.OrdinalIgnoreCase);

        public bool TryResolveContent(string fileName, IServiceProvider serviceProvider, out Stream stream, out IContentImporter contentImporter)
        {
            ZipArchive package;
            ZipArchiveEntry entry;
            string packageFilename;
            string packageDirectory = fileName;

            while (!string.IsNullOrEmpty(packageDirectory = Path.GetDirectoryName(packageDirectory)))
            {
                if (!cachedPackages.TryGetValue(packageDirectory, out package))
                {
                    cachedPackages[packageDirectory] = package =
                        Directory.Exists(packageDirectory) &&
                        File.Exists(packageFilename = packageDirectory + FileExtension) ?
                        new ZipArchive(File.OpenRead(packageFilename)) : null;
                }

                if (package == null)
                    continue;

                if ((entry = package.GetEntry(fileName.Substring(packageDirectory.Length + 1))) != null)
                {
                    stream = entry.Open();
                    contentImporter = binaryLoader;
                    return true;
                }
            }
            stream = null;
            contentImporter = null;
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