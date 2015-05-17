namespace Nine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using System.Reflection;
    using System.Linq;
    using System.IO;
    
    static class Extensions
    {
        public static T GetService<T>(this IServiceProvider provider) where T : class
        {
            var result = provider.GetService(typeof(T)) as T;
            if (result == null)
                throw new InvalidOperationException(
                    "Cannot find service " + typeof(T).ToString());
            return result;
        }

        public static T TryGetService<T>(this IServiceProvider provider) where T : class
        {
            return provider.GetService(typeof(T)) as T;
        }

        public static Vector2 ToVector2(this Point point)
        {
            return new Vector2(point.X, point.Y);
        }

        public static Point ToPoint(this Vector2 vector)
        {
            return new Point((int)vector.X, (int)vector.Y);
        }

        public static string ToContentString(this Matrix matrix)
        {
            return string.Join(" ", matrix.M11, matrix.M12, matrix.M13, matrix.M14
                                  , matrix.M21, matrix.M22, matrix.M23, matrix.M24
                                  , matrix.M31, matrix.M32, matrix.M33, matrix.M34
                                  , matrix.M41, matrix.M42, matrix.M43, matrix.M44);
        }

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
        public static void ForEachRecursive<T>(this IEnumerable target, Action<T> action)
        {
            if (target != null)
            {
                if (target is T)
                    action((T)target);

                ForEachRecursiveInternal<T>(target, action);
            }
        }

        public static bool TryGetAttachedProperty<T>(object tag, string propertyName, out T propertyValue)
        {
            object value;
            var dictionary = tag as IDictionary<string, object>;
            if (dictionary != null && dictionary.TryGetValue(propertyName, out value) && value is T)
            {
                propertyValue = (T)value;
                return true;
            }
            propertyValue = default(T);
            return false;
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

#if !PCL
        public static string CleanPath(string path)
        {
            if (path == null)
                return null;
            if (Path.IsPathRooted(path))
                return Path.GetFullPath(path);
            return Path.GetFullPath(Path.Combine("N:\\", path)).Substring(3);
        }

        public static string MakeRelativePath(string fromPath, string toPath)
        {
            var fromUri = new Uri(fromPath);
            var toUri = new Uri(toPath);

            var relativeUri = fromUri.MakeRelativeUri(toUri);
            var relativePath = Uri.UnescapeDataString(relativeUri.ToString());

            return relativePath.Replace('/', Path.DirectorySeparatorChar);
        }
#endif

        public static int UpperPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

#if !PCL
        public static IEnumerable<Type> FindImplementations(Type baseType)
        {
            Type[] types;
            Assembly[] assemblies;

            try { assemblies = AppDomain.CurrentDomain.GetAssemblies(); }
            catch { yield break; }

            foreach (var assembly in assemblies)
            {
                try { types = assembly.GetTypes(); }
                catch { continue; }

                foreach (var type in types)
                {
                    if (!type.IsAbstract && !type.IsInterface &&
                        !type.IsGenericType && !type.IsGenericTypeDefinition &&
                        baseType.IsAssignableFrom(type))
                    {
                        yield return type;
                    }
                }
            }
        }

        public static IEnumerable<T> FindImplementations<T>()
        {
            foreach (var type in FindImplementations(typeof(T)))
            {
                var constructor = type.GetConstructor(BindingFlags, null, Type.EmptyTypes, null);
                if (constructor != null)
                    yield return (T)constructor.Invoke((object[])null);
            }
        }

        public static IEnumerable<T> FindImplementations<T>(IServiceProvider serviceProvider)
        {
            foreach (var type in FindImplementations(typeof(T)))
            {
                var result = CreateInstance<T>(type, serviceProvider);
                if (result != null)
                    yield return result;
            }
        }

        public static T CreateInstance<T>(Type type, IServiceProvider serviceProvider)
        {
            var constructor = type.GetConstructor(BindingFlags, null, ServiceProviderTypes, null);
            if (constructor != null)
                return (T)constructor.Invoke(new object[] { serviceProvider });
            if ((constructor = type.GetConstructor(BindingFlags, null, Type.EmptyTypes, null)) != null)
                return (T)constructor.Invoke((object[])null);
            return default(T);
        }

        private static BindingFlags BindingFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly | BindingFlags.Instance;
        public static Type[] ServiceProviderTypes = new Type[] { typeof(IServiceProvider) };

#endif

        internal static T TryInvokeContentPipelineMethod<T>(string className, string methodName, params object[] paramters)
        {
#if WINDOWS
            if (PipelineAssembly == null)
                return default(T);
            var flags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.InvokeMethod;
            return (T)PipelineAssembly.GetTypes().Single(type => type.Name == className).InvokeMember(methodName, flags, null, null, paramters);
#else
            return default(T);
#endif
        }
#if !PCL
        static Assembly PipelineAssembly = AppDomain.CurrentDomain.GetAssemblies().SingleOrDefault(assembly => assembly.GetName().Name == "Nine.Content");
#endif
    }
}