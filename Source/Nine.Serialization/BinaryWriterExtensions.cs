namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;

    /// <summary>
    /// Contains extension methods for BinaryWriter.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class BinaryWriterExtensions
    {
        public static void Write(this BinaryWriter output, Type value)
        {
            output.Write(value.AssemblyQualifiedName);
        }

        public static void Write(this BinaryWriter output, DateTime value)
        {
            output.Write(value.ToBinary());
        }

        public static void Write(this BinaryWriter output, TimeSpan value)
        {
            output.Write(value.Ticks);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Write(this BinaryWriter output, object value, IServiceProvider serviceProvider)
        {
            var binaryObjectSerializer = serviceProvider.GetService(typeof(IBinaryObjectSerializer)) as IBinaryObjectSerializer;
            if (binaryObjectSerializer == null)
                throw new InvalidOperationException("Cannot find IBinaryObjectSerializer service.");
            BeforeWriteObject(ref value, serviceProvider);
            binaryObjectSerializer.WriteObject(output, value, serviceProvider);
        }

        static partial void BeforeWriteObject(ref object value, IServiceProvider serviceProvider);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Write<T>(this BinaryWriter output, T[] value, Action<T> write)
        {
            if (value == null)
            {
                output.Write((int)-1);
            }
            else
            {
                var count = value.Length;
                output.Write(count);
                for (int i = 0; i < count; i++)
                    write(value[i]);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Write<T>(this BinaryWriter output, Nullable<T> value, Action<T> write) where T : struct
        {
            if (value == null)
            {
                output.Write((byte)0);
            }
            else
            {
                output.Write((byte)1);
                write(value.Value);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Write<T>(this BinaryWriter output, ICollection<T> value, Action<T> write)
        {
            if (value == null)
            {
                output.Write((int)0);
            }
            else
            {
                var count = value.Count;
                output.Write(count);
                foreach (var element in value)
                    write(element);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Write<TKey, TValue>(this BinaryWriter output, KeyValuePair<TKey, TValue> value, Action<TKey> writeKey, Action<TValue> writeValue)
        {
            writeKey(value.Key);
            writeValue(value.Value);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Write<T>(this BinaryWriter output, List<T> value, Action<T> write)
        {
            if (value == null)
            {
                output.Write((int)-1);
            }
            else
            {
                var count = value.Count;
                output.Write(count);
                for (int i = 0; i < count; i++)
                    write(value[i]);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void Write<TKey, TValue>(this BinaryWriter output, Dictionary<TKey, TValue> value, Action<TKey> writeKey, Action<TValue> writeValue)
        {
            if (value == null)
            {
                output.Write((int)-1);
            }
            else
            {
                var count = value.Count;
                output.Write(count);
                foreach (var pair in value)
                {
                    writeKey(pair.Key);
                    writeValue(pair.Value);
                }
            }
        }
    }
}