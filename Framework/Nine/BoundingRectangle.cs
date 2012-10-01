namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

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
        /// Tests whether the BoundingRectangle contains a point.
        /// </summary>
        public ContainmentType Contains(float x, float y)
        {
            return (x >= Min.X && x <= Max.X && y >= Min.Y && y <= Max.Y) ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingRectangle contains a point.
        /// </summary>
        public ContainmentType Contains(Vector2 point)
        {
            return (point.X >= Min.X && point.X <= Max.X && point.Y >= Min.Y && point.Y <= Max.Y) ? ContainmentType.Contains : ContainmentType.Disjoint;
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

        /// <summary>
        /// Tests whether the BoundingRectangle contains another rectangle.
        /// </summary>
        public void Contains(ref BoundingRectangle rectangle, out ContainmentType containmentType)
        {
            if (this.Min.X > rectangle.Max.X ||
                this.Min.Y > rectangle.Max.Y ||
                this.Max.X < rectangle.Min.X ||
                this.Max.Y < rectangle.Min.Y)
            {
                containmentType = ContainmentType.Disjoint;
                return;
            }
            
            if (this.Min.X <= rectangle.Min.X &&
                this.Min.Y <= rectangle.Min.Y &&
                this.Max.X >= rectangle.Max.X &&
                this.Max.Y >= rectangle.Max.Y)
            {
                containmentType = ContainmentType.Contains;
                return;
            }
            containmentType = ContainmentType.Intersects;
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
            return Min.GetHashCode() ^ Max.GetHashCode();
        }

        public override string ToString()
        {
            return Min.ToString() + " ~ " + Max.ToString();
        }

        /// <summary>
        /// Creates the smallest BoundingBox that will contain a group of points.
        /// </summary>
        public static BoundingRectangle CreateFromPoints(IEnumerable<Vector2> points)
        {
            BoundingRectangle rect = new BoundingRectangle();
            rect.Min = Vector2.One * float.MaxValue;
            rect.Max = Vector2.One * float.MinValue;
            
            foreach (Vector2 pt in points)
            {
                if (pt.X < rect.Min.X)
                    rect.Min.X = pt.X;
                if (pt.X > rect.Max.X)
                    rect.Max.X = pt.X;
                if (pt.Y < rect.Min.Y)
                    rect.Min.Y = pt.Y;
                if (pt.Y > rect.Max.Y)
                    rect.Max.Y = pt.Y;
            }
            return rect;
        }

        /// <summary>
        /// Creates the merged bounding rectangle.
        /// </summary>        
        public static void CreateMerged(ref BoundingRectangle original, ref BoundingRectangle additional, out BoundingRectangle result)
        {
            result = new BoundingRectangle();
            result.Min.X = (original.Min.X > additional.Min.X) ? additional.Min.X : original.Min.X;
            result.Min.Y = (original.Min.Y > additional.Min.Y) ? additional.Min.Y : original.Min.Y;
            result.Max.X = (original.Max.X < additional.Max.X) ? additional.Max.X : original.Max.X;
            result.Max.Y = (original.Max.Y < additional.Max.Y) ? additional.Max.Y : original.Max.Y;
        }

        /// <summary>
        /// Creates the merged bounding rectangle.
        /// </summary>
        public static BoundingRectangle CreateMerged(BoundingRectangle original, BoundingRectangle additional)
        {
            if (original.Min.X > additional.Min.X)
                original.Min.X = additional.Min.X;
            if (original.Min.Y > additional.Min.Y)
                original.Min.Y = additional.Min.Y;
            if (original.Max.X < additional.Max.X)
                original.Max.X = additional.Max.X;
            if (original.Max.Y < additional.Max.Y)
                original.Max.Y = additional.Max.Y;
            
            return original;
        }
    }
}