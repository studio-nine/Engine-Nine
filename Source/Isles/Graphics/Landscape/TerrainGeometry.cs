#region File Description
//-----------------------------------------------------------------------------
// GeometricPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics.Landscape
{
    public class TerrainGeometry : IGeometry, ISurface, IPickable
    {
        // TODO: Divide terrain into patches (p2)
        //       Add patch LOD (p3)

        [ContentSerializer]
        public Vector3 Dimension { get; private set; }

        [ContentSerializerIgnore]
        public Vector3 Position { get; set; }

        [ContentSerializerIgnore]
        public Matrix Transform
        {
            get { return Matrix.CreateTranslation(Position); }
        }

        [ContentSerializer]
        public int HeightmapWidth { get; private set; }

        [ContentSerializer]
        public int HeightmapHeight { get; private set; }

        [ContentSerializer]
        public Vector3[] NormalData { get; private set; }

        [ContentSerializer]
        public Vector3[] TangentData { get; private set; }

        #region Methods


        public TerrainGeometry(Vector3[] positions, Vector3[] normalData, Vector3[] tangentData, int heightmapWidth, int heightmapHeight, Vector3 dimension)
        {
            if (positions == null || normalData == null)
                throw new ArgumentNullException();

            NormalData = normalData;
            TangentData = tangentData;

            HeightmapWidth = heightmapWidth;
            HeightmapHeight = heightmapHeight;

            this.positions = new List<Vector3>(positions);

            Dimension = dimension;
        }


        private TerrainGeometry()
        {
            Dimension = new Vector3(1, 1, 1);
        }

        #endregion

        #region IPickable Members

        public object Pick(Vector3 point)
        {
            return null;
        }

        public object Pick(Ray ray, out float? distance)
        {
            distance = null;
            return null;
        }

        #endregion

        #region IGeometry Members

        private List<ushort> indices = null;

        [ContentSerializer]
        private List<Vector3> positions = null;
        
        public IList<Vector3> Positions { get { return positions.AsReadOnly(); } }

        public IList<ushort> Indices
        {
            get 
            {
                if (indices == null)
                {
                    indices = new List<ushort>((HeightmapWidth - 1) * (HeightmapHeight - 1) * 6);
                    
                    for (int y = 0; y < HeightmapHeight - 1; y++)
                        for (int x = 0; x < HeightmapWidth - 1; x++)
                        {
                            indices.Add((ushort)(y * HeightmapWidth + x));
                            indices.Add((ushort)((y + 1) * HeightmapWidth + x + 1));
                            indices.Add((ushort)(y * HeightmapWidth + x + 1));

                            indices.Add((ushort)(y * HeightmapWidth + x));
                            indices.Add((ushort)((y + 1) * HeightmapWidth + x));
                            indices.Add((ushort)((y + 1) * HeightmapWidth + x + 1));
                        }
                }

                return indices.AsReadOnly(); 
            }
        }

        #endregion

        #region ISurface Members

        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            // first we'll figure out where on the heightmap "position" is...
            position.X = position.X + Dimension.X / 2 - Position.X;
            position.Y = position.Y + Dimension.Y / 2 - Position.Y;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            if (!(position.X > 0 && position.X < Dimension.X &&
                  position.Y > 0 && position.Y < Dimension.Y))
            {
                height = 0;
                normal = Vector3.Zero;

                return false;
            }

            // we'll use integer division to figure out where in the "heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            int left = (int)position.X / (int)(Dimension.X / (HeightmapWidth - 1));
            int top = (int)position.Y / (int)(Dimension.Y / (HeightmapHeight - 1));

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = position.X - left * Dimension.X / (HeightmapWidth - 1);
            float yNormalized = position.Y - top * Dimension.Y / (HeightmapHeight - 1);

            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculuate
            // our height. This process is best explained with a diagram, so please see
            // the accompanying doc for more information.
            // First, calculate the heights on the bottom and top edge of our cell by
            // interpolating from the left and right sides.
            float topHeight = MathHelper.Lerp(
                positions[left + top * HeightmapWidth].Z,
                positions[left + 1 + top * HeightmapWidth].Z, xNormalized);

            float bottomHeight = MathHelper.Lerp(
                positions[left + (top + 1) * HeightmapWidth].Z,
                positions[left + 1 + (top + 1) * HeightmapWidth].Z, xNormalized);

            // next, interpolate between those two values to calculate the height at our
            // position.
            height = MathHelper.Lerp(topHeight, bottomHeight, yNormalized) + Position.Z;

            // We'll repeat the same process to calculate the normal.
            Vector3 topNormal = Vector3.Lerp(
                NormalData[left + top * HeightmapWidth],
                NormalData[left + 1 + top * HeightmapWidth], xNormalized);

            Vector3 bottomNormal = Vector3.Lerp(
                NormalData[left + (top + 1) * HeightmapWidth],
                NormalData[left + 1 + (top + 1) * HeightmapWidth], xNormalized);

            normal = Vector3.Lerp(topNormal, bottomNormal, yNormalized);
            normal.Normalize();

            return true;
        }

        #endregion
    }
}
