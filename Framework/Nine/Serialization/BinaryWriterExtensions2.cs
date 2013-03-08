namespace Nine.Serialization
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.IO;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Contains extension methods for BinaryWriter.
    /// </summary>
    public static partial class BinaryWriterExtensions
    {
        static partial void BeforeWriteObject(ref object value, IServiceProvider serviceProvider)
        {
            object serializedObject;
            var designTimePropertyStore = serviceProvider.TryGetService<ISerializationOverride>();
            if (designTimePropertyStore != null && designTimePropertyStore.TryGetOverride(value, out serializedObject))
                value = serializedObject;
        }

        public static void Write(this BinaryWriter output, Color value)
        {
            output.Write(value.PackedValue);
        }

        public static void Write(this BinaryWriter output, Vector2 value)
        {
            output.Write(value.X);
            output.Write(value.Y);
        }

        public static void Write(this BinaryWriter output, Vector3 value)
        {
            output.Write(value.X);
            output.Write(value.Y);
            output.Write(value.Z);
        }

        public static void Write(this BinaryWriter output, Vector4 value)
        {
            output.Write(value.X);
            output.Write(value.Y);
            output.Write(value.Z);
            output.Write(value.W);
        }

        public static void Write(this BinaryWriter output, Quaternion value)
        {
            output.Write(value.X);
            output.Write(value.Y);
            output.Write(value.Z);
            output.Write(value.W);
        }

        public static void Write(this BinaryWriter output, Matrix value)
        {
            output.Write(value.M11); output.Write(value.M12); output.Write(value.M13); output.Write(value.M14);
            output.Write(value.M21); output.Write(value.M22); output.Write(value.M23); output.Write(value.M24);
            output.Write(value.M31); output.Write(value.M32); output.Write(value.M33); output.Write(value.M34);
            output.Write(value.M41); output.Write(value.M42); output.Write(value.M43); output.Write(value.M44);
        }

        public static void Write(this BinaryWriter output, BoundingBox value)
        {
            output.Write(value.Min.X); output.Write(value.Min.Y); output.Write(value.Min.Z);
            output.Write(value.Max.X); output.Write(value.Max.Y); output.Write(value.Max.Z);
        }

        public static void Write(this BinaryWriter output, Point value)
        {
            output.Write(value.X);
            output.Write(value.Y);
        }

        public static void Write(this BinaryWriter output, Rectangle value)
        {
            output.Write(value.X);
            output.Write(value.Y);
            output.Write(value.Width);
            output.Write(value.Height);
        }

        public static void Write<T>(this BinaryWriter output, Range<T> value, Action<T> write)
        {
            write(value.Min);
            write(value.Max);
        }
    }
}