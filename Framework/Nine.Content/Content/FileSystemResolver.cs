namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;

    /// <summary>
    /// Resolves content from the file system.
    /// </summary>
    public class FileSystemResolver : IContentResolver
    {
        private Dictionary<string, IContentImporter> importers = new Dictionary<string, IContentImporter>();

        public FileSystemResolver()
        {
            foreach (var importer in Extensions.FindImplementations<IContentImporter>())
            {
                var supportedFileExtensions = importer.SupportedFileExtensions;
                if (supportedFileExtensions == null)
                    continue;

                foreach (var ext in supportedFileExtensions)
                {
                    IContentImporter existingImporter = null;
                    if (importers.TryGetValue(ext, out existingImporter))
                        throw new InvalidOperationException(
                            string.Format("{0} conflicts with {1}", importer, existingImporter));
                    importers.Add(ext, importer);
                }
            }
        }

        public bool TryResolveContent(string assetName, IServiceProvider serviceProvider, out Stream stream, out IContentImporter contentLoader)
        {
            if (Path.IsPathRooted(assetName))
            {
                foreach (var extension in importers.Keys)
                {
                    var fileName = assetName + extension;
                    if (File.Exists(fileName))
                    {
                        stream = File.OpenRead(fileName);
                        contentLoader = importers[extension];
                        return true;
                    }
                }
            }

            foreach (var extension in importers.Keys)
            {
                var fileName = assetName + extension;
                if (File.Exists(fileName))
                {
                    stream = File.OpenRead(fileName);
                    contentLoader = importers[extension];
                    return true;
                }
            }

            stream = null;
            contentLoader = null;
            return false;
        }
    }
}