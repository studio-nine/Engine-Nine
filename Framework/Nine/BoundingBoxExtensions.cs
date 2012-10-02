namespace Nine
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;


    /// <summary>
    /// Contains extension methods for BoundingBox.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BoundingBoxExtensions
    {
        /// <summary>
        /// Tests whether the BoundingBox intersects with a line segment.
        /// </summary>
        public static float? Intersects(this BoundingBox box, Vector3 v1, Vector3 v2)
        {
            float? distance;
            Intersects(box, ref v1, ref v2, out distance);
            return distance;
        }

        /// <summary>
        /// Tests whether the BoundingBox intersects with a line segment.
        /// </summary>
        public static void Intersects(this BoundingBox box, ref Vector3 v1, ref Vector3 v2, out float? result)
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

            Ray ray = new Ray(v1, dir);
            box.Intersects(ref ray, out result);
            if (result.HasValue && result.Value > length)
                result = null;
        }

        /// <summary>
        /// Clips a box against a frustum and split the input triangle when they interests.
        /// </summary>
        public static void Intersects(this BoundingBox box, BoundingFrustum frustum, out Vector3[] intersections, out int length)
        {
            if (Intersections == null)
                Intersections = new Vector3[32 * 12];
            intersections = Intersections;
            length = Intersects(box, frustum, intersections, 0);
        }
        static Vector3[] Intersections;

        /// <summary>
        /// Clips a box against a frustum and split the input triangle when they interests.
        /// </summary>
        public static int Intersects(this BoundingBox box, BoundingFrustum frustum, Vector3[] intersections, int startIndex)
        {
            var count = 0;
            frustum.GetCorners(FrustumCorners);            
            for (int i = 0; i < TriangleIndices.Length; i += 3)
            {
                count += Triangle.Intersects(ref FrustumCorners[TriangleIndices[i]],
                                             ref FrustumCorners[TriangleIndices[i + 1]],
                                             ref FrustumCorners[TriangleIndices[i + 2]],
                                             ref box, intersections, startIndex + count);
            }
            return startIndex + count;
        }

        static Vector3[] FrustumCorners = new Vector3[BoundingFrustum.CornerCount];

        /// <summary>
        /// These are the indices used to tessellate a box/frustum into a list of triangles. 
        /// </summary>
        internal static readonly ushort[] TriangleIndices = new ushort[]
        {
            0,1,2,  3,0,2,
            4,6,5,  4,7,6,
            0,3,4,  4,3,7,
            5,1,0,  5,0,4,
            5,6,2,  5,2,1,
            3,2,6,  3,6,7,
        };

        /// <summary>
        /// Tests whether the BoundingBox contains the specified geometry.
        /// </summary>
        /// <param name="transform">
        /// Transform of the geometry.
        /// </param>
        public static ContainmentType Contains(this BoundingBox box, IGeometry geometry)
        {
            if (geometry == null)
                throw new ArgumentNullException("geometry");

            Triangle triangle;
            ContainmentType containment;

            Vector3[] positions;
            ushort[] indices;
            bool? containsLastTriangle = null;
            Matrix matrix = geometry.Transform;
            geometry.TryGetTriangles(out positions, out indices);

            for (int i = 0; i < indices.Length; i += 3)
            {
                if (geometry.Transform == null)
                {
                    triangle.V1 = positions[indices[i]];
                    triangle.V2 = positions[indices[i + 1]];
                    triangle.V3 = positions[indices[i + 2]];
                }
                else
                {
                    Vector3.Transform(ref positions[indices[i]], ref matrix, out triangle.V1);
                    Vector3.Transform(ref positions[indices[i + 1]], ref matrix, out triangle.V2);
                    Vector3.Transform(ref positions[indices[i + 2]], ref matrix, out triangle.V3);
                }

                box.Contains(ref triangle, out containment);
                
                if (containment == ContainmentType.Intersects)
                    return ContainmentType.Intersects;

                if (containment == ContainmentType.Contains)
                {
                    if (containsLastTriangle.HasValue && !containsLastTriangle.Value)
                        return ContainmentType.Intersects;
                    containsLastTriangle = true;
                }
                else
                {
                    if (containsLastTriangle.HasValue && containsLastTriangle.Value)
                        return ContainmentType.Intersects;
                    containsLastTriangle = false;
                }
            }
            return (containsLastTriangle.HasValue && containsLastTriangle.Value) ? ContainmentType.Contains : ContainmentType.Disjoint;
        }

        /// <summary>
        /// Tests whether the BoundingBox contains a Triangle.
        /// </summary>
        public static ContainmentType Contains(this BoundingBox box, Triangle triangle)
        {
            ContainmentType containmentType;
            Contains(box, ref triangle, out containmentType);
            return containmentType;
        }

        /// <summary>
        /// Tests whether the BoundingBox contains a Triangle.
        /// </summary>
        public static void Contains(this BoundingBox box, ref Triangle triangle, out ContainmentType containmentType)
        {
            bool c1 = box.Contains(triangle.V1) == ContainmentType.Contains;
            bool c2 = box.Contains(triangle.V2) == ContainmentType.Contains;
            bool c3 = box.Contains(triangle.V3) == ContainmentType.Contains;

            if (c1 && c2 && c3)
            {
                containmentType = ContainmentType.Contains;
                return;
            }
            if (c1 || c2 || c3)
            {
                containmentType = ContainmentType.Intersects;
                return;
            }

            box.GetCorners(corners);

            for (int i = 0; i < indices.Length; i += 2)
            {
                if (triangle.Intersects(corners[indices[i]], corners[indices[i + 1]]).HasValue)
                {
                    containmentType = ContainmentType.Intersects;
                    return;
                }
            }

            float? distance;
            Intersects(box, ref triangle.V1, ref triangle.V2, out distance);
            if (distance.HasValue)
            {
                containmentType = ContainmentType.Intersects;
                return;
            }
            Intersects(box, ref triangle.V1, ref triangle.V3, out distance);
            if (distance.HasValue)
            {
                containmentType = ContainmentType.Intersects;
                return;
            }
            Intersects(box, ref triangle.V2, ref triangle.V3, out distance);
            if (distance.HasValue)
            {
                containmentType = ContainmentType.Intersects;
                return;
            }

            containmentType = ContainmentType.Disjoint;
        }

        static Vector3[] corners = new Vector3[BoundingBox.CornerCount];
        static int[] indices = new int[] 
        {
            0, 1, 1, 2, 2, 3, 3, 0,  
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7,
        };

        /// <summary>
        /// Gets the center of this BoundingBox.
        /// </summary>
        public static Vector3 GetCenter(this BoundingBox box)
        {
            Vector3 result = new Vector3();

            result.X = (box.Min.X + box.Max.X) / 2;
            result.Y = (box.Min.Y + box.Max.Y) / 2;
            result.Z = (box.Min.Z + box.Max.Z) / 2;

            return result;
        }

        /// <summary>
        /// Gets the center of this Rectangle.
        /// </summary>
        public static Point GetCenter(this Rectangle rectangle)
        {
            Point result = new Point();

            result.X = (rectangle.X + rectangle.Width) / 2;
            result.Y = (rectangle.Y + rectangle.Height) / 2;

            return result;
        }

        /// <summary>
        /// Gets the center of this Rectangle.
        /// </summary>
        public static Vector2 GetCenter(this BoundingRectangle rectangle)
        {
            Vector2 result = new Vector2();

            result.X = (rectangle.Min.X + rectangle.Max.X) / 2;
            result.Y = (rectangle.Min.Y + rectangle.Max.Y) / 2;

            return result;
        }

        /// <summary>
        /// Creates a merged bounding box from a list of existing bounding boxes.
        /// </summary>
        public static BoundingBox CreateMerged(IEnumerable<BoundingBox> boxes)
        {
            BoundingBox result = new BoundingBox();

            int iBox = 0;
            foreach (var box in boxes)
            {
                if (iBox == 0)
                {
                    result = box;
                }
                else
                {
                    result = BoundingBox.CreateMerged(result, box);
                }
                iBox++;
            }

            return result;
        }
        
        /// <summary>
        /// Compute the axis aligned bounding box from an oriented bounding box.
        /// </summary>
        public static BoundingBox CreateAxisAligned(this BoundingBox box, Matrix transform)
        {
            BoundingBox result;
            CreateAxisAligned(box, ref transform, out result);
            return result;
        }

        /// <summary>
        /// Compute the axis aligned bounding box from an oriented bounding box.
        /// </summary>
        public static void CreateAxisAligned(this BoundingBox box, ref Matrix transform, out BoundingBox result)
        {
            // Find the 8 corners
            box.GetCorners(corners);

            // Compute bounding box
            Vector3.Transform(ref corners[0], ref transform, out result.Max);
            result.Min = result.Max;

            var v = new Vector3();
            for (int i = 1; i < corners.Length; ++i)
            {
                Vector3.Transform(ref corners[i], ref transform, out v);

                if (v.X < result.Min.X)
                    result.Min.X = v.X;
                else if (v.X > result.Max.X)
                    result.Max.X = v.X;

                if (v.Y < result.Min.Y)
                    result.Min.Y = v.Y;
                else if (v.Y > result.Max.Y)
                    result.Max.Y = v.Y;

                if (v.Z < result.Min.Z)
                    result.Min.Z = v.Z;
                else if (v.Z > result.Max.Z)
                    result.Max.Z = v.Z;
            }
        }
    }
}