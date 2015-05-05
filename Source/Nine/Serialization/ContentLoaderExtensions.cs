namespace Nine.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Xna.Framework;
    using System.Collections;

    /// <summary>
    /// Contains extension methods for ContentLoaders.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ContentLoaderExtensions
    {
        public static T Load<T>(this IContentImporter loader, string fileName, IServiceProvider serviceProvider) where T : class
        {
            // TODO: 
            //using (var stream = File.OpenRead(fileName))
            //{
            //    return loader.Import(stream, serviceProvider) as T;
            //}

            return null;
        }

        public static T Load<T>(this IContentImporter loader, Stream stream, IServiceProvider serviceProvider) where T : class
        {
            return loader.Import(stream, serviceProvider) as T;
        }
    }
}