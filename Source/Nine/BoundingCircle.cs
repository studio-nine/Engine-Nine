namespace Nine
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;


    /// <summary>
    /// Defines a circle-shaped 2D volume.
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
        /// Tests whether the BoundingCircle contains a point.
        /// </summary>
        public ContainmentType Contains(float x, float y)
        {
            var xx = (x - Center.X);
            var yy = (y - Center.Y);

            return xx * xx + yy * yy < Radius * Radius ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingCircle contains a point.
        /// </summary>
        public ContainmentType Contains(Vector2 point)
        {
            var xx = (point.X - Center.X);
            var yy = (point.Y - Center.Y);

            return xx * xx + yy * yy < Radius * Radius ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingCircle contains another BoundingCircle.
        /// </summary>
        public void Contains(ref BoundingCircle circle, out ContainmentType containmentType)
        {
            var xx = (circle.Center.X - Center.X);
            var yy = (circle.Center.Y - Center.Y);
            var rr = circle.Radius + Radius;
            var dr = circle.Radius - Radius;
            var distanceSq = xx * xx + yy * yy;

            if (distanceSq >= rr * rr)
            {
                containmentType = ContainmentType.Disjoint;
                return;
            }
            if (Radius > circle.Radius && distanceSq < dr * dr)
            {
                containmentType = ContainmentType.Contains;
                return;
            }
            containmentType = ContainmentType.Intersects;
        }

        /// <summary>
        /// Tests whether the BoundingCircle contains another BoundingCircle.
        /// </summary>
        public ContainmentType Contains(BoundingCircle circle)
        {
            var xx = (circle.Center.X - Center.X);
            var yy = (circle.Center.Y - Center.Y);
            var rr = circle.Radius + Radius;
            var dr = circle.Radius - Radius;
            var distanceSq = xx * xx + yy * yy;

            if (distanceSq >= rr * rr)
                return ContainmentType.Disjoint;
            if (Radius > circle.Radius && distanceSq < dr * dr)
                return ContainmentType.Contains;
            return ContainmentType.Intersects;
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
            var sphere = BoundingSphere.CreateFromPoints(points.Select(pt => new Vector3(pt, 0)));
            var result = new BoundingCircle();
            result.Center.X = sphere.Center.X;
            result.Center.Y = sphere.Center.Y;
            result.Radius = sphere.Radius;
            return result;
        }
    }
}