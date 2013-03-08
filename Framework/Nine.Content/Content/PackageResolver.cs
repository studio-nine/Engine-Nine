namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Windows.Markup;

    /// <summary>
    /// Defines a content resolver that resolves content from packages.
    /// </summary>
    public class PackageResolver : IContentResolver
    {
        private XamlSerializer serializer = new XamlSerializer();
        private Dictionary<string, Package> cachedPackages = new Dictionary<string, Package>(StringComparer.OrdinalIgnoreCase);

        public bool TryResolveContent(string assetName, IServiceProvider serviceProvider, out Stream stream, out IContentImporter contentLoader)
        {
            Package package;
            string packageFilename;
            string packageDirectory = assetName;

            while (!string.IsNullOrEmpty(packageDirectory = Path.GetDirectoryName(packageDirectory)))
            {
                if (!cachedPackages.TryGetValue(packageDirectory, out package))
                {
                    if (!Directory.Exists(packageDirectory))
                        continue;
                    if (!File.Exists(packageFilename = packageDirectory + Package.FileExtension))
                        continue;
                    cachedPackages[packageDirectory] = package = serializer.Load<Package>(packageFilename, serviceProvider);
                }
                
                foreach (var packageItem in package.Items)
                {
                    var itemAssetName = Path.GetFileNameWithoutExtension(packageItem.Source);
                    if (string.Equals(assetName, Path.Combine(packageDirectory, package.Name, Path.ChangeExtension(packageItem.Source, "")), StringComparison.OrdinalIgnoreCase))
                    {
                        stream = File.OpenRead(Path.Combine(packageDirectory, package.Name, packageItem.Source));
                        contentLoader = packageItem.Loader;
                        return true;
                    }
                }
            }
            stream = null;
            contentLoader = null;
            return false;
        }
    }
}