namespace Nine.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Collections;

    /// <summary>
    /// Contains extension methods for BinaryReader.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class BinaryReaderExtensions
    {
        public static Type ReadType(this BinaryReader input)
        {
            return Type.GetType(input.ReadString());
        }

        public static DateTime ReadDateTime(this BinaryReader input)
        {
            return DateTime.FromBinary(input.ReadInt64());
        }

        public static TimeSpan ReadTimeSpan(this BinaryReader input)
        {
            return TimeSpan.FromTicks(input.ReadInt64());
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static object ReadObject(this BinaryReader input, object existingInstance, IServiceProvider serviceProvider)
        {
            var binaryObjectSerializer = serviceProvider.GetService(typeof(IBinaryObjectSerializer)) as IBinaryObjectSerializer;
            if (binaryObjectSerializer == null)
                throw new InvalidOperationException("Cannot find IBinaryObjectSerializer service.");
            var result = binaryObjectSerializer.ReadObject(input, existingInstance, serviceProvider);
            AfterReadObject(ref result, serviceProvider);
            return result;
        }

        static partial void AfterReadObject(ref object value, IServiceProvider serviceProvider);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static T[] ReadArray<T>(this BinaryReader input, T[] existingInstance, Func<T, T> read)
        {
            var count = input.ReadInt32();
            if (count < 0)
                return null;
            if (existingInstance == null || existingInstance.Length != count)
                existingInstance = new T[count];
            for (int i = 0; i < count; i++)
                existingInstance[i] = read(existingInstance[i]);
            return existingInstance;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Nullable<T> ReadNullable<T>(this BinaryReader input, Func<T, T> read) where T : struct
        {
            return input.ReadByte() != 0 ? new Nullable<T>(read(default(T))) : null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void ReadCollection<T>(this BinaryReader input, ICollection<T> existingInstance, Func<T, T> read)
        {
            var count = input.ReadInt32();
            if (count <= 0)
                return;

            if (existingInstance == null || existingInstance.Count <= 0)
            {
                for (int i = 0; i < count; i++)
                {
                    var element = read(default(T));
                    if (existingInstance != null)
                        existingInstance.Add(element);
                }
            }
            else
            {
                lock (Buffer<T>.SyncRoot)
                {
                    Buffer<T>.EnsureCapacity(existingInstance.Count);
                    existingInstance.CopyTo(Buffer<T>.Elements, 0);
                    existingInstance.Clear();

                    for (int i = 0; i < count; i++)
                    {
                        var element = read(i < Buffer<T>.Elements.Length ? Buffer<T>.Elements[i] : default(T));
                        existingInstance.Add(element);
                    }
                }
            }
        }

        struct Buffer<T>
        {
            public static T[] Elements;
            public static object SyncRoot = new object();
            public static void EnsureCapacity(int count)
            {
                if (Elements == null || Elements.Length < count)
                    Elements = new T[Math.Max(8, count)];
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static KeyValuePair<TKey, TValue> ReadKeyValuePair<TKey, TValue>(this BinaryReader input, KeyValuePair<TKey, TValue> existingInstance, Func<TKey, TKey> readKey, Func<TValue, TValue> readValue)
        {
            return new KeyValuePair<TKey, TValue>(readKey(existingInstance.Key), readValue(existingInstance.Value));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static List<T> ReadList<T>(this BinaryReader input, Func<T, T> read)
        {
            var count = input.ReadInt32();
            if (count >= 0)
            {
                var result = new List<T>(count);
                for (int i = 0; i < count; i++)
                    result.Add(read(default(T)));
                return result;
            }
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static Dictionary<TKey, TValue> ReadDictonary<TKey, TValue>(this BinaryReader input, Dictionary<TKey, TValue> existingInstance, Func<TKey, TKey> readKey, Func<TValue, TValue> readValue)
        {
            var count = input.ReadInt32();
            if (count >= 0)
            {
                var result = new Dictionary<TKey, TValue>(count);
                for (int i = 0; i < count; i++)
                    result.Add(readKey(default(TKey)), readValue(default(TValue)));
                return result;
            }
            return null;
        }
    }
}