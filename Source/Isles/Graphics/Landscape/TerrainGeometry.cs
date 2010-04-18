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
    /// <summary>
    /// The geometric representation of heightmap. 
    /// The up axis of the terrain is Vector.UnitZ.
    /// </summary>
    public class TerrainGeometry : ISurface, IPickable
    {
        #region Fields
        /// <summary>
        /// Gets the size of the terrain geometry in 3 axis.
        /// </summary>
        [ContentSerializer]
        public Vector3 Dimension { get; private set; }

        /// <summary>
        /// Gets the size of the smallest square block that made up the terrain.
        /// </summary>
        [ContentSerializer]
        public float Step { get; private set; }

        /// <summary>
        /// Gets the heights of all terrain points.
        /// </summary>
        [ContentSerializer]
        public float[] Heights { get; private set; }

        /// <summary>
        /// Gets the normals of all terrain points.
        /// </summary>
        [ContentSerializer]
        public Vector3[] Normals { get; private set; }

        /// <summary>
        /// Gets the tangents of all terrain points.
        /// </summary>
        [ContentSerializer]
        public Vector3[] Tangents { get; private set; }

        /// <summary>
        /// Gets the number of the smallest square block in X axis, or heightmap texture U axis.
        /// </summary>
        [ContentSerializer]
        public int TessellationU { get; private set; }

        /// <summary>
        /// Gets the number of the smallest square block in Y axis, or heightmap texture V axis.
        /// </summary>
        [ContentSerializer]
        public int TessellationV { get; private set; }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        [ContentSerializer]
        public object Tag { get; set; }

        /// <summary>
        /// Gets the axis aligned bounding box of this terrain.
        /// </summary>
        [ContentSerializer]
        public BoundingBox BoundingBox { get; private set; }
        
        /// <summary>
        /// Fired when the heightmap changed.
        /// </summary>
        public event EventHandler Invalidate;

        #endregion
        
        #region Methods

        private TerrainGeometry() { }


        /// <summary>
        /// Creates a new instance of TerrainGeometry.
        /// </summary>
        /// <param name="step">Size of the smallest square block that made up the terrain.</param>
        /// <param name="tessellationU">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="tessellationV">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
        public TerrainGeometry(float step, int tessellationU, int tessellationV)
        {
            LoadHeightmap(new float[(tessellationU + 1) * (tessellationV + 1)], step, tessellationU, tessellationV);
        }

        /// <summary>
        /// Creates a new instance of TerrainGeometry.
        /// </summary>
        /// <param name="heightmap">Heights of each points. The dimension of the array should be (tessellationU + 1) * (tessellationV + 1).</param>
        /// <param name="step">Size of the smallest square block that made up the terrain.</param>
        /// <param name="tessellationU">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="tessellationV">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
        public TerrainGeometry(float[] heightmap, float step, int tessellationU, int tessellationV)
        {
            LoadHeightmap(heightmap, step, tessellationU, tessellationV);
        }

        /// <summary>
        /// Gets the position of the terrain on given point.
        /// </summary>
        /// <param name="u">Point on x axis.</param>
        /// <param name="v">Point on y axis.</param>
        public Vector3 GetPosition(int u, int v)
        {
            Vector3 result;

            result.X = Step * u - Dimension.X / 2;
            result.Y = Step * v - Dimension.Y / 2;
            result.Z = Heights[GetIndex(u, v)];

            return result;
        }

        /// <summary>
        /// Gets the index of the terrain on given point. 
        /// The return value can be used to index Heights, Normals and Tangents.
        /// </summary>
        /// <param name="u">Point on x axis.</param>
        /// <param name="v">Point on y axis.</param>
        public int GetIndex(int u, int v)
        {
            if (u < 0 || v < 0 || u > TessellationU || v > TessellationV)
                throw new ArgumentOutOfRangeException();

            return v * (TessellationU + 1) + u;
        }

        /// <summary>
        /// Loads this terrain geometry with the specified heightmap data.
        /// </summary>
        /// <param name="heightmap">Heights of each points. The dimension of the array should be (tessellationU + 1) * (tessellationV + 1).</param>
        /// <param name="step">Size of the smallest square block that made up the terrain.</param>
        /// <param name="tessellationU">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="tessellationV">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
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
        /// <summary>
        /// Points under the heightmap and are within the boundary are picked.
        /// </summary>
        /// <returns>This TerrainGeometry instance</returns>
        public object Pick(Vector3 point)
        {
            float? height = GetHeight(point.X, point.Y);

            if (height.HasValue && height.Value >= point.Z)
                return this;

            return null;
        }

        /// <summary>
        /// Checks whether a ray intersects the terrain mesh.
        /// </summary>
        /// <returns>This TerrainGeometry instance</returns>
        public object Pick(Ray ray, out float? distance)
        {
            // This method doesn't work correctly for now
            throw new NotImplementedException();

            distance = null;

            // Get two vertices to draw a line through the
            // heightfield.
            //
            // 1. Project the ray to XY plane
            // 2. Compute the 2 intersections of the ray and
            //    terrain bounding box (Projected)
            // 3. Find the 2 points to draw
            int i = 0;
            Vector3[] points = new Vector3[2];

            // Line equation: y = k * (x - x0) + y0
            float k = ray.Direction.Y / ray.Direction.X;
            float invK = ray.Direction.X / ray.Direction.Y;
            float r = ray.Position.Y - ray.Position.X * k;
            if (r >= 0 && r <= Dimension.Y)
            {
                points[i++] = new Vector3(0, r,
                    ray.Position.Z - ray.Position.X *
                    ray.Direction.Z / ray.Direction.X);
            }
            r = ray.Position.Y + (Dimension.X - ray.Position.X) * k;
            if (r >= 0 && r <= Dimension.Y)
            {
                points[i++] = new Vector3(Dimension.X, r,
                    ray.Position.Z + (Dimension.X - ray.Position.X) *
                    ray.Direction.Z / ray.Direction.X);
            }
            if (i < 2)
            {
                r = ray.Position.X - ray.Position.Y * invK;
                if (r >= 0 && r <= Dimension.X)
                    points[i++] = new Vector3(r, 0,
                        ray.Position.Z - ray.Position.Y *
                        ray.Direction.Z / ray.Direction.Y);
            }
            if (i < 2)
            {
                r = ray.Position.X + (Dimension.Y - ray.Position.Y) * invK;
                if (r >= 0 && r <= Dimension.X)
                    points[i++] = new Vector3(r, Dimension.Y,
                        ray.Position.Z + (Dimension.Y - ray.Position.Y) *
                        ray.Direction.Z / ray.Direction.Y);
            }
            if (i < 2)
                return null;

            // When ray position is inside the box, it should be one
            // of the starting point
            bool inside = ray.Position.X > 0 && ray.Position.X < Dimension.X &&
                          ray.Position.Y > 0 && ray.Position.Y < Dimension.Y;

            Vector3 v1 = Vector3.Zero, v2 = Vector3.Zero;
            // Sort the 2 points to make the line follow the direction
            if (ray.Direction.X > 0)
            {
                if (points[0].X < points[1].X)
                {
                    v2 = points[1];
                    v1 = inside ? ray.Position : points[0];
                }
                else
                {
                    v2 = points[0];
                    v1 = inside ? ray.Position : points[1];
                }
            }
            else if (ray.Direction.X < 0)
            {
                if (points[0].X > points[1].X)
                {
                    v2 = points[1];
                    v1 = inside ? ray.Position : points[0];
                }
                else
                {
                    v2 = points[0];
                    v1 = inside ? ray.Position : points[1];
                }
            }

            // Trace steps along your line and determine the Dimension.Z at each point,
            // for each sample point look up the Dimension.Z of the terrain and determine
            // if the point on the line is above or below the terrain. Once you have
            // determined the two sampling points that are above and below the terrain
            // you can refine using binary searching.
            float SamplePrecision = Step / 2;
            const int RefineSteps = 4;

            float length = Vector3.Subtract(v2, v1).Length();
            float current = 0;

            Vector3[] point = new Vector3[2];
            Vector3 step = ray.Direction * SamplePrecision;
            point[0] = v1;

            while (current < length)
            {
                if (GetHeight(point[0].X, point[0].Y) >= point[0].Z)
                    break;

                point[0] += step;
                current += SamplePrecision;
            }

            if (current > 0 && current < length)
            {
                // Perform binary search

                Vector3 p = point[0];
                point[1] = point[0] - step;

                for (i = 0; i < RefineSteps; i++)
                {
                    p = (point[0] + point[1]) * 0.5f;

                    if (GetHeight(p.X, p.Y) >= p.Z)
                        point[0] = p;
                    else
                        point[1] = p;
                }

                distance = Vector3.Subtract(ray.Position, p).Length();
                return this;
            }

            return null;
        }
        #endregion

        #region ISurface Members
        /// <summary>
        /// Gets the height of the terrain at a given location.
        /// </summary>
        /// <returns>Null if the location is outside the boundary of the terrain.</returns>
        public float? GetHeight(float x, float y)
        {
            float result;
            Vector3 position;
            Vector3 normal;

            position.X = x;
            position.Y = y;
            position.Z = 0;

            if (TryGetHeightAndNormal(position, out result, out normal))
                return result;

            return null;
        }

        /// <summary>
        /// Gets the height and normal of the terrain at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the terrain.</returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
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
