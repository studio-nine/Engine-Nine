namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// Defines an interface that imports an object from a stream.
    /// </summary>
    public interface IContentImporter
    {
        /// <summary>
        /// Gets an array of supported file extensions.
        /// </summary>
        string[] SupportedFileExtensions { get; }

        /// <summary>
        /// Loads an object from the target stream.
        /// </summary>
        object Import(Stream stream, IServiceProvider serviceProvider);
    }
    
    /// <summary>
    /// Handler to resolve a stream an loader from an asset file.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public interface IContentResolver
    {
        /// <summary>
        /// Gets the stream and loader of the specified asset.
        /// </summary>
        /// <param name="fileName">
        /// The normalized name of the asset including file extension.
        /// </param>
        bool TryResolveContent(string fileName, IServiceProvider serviceProvider, out Stream stream, out IContentImporter contentLoader);
    }

    /// <summary>
    /// Defines a content loader that supports the infrastructure of the loading system.
    /// </summary>
    public class ContentLoader : IServiceProvider, IDisposable
    {
        /// <summary>
        /// Gets a list of search directories that stores the content files.
        /// </summary>
        public IList<string> SearchDirectories
        {
            get { return searchDirectories; }
        }
        private List<string> searchDirectories = new List<string>();

        /// <summary>
        /// Gets the full path to the current asset file name that is being loaded.
        /// This value is only valid within the within the Load or Create method.
        /// </summary>
        internal string CurrentFileName;

        /// <summary>
        /// Gets a list of resolvers that specifies how to resolve a asset name.
        /// </summary>
        public IList<IContentResolver> Resolvers 
        {
            get { return resolvers; }
        }
        private List<IContentResolver> resolvers = new List<IContentResolver>();

        /// <summary>
        /// Gets a collection of additional service objects exposed though the IServiceProvider interface.
        /// </summary>
        public IList<object> Services
        {
            get { return services ?? (services = new List<object>()); }
        }
        private List<object> services;

        private IServiceProvider serviceProvider;
        private Dictionary<string, object> cachedContents;
        private Stack<string> workingPath = new Stack<string>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentLoader"/> class.
        /// </summary>
        public ContentLoader(IServiceProvider serviceProvider)
        {
            this.serviceProvider = serviceProvider;
            this.resolvers.Add(new PackageResolver());
            this.searchDirectories.Add("");
        }

        /// <summary>
        /// Loads an object with the asset name. 
        /// A cached instance will be returned if that asset has already been loaded.
        /// </summary>
        /// <param name="fileName">
        /// Name of the asset usually including file extension.
        /// </param>
        public virtual T Load<T>(string fileName)
        {
            if (workingPath.Count > 0)
                fileName = Path.Combine(workingPath.Peek(), fileName);
            fileName = Extensions.CleanPath(fileName);

            object result;
            if (cachedContents == null)
                cachedContents = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
            if (!cachedContents.TryGetValue(fileName, out result))
                cachedContents[fileName] = result = Create<object>(fileName);
            return (T)result;
        }

        /// <summary>
        /// Asynchronously loads an object with the asset name.
        /// </summary>
        public Task<T> LoadAsync<T>(string fileName)
        {
            // TODO: Lock
            return Task.Factory.StartNew(() => Load<T>(fileName));
        }

        /// <summary>
        /// Creates a new object from the target asset. 
        /// The life cycle of the created instance has to be managed by caller code.
        /// </summary>
        /// <param name="fileName">
        /// Name of the asset usually including file extension.
        /// </param>
        public virtual T Create<T>(string fileName)
        {
            try
            {
                Stream stream;
                IContentImporter importer;

                workingPath.Push(Path.GetDirectoryName(fileName));

                if (!FindStream(fileName, out stream, out importer))
                    throw new FileNotFoundException("Cannot find file: " + fileName);

                return (T)importer.Import(stream, this);
            }
            finally
            {
                workingPath.Pop();
            }
        }

        /// <summary>
        /// Asynchronously creates an object from the target asset.
        /// </summary>
        public Task<T> CreateAsync<T>(string fileName)
        {
            // TODO: Lock
            return Task.Factory.StartNew(() => Create<T>(fileName));
        }

        /// <summary>
        /// Unloads and disposes all the content managed by this ContentLoader.
        /// </summary>
        public void Unload()
        {
            foreach (var asset in cachedContents.Values)
            {
                var disposable = asset as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
            cachedContents = null;
        }

        private bool FindStream(string fileName, out Stream stream, out IContentImporter loader)
        {
            var searchDirectoryCount = searchDirectories.Count;
            for (int i = 0; i < searchDirectoryCount; i++)
            {
                var searchDirectory = string.IsNullOrEmpty(searchDirectories[i]) 
                    ? Directory.GetCurrentDirectory() : Path.GetFullPath(searchDirectories[i]);
                CurrentFileName = Path.Combine(searchDirectory, fileName);
                if (FindStreamInternal(CurrentFileName, out stream, out loader))
                    return true;
            }
            stream = null;
            loader = null;
            return false;
        }

        private bool FindStreamInternal(string fileName, out Stream stream, out IContentImporter loader)
        {
            var resolverCount = resolvers.Count;
            for (int i = 0; i < resolverCount; i++)
            {
                if (resolvers[i].TryResolveContent(fileName, this, out stream, out loader))
                    return true;
            }
            stream = null;
            loader = null;
            return false;
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            object result;
            if (services != null)
            {
                for (int i = 0; i < services.Count; i++)
                {
                    if ((result = services[i]) != null && serviceType.IsAssignableFrom(result.GetType()))
                        return result;
                }
            }

            if (serviceProvider != null && (result = serviceProvider.GetService(serviceType)) != null)
                return result;
            if (serviceType == ContentLoaderType)
                return this;
            return null;
        }
        static readonly Type ContentLoaderType = typeof(ContentLoader);
        
        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
            }
        }
        #endregion
    }
}