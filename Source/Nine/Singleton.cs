namespace Nine
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

    class Singleton<TKey, TValue>
        where TKey : class
        where TValue : class
    {
        static bool hasDefaultInstance;
        static TValue defaultInstance;
        static TKey defaultKey;
        static Dictionary<KeyValuePair<TKey, Type>, WeakReference<TValue>> resourceDictionary =
           new Dictionary<KeyValuePair<TKey, Type>, WeakReference<TValue>>(new KeyValuePairEqualtyComparer<TKey, Type>());

        public static TValue GetInstance(TKey key)
        {
            return GetInstance(key, typeof(void));
        }

        public static TValue GetInstance(TKey key, Type ownerType)
        {
            if (!hasDefaultInstance)
            {
                hasDefaultInstance = true;
                return defaultInstance = (TValue)Activator.CreateInstance(typeof(TValue), defaultKey = key);
            }

            if (defaultKey == key)
                return defaultInstance;

            TValue result;
            WeakReference<TValue> value;
            KeyValuePair<TKey, Type> currentKey = new KeyValuePair<TKey, Type>(key, ownerType);
            if (resourceDictionary.TryGetValue(currentKey, out value))
            {
                if (value.TryGetTarget(out result))
                    return result;

                // Need to remove it from the dictionary
                resourceDictionary.Remove(currentKey);
            }
            
            result = (TValue)Activator.CreateInstance(typeof(TValue), currentKey);
            resourceDictionary.Add(currentKey, new WeakReference<TValue>(result));
            return result;
        }
    }
}
