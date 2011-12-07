#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using System.Text;
using Nine.Design;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a 3D transform.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    [TypeConverter(typeof(TransformConverter))]
    public struct Transform : IEquatable<Transform>
    {
        /// <summary>
        /// Gets or sets the scale.
        /// </summary>
        public Vector3 Scale;
        
        /// <summary>
        /// Gets or sets rotation on x, y, z axis.
        /// </summary>
        public Vector3 Rotation;

        /// <summary>
        /// Gets or sets the order of rotation.
        /// </summary>
        public RotationOrder RotationOrder;
        
        /// <summary>
        /// Gets or sets the translation
        /// </summary>
        public Vector3 Translation;

        /// <summary>
        /// Gets the identity transform.
        /// </summary>
        public static readonly Transform Identity = new Transform { Scale = Vector3.One };

        /// <summary>
        /// Checks whether the current Triangle intersects a Ray.
        /// </summary>
        public Matrix ToMatrix()
        {
            var value = new Matrix();
            value.M11 = Scale.X;
            value.M22 = Scale.Y;
            value.M33 = Scale.Z;

            Matrix temp;
            if (RotationOrder == RotationOrder.Zxy)
            {
                Matrix.CreateFromYawPitchRoll(Rotation.Y, Rotation.X, Rotation.Z, out temp);
            }
            else
            {
                Matrix temp2;
                Matrix.CreateRotationY(Rotation.Y, out temp);
                Matrix.CreateRotationX(Rotation.X, out temp2);
                Matrix.Multiply(ref temp, ref temp2, out temp);
                Matrix.CreateRotationZ(Rotation.Z, out temp2);
                Matrix.Multiply(ref temp, ref temp2, out temp);
            }
            Matrix.Multiply(ref value, ref temp, out value);

            value.M41 = Translation.X;
            value.M42 = Translation.Y;
            value.M43 = Translation.Z;
            return value;
        }
        
        public bool Equals(Transform other)
        {
            return Scale == other.Scale && Rotation == other.Rotation &&
                   RotationOrder == other.RotationOrder && Translation == other.Translation;
        }

        public override bool Equals(object obj)
        {
            if (obj is Transform)
                return Equals((Transform)obj);

            return false;
        }

        public static bool operator ==(Transform value1, Transform value2)
        {
            return ((value1.Scale == value2.Scale) && (value1.Rotation == value2.Rotation) &&
                    (value1.Translation == value2.Translation) && (value1.RotationOrder == value2.RotationOrder));
        }

        public static bool operator !=(Transform value1, Transform value2)
        {
            return !(value1.Scale == value2.Scale && value1.Rotation == value2.Rotation &&
                     value1.Translation == value2.Translation && value1.RotationOrder == value2.RotationOrder);
        }

        public override int GetHashCode()
        {
            return Scale.GetHashCode() + Rotation.GetHashCode() + Translation.GetHashCode() + RotationOrder.GetHashCode();
        }

        public override string ToString()
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(Scale.X);
            builder.Append(", ");
            builder.Append(Scale.Y);
            builder.Append(", ");
            builder.Append(Scale.Z);
            builder.Append(";");
            builder.Append(Rotation.X);
            builder.Append(", ");
            builder.Append(Rotation.Y);
            builder.Append(", ");
            builder.Append(Rotation.Z);
            builder.Append(";");
            builder.Append(Translation.X);
            builder.Append(", ");
            builder.Append(Translation.Y);
            builder.Append(", ");
            builder.Append(Translation.Z);
            builder.Append(";");
            builder.Append(RotationOrder.ToString());
            return builder.ToString();
        }
    }
}