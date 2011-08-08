#region Copyright 2008 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2008 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
#endregion

namespace Nine
{
    /// <summary>
    /// Contains extension methods for BoundingBox.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class BoundingBoxExtensions
    {
        static Vector3[] corners = new Vector3[BoundingBox.CornerCount];
        static int[] indices = new int[] 
        {
            0, 1, 1, 2, 2, 3, 3, 0,  
            4, 5, 5, 6, 6, 7, 7, 4,
            0, 4, 1, 5, 2, 6, 3, 7,
        };

        /// <summary>
        /// Tests whether the BoundingBox contains a Triangle.
        /// </summary>
        public static ContainmentType Contains(this BoundingBox box, Triangle triangle)
        {
            bool c1 = box.Contains(triangle.V1) == ContainmentType.Contains;
            bool c2 = box.Contains(triangle.V2) == ContainmentType.Contains;
            bool c3 = box.Contains(triangle.V3) == ContainmentType.Contains;
            
            if (c1 && c2 && c3)
                return ContainmentType.Contains;

            if (c1 || c2 || c3)
                return ContainmentType.Intersects;

            box.GetCorners(corners);

            for (int i = 0; i < indices.Length; i += 2)
            {
                if (triangle.Intersects(corners[indices[i]], corners[indices[i + 1]]))
                    return ContainmentType.Intersects;
            }

            return ContainmentType.Disjoint;
        }

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
            const float FloatMax = float.MaxValue;

            // Find the 8 corners
            Vector3[] corners = box.GetCorners();

            // Compute bounding box
            Vector3 min = new Vector3(FloatMax, FloatMax, FloatMax);
            Vector3 max = new Vector3(-FloatMax, -FloatMax, -FloatMax);
            Vector3 v = new Vector3();

            for (int i = 0; i < corners.Length; i++)
            {
                Vector3.Transform(ref corners[i], ref transform, out v);

                if (v.X < min.X)
                    min.X = v.X;
                if (v.X > max.X)
                    max.X = v.X;

                if (v.Y < min.Y)
                    min.Y = v.Y;
                if (v.Y > max.Y)
                    max.Y = v.Y;

                if (v.Z < min.Z)
                    min.Z = v.Z;
                if (v.Z > max.Z)
                    max.Z = v.Z;
            }

            return new BoundingBox(min, max);
        }

        public static BoundingBox Max = new BoundingBox(Vector3.One * float.MinValue, Vector3.One * float.MaxValue);
    }
}