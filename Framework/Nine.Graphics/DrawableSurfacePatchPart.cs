#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// A surface patch part is 8 triangles that makes up a square block.
    /// </summary>
    internal class DrawableSurfacePatchPart
    {
        //  ____ ____
        // |0 / | \ 3|
        // |_/_1|2_\_|
        // | \ 5|6 / |
        // |4_\_|_/_7|
        //

        /// <summary>
        /// Gets the mask that no triangles are visible.
        /// </summary>
        public const byte Empty = 0x00;

        /// <summary>
        /// Gets the mask that all triangles are visible.
        /// </summary>
        public const byte Full = 0xFF;

        /// <summary>
        /// Whether each triangle is visible.
        /// </summary>
        public byte Mask;

        /// <summary>
        /// Gets the axis aligned bounding box of this surface patch part.
        /// </summary>
        public BoundingBox BoundingBox;

        /// <summary>
        /// Constructor is for internal use only.
        /// </summary>
        internal DrawableSurfacePatchPart()
        {
            for (byte i = 0; i < 8; i++)
            {
                Triangles[i] = new DrawableSurfaceTriangle();
                Triangles[i].Part = this;
                Triangles[i].Index = i;
            }
        }

        internal int X, Y;
        internal DrawableSurfacePatch Patch;
        internal DrawableSurfaceTriangle[] Triangles = new DrawableSurfaceTriangle[8];

        #region Indices
        /// <summary>
        /// Gets the indices of points that made up of this surface patch part.
        /// See Surface.GetPosition().
        /// </summary>
        public IEnumerable<Point> GetIndices()
        {
            if (((Mask >> 0) & 0x01) == 1)
            {
                yield return points[0];
                yield return points[1];
                yield return points[3];
            }
            if (((Mask >> 1) & 0x01) == 1)
            {
                yield return points[3];
                yield return points[1];
                yield return points[4];
            }
            if (((Mask >> 2) & 0x01) == 1)
            {
                yield return points[1];
                yield return points[5];
                yield return points[4];
            }
            if (((Mask >> 3) & 0x01) == 1)
            {
                yield return points[1];
                yield return points[2];
                yield return points[5];
            }
            if (((Mask >> 4) & 0x01) == 1)
            {
                yield return points[3];
                yield return points[7];
                yield return points[6];
            }
            if (((Mask >> 5) & 0x01) == 1)
            {
                yield return points[3];
                yield return points[4];
                yield return points[7];
            }
            if (((Mask >> 6) & 0x01) == 1)
            {
                yield return points[4];
                yield return points[5];
                yield return points[7];
            }
            if (((Mask >> 7) & 0x01) == 1)
            {
                yield return points[5];
                yield return points[8];
                yield return points[7];
            }
        }

        public IEnumerable<Point> GetIndicesForTriangle(int index)
        {
            if (index == 0)
            {
                yield return points[0];
                yield return points[1];
                yield return points[3];
            }
            else if (index == 1)
            {
                yield return points[3];
                yield return points[1];
                yield return points[4];
            }
            else if (index == 2)
            {
                yield return points[1];
                yield return points[5];
                yield return points[4];
            }
            else if (index == 3)
            {
                yield return points[1];
                yield return points[2];
                yield return points[5];
            }
            else if (index == 4)
            {
                yield return points[3];
                yield return points[7];
                yield return points[6];
            }
            else if (index == 5)
            {
                yield return points[3];
                yield return points[4];
                yield return points[7];
            }
            else if (index == 6)
            {
                yield return points[4];
                yield return points[5];
                yield return points[7];
            }
            else if (index == 7)
            {
                yield return points[5];
                yield return points[8];
                yield return points[7];
            }
        }

        private static Point[] points = new Point[]
        {
            new Point(0, 0), new Point(1, 0), new Point(2, 0),
            new Point(0, 1), new Point(1, 1), new Point(2, 1),
            new Point(0, 2), new Point(1, 2), new Point(2, 2),
        };
        #endregion
    }
}
