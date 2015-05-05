namespace Nine.Serialization
{
    using System;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Collections;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Contains extension methods for BinaryReader.
    /// </summary>
    public static partial class BinaryReaderExtensions
    {
        static partial void AfterReadObject(ref object value, IServiceProvider serviceProvider)
        {

        }

        public static Color ReadColor(this BinaryReader input)
        {
            return new Color { PackedValue = input.ReadUInt32() };
        }

        public static Vector2 ReadVector2(this BinaryReader input)
        {
            return new Vector2 { X = input.ReadSingle(), Y = input.ReadSingle() };
        }

        public static Vector3 ReadVector3(this BinaryReader input)
        {
            return new Vector3 { X = input.ReadSingle(), Y = input.ReadSingle(), Z = input.ReadSingle() };
        }

        public static Vector4 ReadVector4(this BinaryReader input)
        {
            return new Vector4 { X = input.ReadSingle(), Y = input.ReadSingle(), Z = input.ReadSingle(), W = input.ReadSingle() };
        }

        public static Quaternion ReadQuaternion(this BinaryReader input)
        {
            return new Quaternion { X = input.ReadSingle(), Y = input.ReadSingle(), Z = input.ReadSingle(), W = input.ReadSingle() };
        }

        public static Matrix ReadMatrix(this BinaryReader input)
        {
            return new Matrix
            {
                M11 = input.ReadSingle(), M12 = input.ReadSingle(), M13 = input.ReadSingle(), M14 = input.ReadSingle(),
                M21 = input.ReadSingle(), M22 = input.ReadSingle(), M23 = input.ReadSingle(), M24 = input.ReadSingle(),
                M31 = input.ReadSingle(), M32 = input.ReadSingle(), M33 = input.ReadSingle(), M34 = input.ReadSingle(),
                M41 = input.ReadSingle(), M42 = input.ReadSingle(), M43 = input.ReadSingle(), M44 = input.ReadSingle(),
            };
        }

        public static BoundingBox ReadBoundingBox(this BinaryReader input)
        {
            return new BoundingBox
            {
                Min = new Vector3(input.ReadSingle(), input.ReadSingle(), input.ReadSingle()),
                Max = new Vector3(input.ReadSingle(), input.ReadSingle(), input.ReadSingle()),
            };
        }

        public static Point ReadPoint(this BinaryReader input)
        {
            return new Point(input.ReadInt32(), input.ReadInt32());
        }

        public static Rectangle ReadRectangle(this BinaryReader input)
        {
            return new Rectangle(input.ReadInt32(), input.ReadInt32(), input.ReadInt32(), input.ReadInt32());
        }

        public static Range<T> ReadRange<T>(this BinaryReader input, Func<T, T> read)
        {
            return new Range<T> { Min = read(default(T)), Max = read(default(T)) };
        }
    }
}