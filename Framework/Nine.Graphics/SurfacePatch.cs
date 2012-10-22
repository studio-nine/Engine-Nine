namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    #region SurfacePatch
    /// <summary>
    /// A square block made up of surface patch parts. The whole surface is rendered patch by patch.
    /// </summary>
    public class SurfacePatch : ISpatialQueryable, IDrawableObject, ILightable, Nine.IComponent, IGeometry, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets whether this object is visible.
        /// </summary>
        public bool Visible
        {
            get { return visible && surface.Visible; }
            set { visible = value; }
        }
        private bool visible = true;

        /// <summary>
        /// Gets the number of segments of this patch.
        /// </summary>
        public int SegmentCount
        {
            get { return segmentCount; }
        }
        internal int segmentCount;

        /// <summary>
        /// Gets the x index of the patch on the parent surface.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Gets the z index of the patch on the parent surface.
        /// </summary>
        public int Z { get; private set; }

        /// <summary>
        /// Gets the level of detail of this surface patch.
        /// </summary>
        public int DetailLevel
        {
            get { return detailLevel; }

            // Do not allow set by externals since we need to make sure
            // lod difference between adjacent patches do not exceed 1.
            internal set { detailLevel = value; }
        }
        private int detailLevel = 0;

        /// <summary>
        /// Gets vertex buffer of this patch.
        /// </summary>
        public VertexBuffer VertexBuffer { get; internal set; }

        /// <summary>
        /// Gets index buffer of this patch.
        /// </summary>
        public IndexBuffer IndexBuffer { get; internal set; }

        /// <summary>
        /// Gets the start index of primitives that made up the patch.
        /// </summary>
        public int StartIndex { get; internal set; }
        
        /// <summary>
        /// Gets the number of primitives that made up the patch.
        /// </summary>
        public int PrimitiveCount { get; internal set; }

        /// <summary>
        /// Gets the number of vertices that made up the patch.
        /// </summary>
        public int VertexCount { get; internal set; }

        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the transform matrix used to draw the patch.
        /// </summary>
        public Matrix Transform
        {
            get { return surface.AbsoluteTransform; } 
        }

        /// <summary>
        /// Gets the parent surface.
        /// </summary>
        public Surface Surface 
        {
            get { return surface; }
        }
        internal Surface surface;

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region BoundingBox & Position
        /// <summary>
        /// Gets the axis aligned bounding box of this surface patch.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { surface.EnsureHeightmapUpToDate(); return boundingBox; }
        }
        private BoundingBox boundingBox;

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        event EventHandler<EventArgs> ISpatialQueryable.BoundingBoxChanged
        {
            add { boundingBoxChanged += value; }
            remove { boundingBoxChanged -= value; }
        }
        private EventHandler<EventArgs> boundingBoxChanged;

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Gets the bottom left position of the surface patch.
        /// </summary>
        public Vector3 Position { get; private set; }

        /// <summary>
        /// Gets or sets the center position of the surface patch.
        /// </summary>
        public Vector3 Center
        {
            get { return center; }
        }
        internal Vector3 center;

        private BoundingBox baseBounds;
        private Heightmap heightmap;
        private float distanceToCamera;
        #endregion

        #region ILightable
        Material IDrawableObject.Material 
        {
            get { return materialForRendering; } 
        }
        private Material materialForRendering;

        bool IDrawableObject.CastShadow { get { return surface.CastShadow; } }

        int ILightable.MaxReceivedShadows { get { return 4; } }
        int ILightable.MaxAffectingLights { get { return 1; } }

        bool ILightable.MultiPassLightingEnabled { get { return false; } }
        bool ILightable.MultiPassShadowEnabled { get { return false; } }

        object ILightable.LightingData { get; set; }
        #endregion

        #region IContainedObject
        IContainer IComponent.Parent
        {
            get { return surface; }
            set { throw new InvalidOperationException(); }
        }
        #endregion

        #region IGeometry
        /// <summary>
        /// Gets the triangle vertices of the target geometry.
        /// </summary>        
        public bool TryGetTriangles(out Vector3[] vertices, out ushort[] indices)
        {
            if (this.geometryPositions == null)
            {
                int i = 0;
                this.geometryPositions = new Vector3[(segmentCount + 1) * (segmentCount + 1)];
                for (int z = 0; z <= segmentCount; z++)
                {
                    for (int x = 0; x <= segmentCount; ++x)
                    {
                        int xSurface = (x + (X * segmentCount));
                        int zSurface = (z + (Z * segmentCount));

                        geometryPositions[i++] = surface.heightmap.GetPosition(xSurface, zSurface);
                    }
                }

                int count = SurfaceGeometry.GetIndicesForLevel(segmentCount, 0, false, false, false, false, null, 0);
                geometryIndices = new ushort[count];
                SurfaceGeometry.GetIndicesForLevel(segmentCount, 0, false, false, false, false, geometryIndices, 0);
            }
            vertices = this.geometryPositions;
            indices = this.geometryIndices;
            return true;
        }
        Vector3[] geometryPositions;
        ushort[] geometryIndices;
        #endregion

        #region Methods
        /// <summary>
        /// Constructor is for internal use only.
        /// </summary>
        internal SurfacePatch(Surface surface, int xPatch, int zPatch)
        {
            var patchSegmentCount = surface.PatchSegmentCount;
            if (6 * patchSegmentCount * patchSegmentCount > ushort.MaxValue)
                throw new ArgumentOutOfRangeException();

            if (!IsPowerOfTwo(patchSegmentCount))
                throw new ArgumentOutOfRangeException("PatchSegmentCount must be a power of two.");

            this.surface = surface;
            this.heightmap = surface.Heightmap;
            this.GraphicsDevice = surface.GraphicsDevice;
            this.segmentCount = patchSegmentCount;
            this.X = xPatch;
            this.Z = zPatch;

            // Compute bounding box
            baseBounds = BoundingBox.CreateFromPoints(EnumeratePositions());
            UpdatePosition();
        }

        private bool IsPowerOfTwo(int number)
        {
            return (number > 0) && (number & (number - 1)) == 0;
        }

        private IEnumerable<Vector3> EnumeratePositions()
        {
            for (int x = X * segmentCount; x <= (X + 1) * segmentCount; ++x)
                for (int y = Z * segmentCount; y <= (Z + 1) * segmentCount; ++y)
                    yield return heightmap.GetPosition(x, y);
        }

        internal void UpdatePosition()
        {
            boundingBox.Min = baseBounds.Min + surface.AbsolutePosition - surface.BoundingBoxPadding;
            boundingBox.Max = baseBounds.Max + surface.AbsolutePosition + surface.BoundingBoxPadding;
            
            var offset = new Vector3();
            offset.X = X * heightmap.Step * segmentCount;
            offset.Z = Z * heightmap.Step * segmentCount;
            offset.Y = 0;

            Position = surface.AbsolutePosition + offset;

            offset.X = 0.5f * heightmap.Step * segmentCount;
            offset.Z = 0.5f * heightmap.Step * segmentCount;
                        
            center = Position + offset;

            if (boundingBoxChanged != null)
                boundingBoxChanged(this, EventArgs.Empty);
        }

        internal virtual void Invalidate()
        {
            if (boundingBoxChanged != null)
                boundingBoxChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Updates the level of detail.
        /// </summary>
        internal void UpdateLevelOfDetail()
        {
            int startIndex, primitiveCount;
            {
                surface.Geometry.GetLevel(DetailLevel,
                    GetLevelOfDetail(X - 1, Z), GetLevelOfDetail(X + 1, Z),
                    GetLevelOfDetail(X, Z - 1), GetLevelOfDetail(X, Z + 1), out startIndex, out primitiveCount);
            }
            StartIndex = startIndex;
            PrimitiveCount = primitiveCount;
            IndexBuffer = surface.Geometry.IndexBuffer;
        }

        private int GetLevelOfDetail(int x, int z)
        {
            var patch = surface.Patches[x, z];
            return patch != null ? patch.DetailLevel : DetailLevel;
        }
        #endregion

        #region Draw
        /// <summary>
        /// Perform any updates before this object is drawed.
        /// </summary>
        public bool OnAddedToView(DrawingContext context)
        {
            Vector3.Distance(ref context.matrices.cameraPosition, ref center, out distanceToCamera);
            materialForRendering = surface.Material ?? surface.MaterialLevels.UpdateLevelOfDetail(distanceToCamera);
            return visible;
        }

        /// <summary>
        /// Gets the squared distance from the position of the object to the current camera.
        /// </summary>
        public float GetDistanceToCamera(ref Vector3 cameraPosition)
        {
            return distanceToCamera;
        }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
            GraphicsDevice.Indices = IndexBuffer;
            context.SetVertexBuffer(VertexBuffer, 0);

            material.world = surface.AbsoluteTransform;
            material.BeginApply(context);
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, StartIndex, PrimitiveCount);
            material.EndApply(context);
        }
        #endregion

        #region Dispose
        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (VertexBuffer != null)
                    VertexBuffer.Dispose();
                // Don't dispose the index buffer since it is shared between surfaces.
            }
        }

        ~SurfacePatch()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    #region SurfacePatch<T>
    /// <summary>
    /// To support ConvertVertexType, we need to store T using generic subclass.
    /// </summary>
    class SurfacePatch<T> : SurfacePatch where T: struct, IVertexType
    {
        public SurfaceVertexConverter<T> FillVertex;
        private static WeakReference<T[]> WeakVertices;

        internal SurfacePatch(Surface surface, int xPatch, int yPatch)
            : base(surface, xPatch, yPatch)
        { }

        /// <summary>
        /// Invalidates this instance.
        /// </summary>
        internal override void Invalidate()
        {
            segmentCount = surface.PatchSegmentCount;

            UpdateVertexBuffer();
            UpdateLevelOfDetail();

            base.Invalidate();
        }

        private void UpdateVertexBuffer()
        {
            // Reuse the array to avoid creating large chunk of data.
            VertexCount = (segmentCount + 1) * (segmentCount + 1);
            int indexCount = 6 * segmentCount * segmentCount;

            T[] vertices = null;
            if (WeakVertices == null)
            {
                // Cannot initialize weak reference to null in silverlight
                WeakVertices = new WeakReference<T[]>(vertices = new T[VertexCount]);
            }
            if (!WeakVertices.TryGetTarget(out vertices) || vertices == null || vertices.Length < VertexCount)
            {
                WeakVertices.SetTarget(vertices = new T[VertexCount]);
            }

            // Fill vertices
            int i = 0;
            VertexPositionNormalTexture vertex = new VertexPositionNormalTexture();
            for (int z = 0; z <= segmentCount; z++)
            {
                for (int x = 0; x <= segmentCount; ++x)
                {
                    surface.PopulateVertex(X, Z, x, z, ref vertex, ref vertex);
                    if (FillVertex != null)
                        FillVertex(X, Z, x, z, ref vertex, ref vertices[i]);
                    i++;
                }
            }

            if (VertexBuffer == null)
                VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(T), VertexCount, BufferUsage.WriteOnly);
            VertexBuffer.SetData<T>(vertices, 0, VertexCount);
        }
    }
    #endregion
}
