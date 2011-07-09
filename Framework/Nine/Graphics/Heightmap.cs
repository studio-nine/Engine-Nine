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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Nine.Graphics
{
    /// <summary>
    /// The geometric representation of heightmap. 
    /// The up axis of the terrain is Vector.UnitZ.
    /// </summary>
    public class Heightmap : ISurface
    {
        #region Fields
        /// <summary>
        /// Gets the size of the terrain geometry in 3 axis.
        /// </summary>
        [ContentSerializer]
        public Vector3 Size { get; internal set; }

        /// <summary>
        /// Gets the size of the smallest square block that made up the terrain.
        /// </summary>
        [ContentSerializer]
        public float Step { get; internal set; }

        /// <summary>
        /// Gets the heights of all terrain points.
        /// </summary>
        [ContentSerializer]
        public float[] Heights { get; internal set; }

        /// <summary>
        /// Gets the normals of all terrain points.
        /// </summary>
        [ContentSerializer]
        public Vector3[] Normals { get; internal set; }

        /// <summary>
        /// Gets the tangents of all terrain points.
        /// </summary>
        [ContentSerializer]
        public Vector3[] Tangents { get; internal set; }

        /// <summary>
        /// Gets the number of the smallest square block in X axis, or heightmap texture U axis.
        /// </summary>
        [ContentSerializer]
        public int Width { get; internal set; }

        /// <summary>
        /// Gets the number of the smallest square block in Y axis, or heightmap texture V axis.
        /// </summary>
        [ContentSerializer]
        public int Height { get; internal set; }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        [ContentSerializer]
        public object Tag { get; set; }

        /// <summary>
        /// Gets the axis aligned bounding box of this terrain.
        /// </summary>
        [ContentSerializer]
        public BoundingBox BoundingBox { get; internal set; }
        
        /// <summary>
        /// Occured when the heightmap changed.
        /// </summary>
        public event EventHandler Invalidate;

        #endregion
        
        #region Methods

        internal Heightmap() { }


        /// <summary>
        /// Creates a new instance of Heightmap.
        /// </summary>
        /// <param name="step">Size of the smallest square block that made up the terrain.</param>
        /// <param name="segmentCountX">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="segmentCountY">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
        public Heightmap(float step, int segmentCountX, int segmentCountY)
            : this(new float[(segmentCountX + 1) * (segmentCountY + 1)], step, segmentCountX, segmentCountY)
        {
        }

        /// <summary>
        /// Creates a new instance of Heightmap.
        /// </summary>
        /// <param name="heightmap">Heights of each points. The dimension of the array should be (segmentCountX + 1) * (segmentCountY + 1).</param>
        /// <param name="step">Size of the smallest square block that made up the terrain.</param>
        /// <param name="segmentCountX">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="segmentCountY">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
        public Heightmap(float[] heightmap, float step, int segmentCountX, int segmentCountY)
        {
            if (step <= 0 || segmentCountX <= 0 || segmentCountY <= 0)
                throw new ArgumentOutOfRangeException();

            Step = step;
            Width = segmentCountX;
            Height = segmentCountY;

            LoadHeightmap(heightmap);
        }

        /// <summary>
        /// Gets the position of the terrain on given point.
        /// </summary>
        /// <param name="x">Point on x axis.</param>
        /// <param name="y">Point on y axis.</param>
        public Vector3 GetPosition(int x, int y)
        {
            Vector3 result = new Vector3();

            result.X = Step * x;
            result.Y = Step * y;
            result.Z = Heights[GetIndex(x, y)];

            return result;
        }

        /// <summary>
        /// Gets the index of the terrain on given point. 
        /// The return value can be used to index Heights, Normals and Tangents.
        /// </summary>
        /// <param name="x">Point on x axis.</param>
        /// <param name="y">Point on y axis.</param>
        public int GetIndex(int x, int y)
        {
            if (x < 0 || y < 0 || x > Width || y > Height)
                throw new ArgumentOutOfRangeException();

            return y * (Width + 1) + x;
        }

        /// <summary>
        /// Loads this terrain geometry with the specified heightmap data.
        /// </summary>
        /// <param name="heightmap">Heights of each points. The dimension of the array should be (segmentCountX + 1) * (segmentCountY + 1).</param>
        public void LoadHeightmap(float[] heightmap)
        {
            Heights = heightmap;

            // Find maximun height value
            float maxheight = float.MinValue;

            foreach (float height in heightmap)
                if (height > maxheight)
                    maxheight = height;
                        
            Size = new Vector3(Step * Width, Step * Height, maxheight);

            BoundingBox = BoundingBox.CreateFromPoints(EnumeratePositions());


            // Allocation space for normals and tangents
            int count = (Width + 1) * (Height + 1);

            Vector3[] normals = Normals;
            Vector3[] tangents = Tangents;

            if (Normals == null || Normals.Length < count)
                normals = new Vector3[count];

            if (Tangents == null || Tangents.Length < count)
                tangents = new Vector3[count];


            // Compute normals and tangents
            CalculateNormalsAndTangents(
                Width + 1, Height + 1, heightmap, 
                Size.X, Size.Y, ref normals, ref tangents);

            Normals = normals;
            Tangents = tangents;


            // Fire invalidate event
            if (Invalidate != null)
                Invalidate(this, EventArgs.Empty);
        }

        private IEnumerable<Vector3> EnumeratePositions()
        {
            for (int x = 0; x <= Width; x++)
            {
                for (int y = 0; y <= Height; y++)
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

            Vector3 result = new Vector3();

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

        #region ISurface Members
        /// <summary>
        /// Gets the height of the terrain at a given location.
        /// </summary>
        /// <returns>Null if the location is outside the boundary of the terrain.</returns>
        public float GetHeight(float x, float y)
        {
            float result;
            Vector3 position = new Vector3();
            Vector3 normal = new Vector3();

            position.X = x;
            position.Y = y;
            position.Z = 0;

            if (TryGetHeightAndNormal(position, out result, out normal))
                return result;

            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Gets the normal of the terrain at a given location.
        /// </summary>
        /// <returns>Null if the location is outside the boundary of the terrain.</returns>
        public Vector3 GetNormal(float x, float y)
        {
            float result;
            Vector3 position = new Vector3();
            Vector3 normal = new Vector3();

            position.X = x;
            position.Y = y;
            position.Z = 0;

            if (TryGetHeightAndNormal(position, out result, out normal))
                return normal;

            throw new ArgumentOutOfRangeException();
        }

        /// <summary>
        /// Gets the height and normal of the terrain at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the terrain.</returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            // first we'll figure out where on the heightmap "position" is...
            if (position.X == Size.X)
                position.X -= float.Epsilon;
            if (position.Y == Size.Y)
                position.Y -= float.Epsilon;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            if (!(position.X >= 0 && position.X < Size.X &&
                  position.Y >= 0 && position.Y < Size.Y))
            {
                height = float.MinValue;
                normal = Vector3.UnitZ;

                return false;
            }

            // we'll use integer division to figure out where in the "heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            int left = (int)Math.Floor(position.X * Width / Size.X);
            int top = (int)Math.Floor(position.Y * Height / Size.Y);

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            float xNormalized = position.X - left * Size.X / Width;
            float yNormalized = position.Y - top * Size.Y / Height;

            // Now that we've calculated the indices of the corners of our cell, and
            // where we are in that cell, we'll use bilinear interpolation to calculate
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

            // We'll repeat the same process to calculate the normal.
            Vector3 topNormal = Vector3.Lerp(
                Normals[GetIndex(left, top)],
                Normals[GetIndex(left + 1, top)], xNormalized);

            Vector3 bottomNormal = Vector3.Lerp(
                Normals[GetIndex(left, top + 1)],
                Normals[GetIndex(left + 1, top + 1)], xNormalized);

            normal = Vector3.Lerp(topNormal, bottomNormal, yNormalized);
            normal.Normalize();

            return true;
        }

        #endregion
    }
}
