#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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

        /// <summary>
        /// Determines if a Range object equals to another Range object
        /// </summary>
        public bool Equals(BoundingCircle other)
        {
            return Center.Equals(other.Center) && Radius.Equals(other.Radius);
        }

        public override string ToString()
        {
            return Center.ToString() + " ~ " + Radius.ToString();
        }
    }
}