#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
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
    /// <summary>
    /// Contains extension methods for IServiceProvider.
    /// </summary>
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


#if !WINDOWS    
    /// <summary>
    /// Mimic the System.ComponentModel.DisplayNameAttribute for .NET Compact Framework.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Event)]
    public class DisplayNameAttribute : Attribute
    {
        public DisplayNameAttribute()
        {
        }

        public DisplayNameAttribute(string displayName)
        {
            DisplayName = displayName;
        }

        public string DisplayName { get; set; }
    }
#endif
}