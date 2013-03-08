namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;

    class KeyValuePairEqualtyComparer<TKey, TValue> : IEqualityComparer<KeyValuePair<TKey, TValue>>
    {
        public bool Equals(KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y)
        {
            return object.Equals(x.Key, y.Key) && object.Equals(x.Value, y.Value);
        }

        public int GetHashCode(KeyValuePair<TKey, TValue> obj)
        {
            return obj.Key.GetHashCode() ^ obj.Value.GetHashCode();
        }
    }

    class GraphicsResources<T> where T : class
    {
        static T defaultInstance;
        static GraphicsDevice defaultGraphics;
        static Dictionary<KeyValuePair<GraphicsDevice, Type>, WeakReference<T>> resourceDictionary =
           new Dictionary<KeyValuePair<GraphicsDevice, Type>, WeakReference<T>>(new KeyValuePairEqualtyComparer<GraphicsDevice, Type>());

        public static T GetInstance(GraphicsDevice graphics)
        {
            return GetInstance(graphics, typeof(void));
        }

        public static T GetInstance(GraphicsDevice graphics, Type ownerType)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (defaultGraphics == null)
                return defaultInstance = (T)Activator.CreateInstance(typeof(T), defaultGraphics = graphics);
            if (defaultGraphics == graphics)
                return defaultInstance;

            T result;
            WeakReference<T> value;
            KeyValuePair<GraphicsDevice, Type> key = new KeyValuePair<GraphicsDevice, Type>(graphics, ownerType);
            if (resourceDictionary.TryGetValue(key, out value))
            {
                if (value.TryGetTarget(out result))
                    return result;

                // Need to remove it from the dictionary
                resourceDictionary.Remove(key);
            }
            
            result = (T)Activator.CreateInstance(typeof(T), graphics);
            resourceDictionary.Add(key, new WeakReference<T>(result));
            return result;
        }
    }
}
