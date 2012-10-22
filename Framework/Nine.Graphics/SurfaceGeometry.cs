namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Represents a terrain level of detail technique using GeoMipMapping.
    /// </summary>
    class SurfaceGeometry : IDisposable
    {
        private int patchSegmentCount;
        private GraphicsDevice graphics;
        private List<int> startIndices = new List<int>();

        /// <summary>
        /// Gets the index buffer.
        /// </summary>
        public IndexBuffer IndexBuffer { get; private set; }

        /// <summary>
        /// Gets the max level of detail.
        /// </summary>
        public int MaxLevelOfDetail { get; private set; }

        /// <summary>
        /// Gets a value indicating whether level of detail is enabled.
        /// </summary>
        public bool LevelOfDetailEnabled { get; private set; }
        
        /// <summary>
        /// Initializes a new instance of the <see cref="SurfaceGeometry"/> class.
        /// </summary>
        /// <param name="graphics">The graphics.</param>
        /// <param name="patchSegmentCount">The patch segment count.</param>
        public SurfaceGeometry(GraphicsDevice graphics, int patchSegmentCount)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (patchSegmentCount < 1)
                throw new ArgumentOutOfRangeException("Patch segment count must be at least 2.");

            if (!IsPowerOfTwo(patchSegmentCount))
                throw new ArgumentOutOfRangeException("Patch segment count must be a power of two.");

            this.graphics = graphics;
            this.patchSegmentCount = patchSegmentCount;
            this.MaxLevelOfDetail = LogBaseTowOf(patchSegmentCount);
        }

        /// <summary>
        /// Gets a static instance of SurfaceGeometry.
        /// </summary>
        public static SurfaceGeometry GetInstance(GraphicsDevice graphics, int patchSegmentCount)
        {
            SurfaceGeometry result = null;
            WeakReference<SurfaceGeometry> value;

            if (resourceDictionary.TryGetValue(new KeyValuePair<GraphicsDevice, int>(graphics, patchSegmentCount), out value))
            {
                if (value.TryGetTarget(out result))
                    return result;
                resourceDictionary.Remove(new KeyValuePair<GraphicsDevice, int>(graphics, patchSegmentCount));
            }

            result = (SurfaceGeometry)Activator.CreateInstance(typeof(SurfaceGeometry), graphics, patchSegmentCount);
            value = new WeakReference<SurfaceGeometry>(result);
            resourceDictionary.Add(new KeyValuePair<GraphicsDevice, int>(graphics, patchSegmentCount), value);

            return result;
        }
        static Dictionary<KeyValuePair<GraphicsDevice, int>, WeakReference<SurfaceGeometry>> resourceDictionary = new Dictionary<KeyValuePair<GraphicsDevice, int>, WeakReference<SurfaceGeometry>>();

        /// <summary>
        /// Enables the level of detail. By default it is not enabled.
        /// </summary>
        public void EnableLevelOfDetail()
        {
            if (!LevelOfDetailEnabled)
            {
                LevelOfDetailEnabled = true;
                UpdateIndexBuffer();
            }
        }

        /// <summary>
        /// Gets the level of detail.
        /// </summary>
        public void GetLevel(int level, int left, int right, int bottom, int top, out int startIndex, out int primitiveCount)
        {
            GetLevel(level, left > level, right > level, bottom > level, top > level, out startIndex, out primitiveCount);
        }

        /// <summary>
        /// Gets the level of detail.
        /// </summary>
        private void GetLevel(int level, bool left, bool right, bool bottom, bool top, out int startIndex, out int primitiveCount)
        {
            if (6 * patchSegmentCount * patchSegmentCount > ushort.MaxValue)
                throw new ArgumentOutOfRangeException();

            if (level > MaxLevelOfDetail)
                throw new ArgumentOutOfRangeException("Max level of detail value allowed is " + MaxLevelOfDetail);
                        
            // Resize index buffer
            if (IndexBuffer == null || IndexBuffer.IsDisposed)
            {
                UpdateIndexBuffer();
            }

            int border = 0;
            if (LevelOfDetailEnabled)
            {
                if (left)
                    border |= (1 << 0);
                if (right)
                    border |= (1 << 1);
                if (bottom)
                    border |= (1 << 2);
                if (top)
                    border |= (1 << 3);
                border = border + level * 16;
            }

            startIndex = startIndices[border];
            primitiveCount = (startIndices[border + 1] - startIndex) / 3;
        }

        private void UpdateIndexBuffer()
        {
            // Calculate index count.
            ushort[] indices = new ushort[GetEstimateIndexCount(MaxLevelOfDetail)];

            // Fill indices.
            int start = 0;
            int lod = 0;

            startIndices.Clear();
            startIndices.Add(0);
            while (lod <= MaxLevelOfDetail)
            {
                for (int i = 0; i < 16; ++i)
                {
                    if (LevelOfDetailEnabled || (lod == 0 && i == 0))
                    {
                        startIndices.Add(start = GetIndicesForLevel(patchSegmentCount, lod,
                            ((i >> 0) & 1) == 1, ((i >> 1) & 1) == 1,
                            ((i >> 2) & 1) == 1, ((i >> 3) & 1) == 1, indices, start));
                    }
                }
                lod++;
            }

            if (IndexBuffer != null)
                IndexBuffer.Dispose();
            IndexBuffer = new IndexBuffer(graphics, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
            IndexBuffer.SetData<ushort>(indices, 0, indices.Length);
        }

        private static int GetEstimateIndexCount(int maxLod)
        {
            int indexCount = 0;
            int level = 0;
            int step = 1;
            while (level <= maxLod)
            { 
                indexCount += step * step * 6 * 16;
                step *= 2;
                level++;
            }
            return indexCount;
        }

        /// <summary>
        /// Gets the indices for the specified detail level.
        /// </summary>
        public static int GetIndicesForLevel(int patchSegmentCount, int level, bool left, bool right, bool bottom, bool top, ushort[] indices, int startIndex)
        {
            int step = patchSegmentCount;
            for (int i = 0; i < level; ++i)
                step >>= 1;

            int n = Math.Max(step / 2, 1);
            int lengh = step == 1 ? 6 : Indices.Length;
            for (int y = 0; y < n; ++y)
            {
                for (int x = 0; x < n; ++x)
                {
                    for (int i = 0; i < lengh; ++i)
                    {
                        Point point = Indices[i /*InvertWindingOrder(i)*/];
                        if (left && x == 0 && point.Y == 1 && point.X == 0)
                            point.Y = 0;
                        if (right && x == n - 1 && point.Y == 1 && point.X == 2)
                            point.Y = 0;
                        if (bottom && y == 0 && point.X == 1 && point.Y == 0)
                            point.X = 0;
                        if (top && y == n - 1 && point.X == 1 && point.Y == 2)
                            point.X = 0;

                        startIndex = AddVertex(
                            PointToIndex(patchSegmentCount, x, y, patchSegmentCount / step, ref point), indices, startIndex);
                    }
                }
            }
            return startIndex;
        }

        private static int AddVertex(ushort value, ushort[] indices, int startIndex)
        {
            // Remove degenerated triangles
            if (vv == 0)
            {
                vv++;
                v0 = value;
                return startIndex;
            }
            if (vv == 1)
            {
                vv++;
                v1 = value;
                return startIndex;
            }
            else if (vv == 2)
            {
                vv = 0;
                if (value == v0 || value == v1 || v0 == v1)
                    return startIndex;
                if (indices == null)
                {
                    startIndex += 3;
                }
                else
                {
                    indices[startIndex++] = v0;
                    indices[startIndex++] = v1;
                    indices[startIndex++] = value;
                }
            }
            return startIndex;
        }
        static ushort v0, v1, vv;

        private static ushort PointToIndex(int patchSegmentCount, int x, int y, int step, ref Point point)
        {
            x = (point.X + x * 2) * step;
            y = (point.Y + y * 2) * step;

            return (ushort)(x + y * (patchSegmentCount + 1));
        }
        
        private static int InvertWindingOrder(int i)
        {
            int mod = i % 3;
            if (mod == 1)
                return i + 1;
            if (mod == 2)
                return i - 1;
            return i;
        }

        /// <summary>
        /// Gets the triangle indices at the specified grid.
        /// </summary>
        private Point[] GetTriangles(int x, int y)
        {
            if (x < 0 || x >= patchSegmentCount ||
                y < 0 || y >= patchSegmentCount)
            {
                throw new ArgumentOutOfRangeException();
            }

            int n = (x % 2) + (y % 2) * 2;
            for (int i = 0; i < 6; ++i)
            {
                Point pt = Indices[i + n * 6];

                triangles[i].X = pt.X + (x / 2) * 2;
                triangles[i].Y = pt.Y + (y / 2) * 2;
            }
            return triangles;
        }
        static Point[] triangles = new Point[6];
        
        /// <summary>
        /// Gets the indices of points that takes the following 8 triangles to makes up a square block.
        ///  ____ ____
        /// | \ 1|2 / |
        /// |0_\_|_/_3|
        /// |4 / | \ 7|
        /// |_/_5|6_\_|
        /// 
        /// </summary>
        static Point[] Points = new Point[]
        {
            new Point(0, 0), new Point(1, 0), new Point(2, 0),
            new Point(0, 1), new Point(1, 1), new Point(2, 1),
            new Point(0, 2), new Point(1, 2), new Point(2, 2),
        };

        static Point[] Indices = new Point[] 
        {
            Points[0], Points[4], Points[3], 
            Points[0], Points[1], Points[4], 

            Points[1], Points[2], Points[4], 
            Points[4], Points[2], Points[5], 

            Points[3], Points[4], Points[6], 
            Points[6], Points[4], Points[7], 

            Points[4], Points[8], Points[7], 
            Points[5], Points[8], Points[4], 
        };

        /// <summary>
        /// The input must be a power of two.
        /// Bitwise hack: http://graphics.stanford.edu/~seander/bithacks.html
        /// </summary>
        private int LogBaseTowOf(int powerOfTwo)
        {
            return MultiplyDeBruijnBitPosition2[(UInt32)(powerOfTwo * 0x077CB531U) >> 27];
        }

        static int[] MultiplyDeBruijnBitPosition2 = new int[32]
        {
            0, 1, 28, 2, 29, 14, 24, 3, 30, 22, 20, 15, 25, 17, 4, 8, 
            31, 27, 13, 23, 21, 19, 16, 7, 26, 12, 18, 6, 11, 5, 10, 9
        };

        private bool IsPowerOfTwo(int number)
        {
            return (number > 0) && (number & (number - 1)) == 0;
        }

        private static int AlignToPowerOfTwo(int v)
        {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            if (IndexBuffer != null)
            {
                IndexBuffer.Dispose();
                IndexBuffer = null;
            }
        }
    }
}
