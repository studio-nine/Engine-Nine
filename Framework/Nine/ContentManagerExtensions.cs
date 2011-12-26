#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine
{
    /// <summary>
    /// Contains extension methods for ContentManager.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContentManagerExtensions
    {
        static Type[] ReadAssetParameterTypes = new Type[] { typeof(string), typeof(Action<IDisposable>) };

        /// <summary>
        /// Creates a new instance of an asset that has been processed by the Content Pipeline.
        /// </summary>
        public static T Create<T>(this ContentManager content, string assetName)
        {
            if (content is ContentFactory)
            {
                return ((ContentFactory)content).CreateInternal<T>(assetName);
            }

#if WINDOWS_PHONE
            throw new NotSupportedException("ContentManager.Create only works for ObjectFactoryContentManager");
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
    /// Defines a content manager that can create a new instance for the same asset name.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ContentFactory : ContentManager
    {
        public ContentFactory(IServiceProvider services) : base(services) { }
        public ContentFactory(ContentManager innerContent) : base(innerContent.ServiceProvider) 
        {
            RootDirectory = innerContent.RootDirectory;
        }

        internal T CreateInternal<T>(string assetName)
        {
            return base.ReadAsset<T>(assetName, null);
        }
    }

    class ContentObjectFactory : IObjectFactory
    {
        ContentManager content;
        public ContentObjectFactory(ContentManager innerContent)
        {
            content = innerContent;
        }

        public T Create<T>(string typeName)
        {
            return content.Create<T>(typeName);
        }
    }
}