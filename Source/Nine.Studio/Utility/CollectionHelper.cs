namespace Nine.Studio
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Nine.Studio.Extensibility;

    internal static class CollectionHelper
    {
        public static void AddRange<T>(this ICollection<T> collection, IEnumerable<T> values)
        {
            foreach (T item in values)
                collection.Add(item);
        }

        public static void ForEach<T>(this IList<T> collection, Action<T> action)
        {
            // Avoid modify collection during enumeration
            for (int i = 0; i < collection.Count; i++)
                action(collection[i]);
        }
    }
}
