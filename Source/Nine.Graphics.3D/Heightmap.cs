namespace Nine.Graphics
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Nine.AttachedProperty;
    using Nine.Serialization;
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;

    /// <summary>
    /// Represents a height field in 3d space.
    /// </summary>
    public interface IHeightmap
    {
        /// <summary>
        /// Gets the number of the smallest square block in Y axis, or heightmap texture V axis.
        /// </summary>
        int Width { get; }

        /// <summary>
        /// Gets the number of the smallest square block in Z axis, or heightmap texture U axis.
        /// </summary>
        int Height { get; }

        /// <summary>
        /// Gets the size of the smallest square block that made up the terrain.
        /// </summary>
        float Step { get; }

        /// <summary>
        /// Gets the height of the terrain on given point.
        /// </summary>
        float GetHeight(int x, int z);

        /// <summary>
        /// Gets the normal of the terrain on given point.
        /// </summary>
        Vector3 GetNormal(int x, int z);

        /// <summary>
        /// Gets the tangent of the terrain on given point.
        /// </summary>
        Vector3 GetTangent(int x, int z);

        /// <summary>
        /// Occured when the heightmap has changed.
        /// An optional rectangle region parameters is provided with the event
        /// to indicate the dirty region of heightmap change.
        /// </summary>
        event Action<Rectangle?> HeightmapChanged;
    }

    /// <summary>
    /// The geometric representation of heightmap. The up axis of the terrain is Vector.UnitY.
    /// </summary>
    [BinarySerializable]
    public class Heightmap : IHeightmap
    {
        #region Fields
        /// <summary>
        /// Gets the size of the smallest square block that made up the terrain.
        /// </summary>
        [BinarySerializable]
        public float Step { get; internal set; }
        
        /// <summary>
        /// Gets the number of the smallest square block in X axis, or heightmap texture U axis.
        /// </summary>
        public int Width { get; internal set; }

        /// <summary>
        /// Gets the number of the smallest square block in Z axis, or heightmap texture V axis.
        /// </summary>
        public int Height { get; internal set; }
        
        /// <summary>
        /// Occured when the heightmap has changed.
        /// An optional rectangle region parameters is provided with the event
        /// to indicate the dirty region of heightmap change.
        /// </summary>
        public event Action<Rectangle?> HeightmapChanged;

        [BinarySerializable]
        internal float[] Heights;
        [BinarySerializable]
        internal Vector3[] Normals;
        [BinarySerializable]
        internal Vector3[] Tangents;
        #endregion
        
        #region Methods
        internal Heightmap() { }

        /// <summary>
        /// Creates a new instance of Heightmap.
        /// </summary>
        /// <param name="step">Size of the smallest square block that made up the terrain.</param>
        /// <param name="segmentCountX">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="segmentCountZ">Number of the smallest square block in Z axis, or heightmap texture V axis.</param>
        public Heightmap(float step, int segmentCountX, int segmentCountZ)
            : this(new float[(segmentCountX + 1) * (segmentCountZ + 1)], step, segmentCountX, segmentCountZ)
        {
        }

        /// <summary>
        /// Creates a new instance of Heightmap.
        /// </summary>
        /// <param name="heightmap">Heights of each points. The dimension of the array should be (segmentCountX + 1) * (segmentCountZ + 1).</param>
        /// <param name="step">Size of the smallest square block that made up the terrain.</param>
        /// <param name="segmentCountX">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="segmentCountZ">Number of the smallest square block in Z axis, or heightmap texture V axis.</param>
        public Heightmap(float[] heightmap, float step, int segmentCountX, int segmentCountZ)
        {
            if (step <= 0 || segmentCountX <= 0 || segmentCountZ <= 0)
                throw new ArgumentOutOfRangeException();

            Step = step;
            Width = segmentCountX;
            Height = segmentCountZ;

            LoadHeightmap(heightmap);
        }

        /// <summary>
        /// Gets the position of the terrain on given point.
        /// </summary>
        /// <param name="x">Point on x axis.</param>
        /// <param name="z">Point on z axis.</param>
        public Vector3 GetPosition(int x, int z)
        {
            Vector3 result = new Vector3();

            result.X = Step * x;
            result.Z = Step * z;
            result.Y = Heights[GetIndex(x, z)];

            return result;
        }

        /// <summary>
        /// Gets the height of the terrain on given point.
        /// </summary>
        public float GetHeight(int x, int z)
        {
            return Heights[GetIndex(x, z)];
        }

        /// <summary>
        /// Gets the normal of the terrain on given point.
        /// </summary>
        public Vector3 GetNormal(int x, int z)
        {
            return Normals[GetIndex(x, z)];
        }

        /// <summary>
        /// Gets the tangent of the terrain on given point.
        /// </summary>
        public Vector3 GetTangent(int x, int z)
        {
            return Tangents[GetIndex(x, z)];
        }

        /// <summary>
        /// Gets the index of the terrain on given point. 
        /// The return value can be used to index Heights, Normals and Tangents.
        /// </summary>
        /// <param name="x">Point on x axis.</param>
        /// <param name="z">Point on z axis.</param>
        public int GetIndex(int x, int z)
        {
            if (x < 0 || z < 0 || x > Width || z > Height)
                throw new ArgumentOutOfRangeException();

            return z * (Width + 1) + x;
        }

        /// <summary>
        /// Loads this terrain geometry with the specified heightmap data.
        /// </summary>
        /// <param name="heightmap">Heights of each points. The dimension of the array should be (segmentCountX + 1) * (segmentCountY + 1).</param>
        public void LoadHeightmap(float[] heightmap)
        {
            Heights = heightmap;

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
                Step * Width, Step * Height, ref normals, ref tangents);

            Normals = normals;
            Tangents = tangents;

            // Fire invalidate event
            if (HeightmapChanged != null)
                HeightmapChanged(null);
        }

        private IEnumerable<Vector3> EnumeratePositions()
        {
            for (int x = 0; x <= Width; ++x)
            {
                for (int z = 0; z <= Height; z++)
                {
                    yield return GetPosition(x, z);
                }
            }
        }
        #endregion

        #region Terrain Normal & Tangent Data Generation
        private static Vector3 CalculatePosition(int x, int z, int w, int h, float[] heights, float sizeX, float sizeZ)
        {
            // Make sure we stay on the valid map data
            int mapX = x < 0 ? 0 : x >= w ? w - 1 : x;
            int mapZ = z < 0 ? 0 : z >= h ? h - 1 : z;

            Vector3 result = new Vector3();

            result.X = x * sizeX / (w - 1);
            result.Z = z * sizeZ / (h - 1);
            result.Y = heights[mapX + mapZ * w];

            return result;
        }

        private static Vector3[] normalsForSmoothing = null;

        /// <summary>
        /// Calculate normals from height data
        /// </summary>
        private static void CalculateNormalsAndTangents(int w, int h, float[] heights, float sizeX, float sizeZ, ref Vector3[] normals, ref Vector3[] tangents)
        {        
            #region Build tangent vertices
            // Build our tangent vertices
            for (int x = 0; x < w; ++x)
                for (int z = 0; z < h; z++)
                {
                    // Step 1: Calculate position
                    Vector3 pos = CalculatePosition(x, z, w, h, heights, sizeX, sizeZ);

                    // Step 2: Calculate all edge vectors (for normals and tangents)
                    Vector3 edge1 = pos - CalculatePosition(x, z - 1, w, h, heights, sizeX, sizeZ);
                    Vector3 edge2 = pos - CalculatePosition(x + 1, z, w, h, heights, sizeX, sizeZ);
                    Vector3 edge3 = pos - CalculatePosition(x, z + 1, w, h, heights, sizeX, sizeZ);
                    Vector3 edge4 = pos - CalculatePosition(x - 1, z, w, h, heights, sizeX, sizeZ);

                    // Step 3: Calculate normal based on the edges (interpolate
                    // from 3 cross products we build from our edges).
                    normals[x + z * w] = Vector3.Normalize(
                        Vector3.Cross(edge2, edge1) +
                        Vector3.Cross(edge3, edge2) +
                        Vector3.Cross(edge4, edge3) +
                        Vector3.Cross(edge1, edge4));

                    // Step 4: Set tangent data
                    tangents[x + z * w] = Vector3.Normalize(edge4);
                }
            #endregion
            
            #region Smooth normals
            // Smooth all normals, first copy them over, then smooth everything
            if (normalsForSmoothing == null || normalsForSmoothing.Length < w * h)
                normalsForSmoothing = new Vector3[w * h];

            Array.Copy(normals, normalsForSmoothing, normals.Length);

            // Time to smooth to normals we just saved
            for (int x = 1; x < w - 1; ++x)
            {
                for (int z = 1; z < h - 1; z++)
                {
                    // Smooth 3x3 normals, but still use old normal to 40% (5 of 13)
                    Vector3 normal = normals[x + z * w] * 4;
                    for (int xAdd = -1; xAdd <= 1; xAdd++)
                        for (int yAdd = -1; yAdd <= 1; yAdd++)
                            normal += normalsForSmoothing[x + xAdd + (z + yAdd) * w];
                    normals[x + z * w] = Vector3.Normalize(normal);

                    // Also recalculate tangent to let it stay 90 degrees on the normal
                    Vector3 helperVector = Vector3.Cross(normals[x + z * w], tangents[x + z * w]);
                    tangents[x + z * w] = Vector3.Cross(helperVector, normals[x + z * w]);
                }
            }
            #endregion
        }

        #endregion
        
        #region AttachedProperty
        private static AttachableMemberIdentifier SizeProperty = new AttachableMemberIdentifier(typeof(Heightmap), "Size");

        /// <summary>
        /// Gets the width of the heightmap.
        /// </summary>
        public static int GetWidth(Surface surface)
        {
            Vector3 value = Vector3.One;
            AttachablePropertyServices.TryGetProperty(surface, SizeProperty, out value);
            return (int)value.X;
        }

        /// <summary>
        /// Sets the width of the heightmap.
        /// </summary>
        public static void SetWidth(Surface surface, int value)
        {
            Vector3 size;
            if (!AttachablePropertyServices.TryGetProperty(surface, SizeProperty, out size))
                size = Vector3.UnitZ;
            size.X = value;
            AttachablePropertyServices.SetProperty(surface, SizeProperty, size);
        }

        /// <summary>
        /// Gets the height of the heightmap.
        /// </summary>
        public static int GetHeight(Surface surface)
        {
            Vector3 value = Vector3.One;
            AttachablePropertyServices.TryGetProperty(surface, SizeProperty, out value);
            return (int)value.Y;
        }

        /// <summary>
        /// Sets the height of the heightmap.
        /// </summary>
        public static void SetHeight(Surface surface, int value)
        {
            Vector3 size;
            if (!AttachablePropertyServices.TryGetProperty(surface, SizeProperty, out size))
                size = Vector3.UnitZ;
            size.Y = value;
            AttachablePropertyServices.SetProperty(surface, SizeProperty, size);
        }

        /// <summary>
        /// Gets the step of the heightmap.
        /// </summary>
        public static float GetStep(Surface surface)
        {
            Vector3 value = Vector3.One;
            return AttachablePropertyServices.TryGetProperty(surface, SizeProperty, out value) ? value.Z : 1;
        }

        /// <summary>
        /// Sets the step of the heightmap.
        /// </summary>
        public static void SetStep(Surface surface, float value)
        {
            Vector3 size = Vector3.One;
            AttachablePropertyServices.TryGetProperty(surface, SizeProperty, out size);
            size.Z = value;
            AttachablePropertyServices.SetProperty(surface, SizeProperty, size);
        }

        /// <summary>
        /// This method is called at runtime to adjust surface heightmap
        /// </summary>
        internal static void SetSize(Surface surface, Vector3 size)
        {
            surface.Heightmap = new Heightmap(size.Z, (int)size.X, (int)size.Y);
        }
        #endregion
    }

    /// <summary>
    /// Represents a flat heightmap.
    /// </summary>
    [BinarySerializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class FlatHeightmap : IHeightmap
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public float Step { get; set; }

        float IHeightmap.GetHeight(int x, int z) { return 0; }
        Vector3 IHeightmap.GetNormal(int x, int z) { return Vector3.Up; }
        Vector3 IHeightmap.GetTangent(int x, int z) { return Vector3.UnitX; }
        event Action<Rectangle?> IHeightmap.HeightmapChanged { add { } remove { } }
    }
}