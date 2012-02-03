#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines an circle-shaped 2D volume.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    public struct BoundingCircle : IEquatable<BoundingCircle>
    {
        /// <summary>
        /// Gets or sets the center of the circle.
        /// </summary>
        public Vector2 Center;

        /// <summary>
        /// Gets or sets the radius of the circle.
        /// </summary>
        public float Radius;

        /// <summary>
        /// Create a new instance of BoundingCircle object.
        /// </summary>
        public BoundingCircle(Vector2 center, float radius)
        {
            Center = center;
            Radius = radius;
        }

        /// <summary>
        /// Create a new instance of BoundingCircle object.
        /// </summary>
        public BoundingCircle(BoundingSphere sphere)
        {
            Center = new Vector2(sphere.Center.X, sphere.Center.Y);
            Radius = sphere.Radius;
        }

        /// <summary>
        /// Tests whether the BoundingCircle contains a point.
        /// </summary>
        public ContainmentType Contains(float x, float y)
        {
            return Math2D.PointInCircle(new Vector2(x, y), Center, Radius)
                ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingCircle contains a point.
        /// </summary>
        public ContainmentType Contains(Vector2 point)
        {
            return Math2D.PointInCircle(point, Center, Radius)
                ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingCircle contains another BoundingCircle.
        /// </summary>
        public ContainmentType Contains(BoundingCircle circle)
        {
            return Math2D.CircleIntersects(Center, Radius, circle.Center, circle.Radius);
        }

        public bool Equals(BoundingCircle other)
        {
            return Center == other.Center && Radius == other.Radius;
        }

        public override bool Equals(object obj)
        {
            if (obj is BoundingCircle)
                return Equals((BoundingCircle)obj);
            
            return false;
        }

        public static bool operator ==(BoundingCircle value1, BoundingCircle value2)
        {
            return ((value1.Center == value2.Center) && (value1.Radius == value2.Radius));
        }

        public static bool operator !=(BoundingCircle value1, BoundingCircle value2)
        {
            return !(value1.Center == value2.Center && value1.Radius == value2.Radius);
        }

        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ Radius.GetHashCode();
        }

        public override string ToString()
        {
            return Center.ToString() + " ~ " + Radius.ToString();
        }

        /// <summary>
        /// Creates a BoundingCircle that can contain a specified list of points.
        /// </summary>
        public static BoundingCircle CreateFromPoints(IEnumerable<Vector2> points)
        {
            return new BoundingCircle(BoundingSphere.CreateFromPoints(points.Select(pt => new Vector3(pt, 0))));
        }
    }
}