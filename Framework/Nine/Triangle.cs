#region Copyright 2008 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a 3D triangle made up of tree vertices.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    public struct Triangle : IEquatable<Triangle>
    {
        /// <summary>
        /// Gets or sets the first vertex.
        /// </summary>
        public Vector3 V1;
        
        /// <summary>
        /// Gets or sets the second vertex.
        /// </summary>
        public Vector3 V2;
        
        /// <summary>
        /// Gets or sets the third vertex.
        /// </summary>
        public Vector3 V3;

        /// <summary>
        /// Create a new instance of Triangle.
        /// </summary>
        public Triangle(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }
        
        /// <summary>
        /// Checks whether the current Triangle intersects a Ray.
        /// </summary>
        public float? Intersects(Ray ray)
        {
            float? result;
            RayExtensions.Intersects(ray, ref V1, ref V2, ref V3, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the current Triangle intersects a Ray.
        /// </summary>
        public void Intersects(ref Ray ray, out float? result)
        {
            RayExtensions.Intersects(ray, ref V1, ref V2, ref V3, out result);
        }
        
        /// <summary>
        /// Checks whether the current Triangle intersects a line segment.
        /// </summary>
        /// <returns>
        /// The distance between the intersection point and v1.
        /// </returns>
        public float? Intersects(Vector3 v1, Vector3 v2)
        {
            float? result;
            Intersects(ref v1, ref v2, out result);
            return result;
        }

        /// <summary>
        /// Checks whether the current Triangle intersects a line segment.
        /// </summary>
        public void Intersects(ref Vector3 v1, ref Vector3 v2, out float? result)
        {
            const float Epsilon = 1E-10F;

            Vector3 dir;
            Vector3.Subtract(ref v2, ref v1, out dir);

            float length = dir.Length();
            if (length <= Epsilon)
            {
                result = null;
                return;
            }

            float inv = 1.0f / length;
            dir.X *= inv;
            dir.Y *= inv;
            dir.Z *= inv;

            RayExtensions.Intersects(new Ray(v1, dir), ref V1, ref V2, ref V3, out result);
            if (result.HasValue && result.Value > length)
                result = null;
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other"/> parameter; otherwise, false.
        /// </returns>
        public bool Equals(Triangle other)
        {
            return V1 == other.V1 && V2 == other.V2 && V3 == other.V3;
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            if (obj is Triangle)
                return Equals((Triangle)obj);

            return false;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(Triangle value1, Triangle value2)
        {
            return ((value1.V1 == value2.V1) && (value1.V2 == value2.V2) && (value1.V3 == value2.V3));
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="value1">The value1.</param>
        /// <param name="value2">The value2.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(Triangle value1, Triangle value2)
        {
            return !(value1.V1 == value2.V1 && value1.V2 == value2.V2 && value1.V3 == value2.V3);
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            return V1.GetHashCode() ^ V2.GetHashCode() ^ V3.GetHashCode();
        }

        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String"/> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return V1.ToString() + "|" + V2.ToString() + "|" + V3.ToString();
        }
    }
}