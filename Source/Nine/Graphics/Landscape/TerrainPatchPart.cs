#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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


namespace Nine.Graphics.Landscape
{
    /// <summary>
    /// A terrain patch part is 8 triangles that makes up a square block.
    /// </summary>
    /// <remarks>
    ///  ____ ____
    /// |0 / | \ 3|
    /// |_/_1|2_\_|
    /// | \ 5|6 / |
    /// |4_\_|_/_7|
    /// </remarks>
    public sealed class TerrainPatchPart
    {
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
        public byte Mask { get; set; }

        /// <summary>
        /// Center position of this patch part.
        /// </summary>
        public Vector3 Position { get; internal set; }

        /// <summary>
        /// Gets the axis aligned bounding box of this terrain patch part.
        /// </summary>
        public BoundingBox BoundingBox { get; internal set; }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Constructor is for internal use only.
        /// </summary>
        internal TerrainPatchPart() { }
        
        internal int X, Y;

        #region Indices
        /// <summary>
        /// Gets the indices of points that made up of this terrain patch part.
        /// See Terrain.GetPosition().
        /// </summary>
        public IEnumerable<Point> Indices
        {
            get 
            {
                if ((Mask > 0) & 0x01 == 1)
                {
                    yield return points[0];
                    yield return points[1];
                    yield return points[3];
                }
                if ((Mask > 1) & 0x01 == 1)
                {
                    yield return points[3];
                    yield return points[1];
                    yield return points[4];
                }
                if ((Mask > 2) & 0x01 == 1)
                {
                    yield return points[1];
                    yield return points[5];
                    yield return points[4];
                }
                if ((Mask > 3) & 0x01 == 1)
                {
                    yield return points[1];
                    yield return points[2];
                    yield return points[5];
                }
                if ((Mask > 4) & 0x01 == 1)
                {
                    yield return points[3];
                    yield return points[7];
                    yield return points[6];
                }
                if ((Mask > 5) & 0x01 == 1)
                {
                    yield return points[3];
                    yield return points[4];
                    yield return points[7];
                }
                if ((Mask > 6) & 0x01 == 1)
                {
                    yield return points[4];
                    yield return points[5];
                    yield return points[7];
                }
                if ((Mask > 7) & 0x01 == 1)
                {
                    yield return points[5];
                    yield return points[8];
                    yield return points[7];
                }
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
