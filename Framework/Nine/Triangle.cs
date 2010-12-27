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
        /// Gets or sets the first vertex.
        /// </summary>
        public Vector3 V2;
        
        /// <summary>
        /// Gets or sets the first vertex.
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
        /// Determines if a Vector3 equals to another Vector3.
        /// </summary>
        public bool Equals(Triangle other)
        {
            return V1.Equals(other.V1) && V2.Equals(other.V2) && V3.Equals(other.V3);
        }

        public override string ToString()
        {
            return V1.ToString() + "|" + V2.ToString() + "|" + V3.ToString();
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
        /// Checks whether the current Triangle intersects a line segment.
        /// </summary>
        public bool Intersects(Vector3 v1, Vector3 v2)
        {
            Vector3 dir;
            Vector3.Subtract(ref v2, ref v1, out dir);

            float length = dir.Length();
            if (length <= 0)
                return false;

            float inv = 1.0f / length;
            dir.X *= inv;
            dir.Y *= inv;
            dir.Z *= inv;

            float? result;
            RayExtensions.Intersects(new Ray(v1, dir), ref V1, ref V2, ref V3, out result);

            return result.HasValue && result.Value <= length;
        }
    }
}