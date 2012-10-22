namespace Nine
{
    using System;
    using System.ComponentModel;
    using System.Reflection;
    using Microsoft.Xna.Framework.Content;
    
    /// <summary>
    /// Contains extension methods for ContentManager.
    /// </summary>
    static class ContentManagerExtensions
    {
        static Type[] ReadAssetParameterTypes = new Type[] { typeof(string), typeof(Action<IDisposable>) };

        /// <summary>
        /// Creates a new instance of an asset that has been processed by the Content Pipeline.
        /// </summary>
        public static T Create<T>(this ContentManager content, string assetName)
        {
            if (content is ContentLoader)
            {
                return ((ContentLoader)content).CreateInternal<T>(assetName);
            }

#if WINDOWS_PHONE
            throw new NotSupportedException("ContentManager.Create only works for ContentLoader");
#elif WINRT
            return content.ReadAsset<T>(assetName);
#else
            // Hack into ReadAsset using reflection.
            var readAsset = content.GetType().GetMethod("ReadAsset", BindingFlags.Instance | BindingFlags.NonPublic, null, ReadAssetParameterTypes, null);
            var genericReadAsset = readAsset.MakeGenericMethod(typeof(T));
            try
            {
                return (T)genericReadAsset.Invoke(content, new object[] { assetName, null });
            }
            catch (TargetInvocationException e)
            {
                throw e.InnerException;
            }
#endif
        }
    }

    /// <summary>
    /// Defines a content loader that supports the infrastructure of the loading system.
    /// </summary>
    public class ContentLoader : ContentManager
    {
        public ContentLoader(IServiceProvider services) : base(services) 
        {
            Group.EnsureDefaultServiceProvider(this);
        }

        public ContentLoader(ContentManager innerContent) : this(innerContent.ServiceProvider) 
        {
            RootDirectory = innerContent.RootDirectory;
        }

        internal T CreateInternal<T>(string assetName)
        {
            return base.ReadAsset<T>(assetName, null);
        }
    }
}