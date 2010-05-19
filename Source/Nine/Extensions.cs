#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    #region ServiceProviderExtensions
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ServiceProviderExtensions
    {
        public static T GetService<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService(typeof(T)) as T;
        }

        public static K GetService<T, K>(this IServiceProvider provider)
            where T : class, K
            where K : class
        {
            return provider.GetService(typeof(T)) as K;
        }

        public static K GetService<K>(this IServiceProvider provider, Type type)
            where K : class
        {
            return provider.GetService(type) as K;
        }

        public static K GetService<K>(this IServiceProvider provider, string type)
            where K : class
        {
            return provider.GetService(Type.GetType(type)) as K;
        }
    }
    #endregion
}