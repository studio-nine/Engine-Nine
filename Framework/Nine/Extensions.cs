#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Linq;
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
        public static T GetService<T>(this IServiceProvider provider)
        {
            var result = provider.GetService(typeof(T));
            return result is T ? (T)result : default(T);
        }

        public static K GetService<T, K>(this IServiceProvider provider) where T : K
        {
            var result = provider.GetService(typeof(T));
            return result is K ? (K)result : default(K);
        }
    }

    /// <summary>
    /// Contains commonly used utility extension methods.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class UtilityExtensions
    {
        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }


#if WINDOWS
        public static string ToContentString(this Matrix matrix)
        {
            return string.Join(" ", matrix.M11, matrix.M12, matrix.M13, matrix.M14
                                  , matrix.M21, matrix.M22, matrix.M23, matrix.M24
                                  , matrix.M31, matrix.M32, matrix.M33, matrix.M34
                                  , matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }
#endif

        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (T item in values)
                collection.Add(item);
        }

        public static void ForEach<T>(this IEnumerable<T> collection, Action<T> action)
        {
            foreach (T item in collection)
                action(item);
        }

        /// <summary>
        /// Tests the target object and its descendants to see if it of type T.
        /// </summary>
        internal static void ForEachRecursive<T>(this IEnumerable target, Action<T> action)
        {
            if (target != null)
            {
                if (target is T)
                    action((T)target);

                ForEachRecursiveInternal<T>(target, action);
            }
        }

        private static void ForEachRecursiveInternal<T>(this IEnumerable target, Action<T> action)
        {
            if (target != null)
            {
                foreach (var item in target)
                {
                    if (item is T)
                        action((T)item);

                    IEnumerable enumerable = item as IEnumerable;
                    if (enumerable != null)
                    {
                        ForEachRecursiveInternal<T>(enumerable, action);
                    }
                }
            }
        }
        
#if WINDOWS && UNSAFE
        internal static unsafe void FastClear(int[] array)
        {
            fixed (int* pArray = array)
            {
                ZeroMemory((byte*)pArray, sizeof(int) * array.Length);
            }
        }

        [System.Runtime.InteropServices.DllImport("KERNEL32.DLL", EntryPoint = "RtlZeroMemory")]
        unsafe static extern bool ZeroMemory(byte* destination, int length);
#else                
        internal static void FastClear(int[] array)
        {
            int blockSize = 4096;
            int index = 0;

            int length = Math.Min(blockSize, array.Length);
            Array.Clear(array, 0, length);

            length = array.Length;
            while (index < length)
            {
                Buffer.BlockCopy(array, 0, array, index, Math.Min(blockSize, length - index));
                index += blockSize;
            }
        }
#endif
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