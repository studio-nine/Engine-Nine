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
    /// Defines an axis-aligned rectangle-shaped 2D volume.
    /// </summary>
#if WINDOWS
    [Serializable()]
#endif
    public struct BoundingRectangle : IEquatable<BoundingRectangle>
    {
        /// <summary>
        /// Gets or sets the min value.
        /// </summary>
        public Vector2 Min;

        /// <summary>
        /// Gets or sets the max value.
        /// </summary>
        public Vector2 Max;

        /// <summary>
        /// Create a new instance of BoundingRectangle object.
        /// </summary>
        public BoundingRectangle(Vector2 min, Vector2 max)
        {
            Min = min;
            Max = max;
        }

        /// <summary>
        /// Create a new instance of BoundingRectangle object.
        /// </summary>
        public BoundingRectangle(Rectangle rectangle)
        {
            Min = new Vector2(rectangle.X, rectangle.Y);
            Max = new Vector2(rectangle.Right, rectangle.Bottom);
        }

        /// <summary>
        /// Create a new instance of BoundingRectangle object.
        /// </summary>
        public BoundingRectangle(BoundingBox box)
        {
            Min = new Vector2(box.Min.X, box.Min.Y);
            Max = new Vector2(box.Max.X, box.Max.Y);
        }

        /// <summary>
        /// Tests whether the BoundingRectangle contains a point.
        /// </summary>
        public ContainmentType Contains(float x, float y)
        {
            return Math2D.PointInRectangle(new Vector2(x, y), Min, Max)
                ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingRectangle contains a point.
        /// </summary>
        public ContainmentType Contains(Vector2 point)
        {
            return Math2D.PointInRectangle(point, Min, Max)
                ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingRectangle contains another rectangle.
        /// </summary>
        public ContainmentType Contains(BoundingRectangle rectangle)
        {
            if (this.Min.X > rectangle.Max.X ||
                this.Min.Y > rectangle.Max.Y ||
                this.Max.X < rectangle.Min.X ||
                this.Max.Y < rectangle.Min.Y)
            {
                return ContainmentType.Disjoint;
            }
            else if (
                this.Min.X <= rectangle.Min.X &&
                this.Min.Y <= rectangle.Min.Y &&
                this.Max.X >= rectangle.Max.X &&
                this.Max.Y >= rectangle.Max.Y)
            {
                return ContainmentType.Contains;
            }
            else
            {
                return ContainmentType.Intersects;
            }
        }

        public bool Equals(BoundingRectangle other)
        {
            return Min == other.Min && Max == other.Max;
        }

        public override bool Equals(object obj)
        {
            if (obj is BoundingRectangle)
                return Equals((BoundingRectangle)obj);

            return false;
        }

        public static bool operator ==(BoundingRectangle value1, BoundingRectangle value2)
        {
            return ((value1.Min == value2.Min) && (value1.Max == value2.Max));
        }

        public static bool operator !=(BoundingRectangle value1, BoundingRectangle value2)
        {
            return !(value1.Min == value2.Min && value1.Max == value2.Max);
        }

        public override int GetHashCode()
        {
            return Min.GetHashCode() + Max.GetHashCode();
        }

        public override string ToString()
        {
            return Min.ToString() + " ~ " + Max.ToString();
        }
    }
}