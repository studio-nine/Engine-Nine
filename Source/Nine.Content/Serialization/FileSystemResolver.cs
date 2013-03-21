namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;

    /// <summary>
    /// Resolves content from the file system.
    /// </summary>
    /// <remarks>
    /// FileSystemResolver finds the asset based on filenames. 
    /// When an asset file is found, it first examines whether there is a corresponding
    /// side by side *.importer file. The *.importer file contains a XAML representation
    /// of the importer used to load the asset. If the *.importer is not found, 
    /// FileSystemResolver will choose an IContentImporter that supports the asset file extension.
    /// </remarks>
    public class FileSystemResolver : IContentResolver
    {
        private XamlSerializer xaml = new XamlSerializer();
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
                        throw new InvalidOperationException(string.Format("{0} conflicts with {1}", importer, existingImporter));
                    importers.Add(ext, importer);
                }
            }
        }

        public bool TryResolveContent(string fileName, IServiceProvider serviceProvider, out Stream stream, out IContentImporter contentImporter)
        {
            stream = null;
            contentImporter = null;

            if (!File.Exists(fileName))
                return false;

            stream = File.OpenRead(fileName);

            var importerFile = fileName + ".importer";
            if (File.Exists(importerFile))
            {
                contentImporter = xaml.Load<IContentImporter>(importerFile, serviceProvider);
            }
            else if (!importers.TryGetValue(Path.GetExtension(fileName), out contentImporter))
            {
                throw new InvalidOperationException("Cannot find importer for file: " + fileName);
            }

            return true;
        }
    }
}