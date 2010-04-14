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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics.Landscape
{
    public class TerrainGeometry : ISurface, IPickable
    {
        #region Fields
        [ContentSerializer]
        public Vector3 Dimension { get; private set; }

        [ContentSerializer]
        public float Step { get; private set; }

        [ContentSerializer]
        public float[] Heights { get; private set; }

        [ContentSerializer]
        public Vector3[] Normals { get; private set; }

        [ContentSerializer]
        public Vector3[] Tangents { get; private set; }

        [ContentSerializer]
        public int TessellationU { get; private set; }

        [ContentSerializer]
        public int TessellationV { get; private set; }

        [ContentSerializer]
        public object Tag { get; set; }

        [ContentSerializer]
        public BoundingBox BoundingBox { get; private set; }


        public event EventHandler Invalidate;

        #endregion
        
        #region Methods

        private TerrainGeometry() { }

        public TerrainGeometry(float step, int tessellationU, int tessellationV)
        {
            LoadHeightmap(new float[(tessellationU + 1) * (tessellationV + 1)], step, tessellationU, tessellationV);
        }
        
        public TerrainGeometry(float[] heightmap, float step, int tessellationU, int tessellationV)
        {
            LoadHeightmap(heightmap, step, tessellationU, tessellationV);
        }


        public Vector3 GetPosition(int u, int v)
        {
            Vector3 result;

            result.X = Step * u - Dimension.X / 2;
            result.Y = Step * v - Dimension.Y / 2;
            result.Z = Heights[GetIndex(u, v)];

            return result;
        }

        public int GetIndex(int u, int v)
        {
            if (u < 0 || v < 0 || u > TessellationU || v > TessellationV)
                throw new ArgumentOutOfRangeException();

            return v * (TessellationU + 1) + u;
        }

        public void LoadHeightmap(float[] heightmap, float step, int tessellationU, int tessellationV)
        {
            if (step < 0 || tessellationU < 0 || tessellationV < 0)
                throw new ArgumentOutOfRangeException();


            Step = step;
            Heights = heightmap;
            TessellationU = tessellationU;
            TessellationV = tessellationV;


            // Find maximun height value
            float maxheight = float.MinValue;

            foreach (float height in heightmap)
                if (height > maxheight)
                    maxheight = height;
                        
            Dimension = new Vector3(step * tessellationU, step * tessellationV, maxheight);

            BoundingBox = BoundingBox.CreateFromPoints(EnumeratePositions());


            // Allocation space for normals and tangents
            int count = (tessellationU + 1) * (tessellationV + 1);

            Vector3[] normals = Normals;
            Vector3[] tangents = Tangents;

            if (Normals == null || Normals.Length < count)
                normals = new Vector3[count];

            if (Tangents == null || Tangents.Length < count)
                tangents = new Vector3[count];


            // Compute normals and tangents
            CalculateNormalsAndTangents(
                tessellationU + 1, tessellationV + 1, heightmap, 
                Dimension.X, Dimension.Y, ref normals, ref tangents);

            Normals = normals;
            Tangents = tangents;


            // Fire invalidate event
            if (Invalidate != null)
                Invalidate(this, EventArgs.Empty);
        }

        private IEnumerable<Vector3> EnumeratePositions()
        {
            for (int x = 0; x <= TessellationU; x++)
            {
                for (int y = 0; y <= TessellationV; y++)
                {
                    yield return GetPosition(x, y);
                }
            }
        }

        #region Terrain Normal & Tangent Data Generation
        private static Vector3 CalculatePosition(int x, int y, int w, int h, float[] heights, float sizeX, float sizeY)
        {
            // Make sure we stay on the valid map data
            int mapX = x < 0 ? 0 : x >= w ? w - 1 : x;
            int mapY = y < 0 ? 0 : y >= h ? h - 1 : y;

            Vector3 result;

            result.X = x * sizeX / (w - 1);
            result.Y = y * sizeY / (h - 1);
            result.Z = heights[mapX + mapY * w];

            return result;
        }

        private static Vector3[] normalsForSmoothing = null;

        /// <summary>
        /// Calculate normals from height data
        /// </summary>
        private static void CalculateNormalsAndTangents(int w, int h, float[] heights, float sizeX, float sizeY, ref Vector3[] normals, ref Vector3[] tangents)
        {
            #region Build tangent vertices
            // Build our tangent vertices
            for (int x = 0; x < w; x++)
                for (int y = 0; y < h; y++)
                {
                    // Step 1: Calculate position
                    Vector3 pos = CalculatePosition(x, y, w, h, heights, sizeX, sizeY);

                    // Step 2: Calculate all edge vectors (for normals and tangents)
                    // This involves quite complicated optimizations and mathematics,
                    // hard to explain with just a comment. Read my book :D
                    Vector3 edge1 = pos - CalculatePosition(x, y + 1, w, h, heights, sizeX, sizeY);
                    Vector3 edge2 = pos - CalculatePosition(x + 1, y, w, h, heights, sizeX, sizeY);
                    Vector3 edge3 = pos - CalculatePosition(x - 1, y + 1, w, h, heights, sizeX, sizeY);
                    Vector3 edge4 = pos - CalculatePosition(x + 1, y + 1, w, h, heights, sizeX, sizeY);
                    Vector3 edge5 = pos - CalculatePosition(x - 1, y - 1, w, h, heights, sizeX, sizeY);

                    // Step 3: Calculate normal based on the edges (interpolate
                    // from 3 cross products we build from our edges).
                    normals[x + y * w] = Vector3.Normalize(
                        Vector3.Cross(edge2, edge1) +
                        Vector3.Cross(edge4, edge3) +
                        Vector3.Cross(edge3, edge5));

                    // Step 4: Set tangent data, just use edge1
                    tangents[x + y * w] = Vector3.Normalize(edge1);
                }
            #endregion


            #region Smooth normals
            // Smooth all normals, first copy them over, then smooth everything
            if (normalsForSmoothing == null || normalsForSmoothing.Length < w * h)
                normalsForSmoothing = new Vector3[w * h];

            Array.Copy(normals, normalsForSmoothing, normals.Length);

            // Time to smooth to normals we just saved
            for (int x = 1; x < w - 1; x++)
            {
                for (int y = 1; y < h - 1; y++)
                {
                    // Smooth 3x3 normals, but still use old normal to 40% (5 of 13)
                    Vector3 normal = normals[x + y * w] * 4;
                    for (int xAdd = -1; xAdd <= 1; xAdd++)
                        for (int yAdd = -1; yAdd <= 1; yAdd++)
                            normal += normalsForSmoothing[x + xAdd + (y + yAdd) * w];
                    normals[x + y * w] = Vector3.Normalize(normal);

                    // Also recalculate tangent to let it stay 90 degrees on the normal
                    Vector3 helperVector = Vector3.Cross(normals[x + y * w], tangents[x + y * w]);
                    tangents[x + y * w] = Vector3.Cross(helperVector, normals[x + y * w]);
                }
            }
            #endregion
        }

        #endregion

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

        #region ISurface Members

        public bool TryGetHeightAndNormal(Vector3 position, ref float height, ref Vector3 normal)
        {
            // first we'll figure out where on the heightmap "position" is...
            position.X = position.X + Dimension.X / 2;
            position.Y = position.Y + Dimension.Y / 2;

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
            int left = (int)position.X / (int)(Dimension.X / TessellationU);
            int top = (int)position.Y / (int)(Dimension.Y / TessellationV);

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = position.X - left * Dimension.X / TessellationU;
            float yNormalized = position.Y - top * Dimension.Y / TessellationV;

            if (height != null)
            {
                // Now that we've calculated the indices of the corners of our cell, and
                // where we are in that cell, we'll use bilinear interpolation to calculuate
                // our height. This process is best explained with a diagram, so please see
                // the accompanying doc for more information.
                // First, calculate the heights on the bottom and top edge of our cell by
                // interpolating from the left and right sides.
                float topHeight = MathHelper.Lerp(
                    Heights[GetIndex(left, top)],
                    Heights[GetIndex(left + 1, top)], xNormalized);

                float bottomHeight = MathHelper.Lerp(
                    Heights[GetIndex(left, top + 1)],
                    Heights[GetIndex(left + 1, top + 1)], xNormalized);

                // next, interpolate between those two values to calculate the height at our
                // position.
                height = MathHelper.Lerp(topHeight, bottomHeight, yNormalized);
            }

            if (normal != null)
            {
                // We'll repeat the same process to calculate the normal.
                Vector3 topNormal = Vector3.Lerp(
                    Normals[GetIndex(left, top)],
                    Normals[GetIndex(left + 1, top)], xNormalized);

                Vector3 bottomNormal = Vector3.Lerp(
                    Normals[GetIndex(left, top + 1)],
                    Normals[GetIndex(left + 1, top + 1)], xNormalized);

                normal = Vector3.Lerp(topNormal, bottomNormal, yNormalized);
                normal.Normalize();
            }

            return true;
        }

        #endregion
    }
}
