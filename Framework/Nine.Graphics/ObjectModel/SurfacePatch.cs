#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.ObjectModel
{
    #region SurfacePatch
    /// <summary>
    /// A square block made up of surface patch parts. The whole surface is rendered patch by patch.
    /// </summary>
    [ContentSerializable]
    public class SurfacePatch : ISpatialQueryable, IDrawableObject, ILightable, IContainedObject, IGeometry, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets whether this object is visible.
        /// </summary>
        public bool Visible
        {
            get { return visible && Surface.Visible; }
            set { visible = value; }
        }
        private bool visible = true;

        /// <summary>
        /// Gets the number of segments of this patch.
        /// </summary>
        public int SegmentCount { get; internal set; }

        /// <summary>
        /// Gets the x index of the patch on the parent surface.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Gets the y index of the patch on the parent surface.
        /// </summary>
        public int Y { get; private set; }

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
            get { return Surface.AbsoluteTransform; } 
        }

        /// <summary>
        /// Gets the parent surface.
        /// </summary>
        public Surface Surface { get; private set; }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region BoundingBox & Position
        /// <summary>
        /// Gets the axis aligned bounding box of this surface patch.
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;

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
        #endregion

        #region ILightable
        Material IDrawableObject.Material 
        {
            get { return materialForRendering; } 
        }
        private Material materialForRendering;

        bool ILightable.CastShadow { get { return Surface.CastShadow; } }
        bool ILightable.ReceiveShadow { get { return Surface.ReceiveShadow; } }
        bool ILightable.LightingEnabled { get { return Surface.LightingEnabled; } }

        int ILightable.MaxReceivedShadows { get { return 4; } }
        int ILightable.MaxAffectingLights { get { return 1; } }

        bool ILightable.MultiPassLightingEnabled { get { return false; } }
        bool ILightable.MultiPassShadowEnabled { get { return false; } }

        object ILightable.LightingData { get; set; }
        #endregion

        #region IContainedObject
        object IContainedObject.Parent
        {
            get { return Surface; }
        }
        #endregion

        #region IGeometry
        Matrix? IGeometry.Transform { get { return Transform; } }

        Vector3[] IGeometry.Positions
        {
            get
            {
                if (positions == null)
                {
                    int i = 0;
                    positions = new Vector3[(SegmentCount + 1) * (SegmentCount + 1)];
                    for (int y = 0; y <= SegmentCount; y++)
                    {
                        for (int x = 0; x <= SegmentCount; x++)
                        {
                            int xSurface = (x + (X * SegmentCount));
                            int ySurface = (y + (Y * SegmentCount));

                            positions[i++] = Surface.Heightmap.GetPosition(xSurface, ySurface);
                        }
                    }
                }
                return positions;
            }
        }
        Vector3[] positions;

        ushort[] IGeometry.Indices
        {
            get
            {
                if (indices == null)
                {
                    int count = SurfaceGeometry.GetIndicesForLevel(SegmentCount, 0, false, false, false, false, null, 0); 
                    indices = new ushort[count];
                    SurfaceGeometry.GetIndicesForLevel(SegmentCount, 0, false, false, false, false, indices, 0);
                }
                return indices;
            }
        }
        ushort[] indices;
        #endregion

        #region Methods
        /// <summary>
        /// Constructor is for internal use only.
        /// </summary>
        internal SurfacePatch(Surface surface, int xPatch, int yPatch)
        {
            var patchSegmentCount = surface.PatchSegmentCount;
            if (6 * patchSegmentCount * patchSegmentCount > ushort.MaxValue)
                throw new ArgumentOutOfRangeException();

            if (!IsPowerOfTwo(patchSegmentCount))
                throw new ArgumentOutOfRangeException("PatchSegmentCount must be a power of two.");

            this.Surface = surface;
            this.heightmap = surface.Heightmap;
            this.GraphicsDevice = surface.GraphicsDevice;
            this.SegmentCount = patchSegmentCount;
            this.X = xPatch;
            this.Y = yPatch;

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
            for (int x = X * SegmentCount; x <= (X + 1) * SegmentCount; x++)
                for (int y = Y * SegmentCount; y <= (Y + 1) * SegmentCount; y++)
                    yield return heightmap.GetPosition(x, y);
        }

        internal void UpdatePosition()
        {
            BoundingBox box;

            box.Min = baseBounds.Min + Surface.AbsolutePosition;
            box.Max = baseBounds.Max + Surface.AbsolutePosition;

            BoundingBox = box;
            
            var offset = new Vector3();
            offset.X = X * heightmap.Step * SegmentCount;
            offset.Y = Y * heightmap.Step * SegmentCount;
            offset.Z = 0;

            Position = Surface.AbsolutePosition + offset;

            offset.X = 0.5f * heightmap.Step * SegmentCount;
            offset.Y = 0.5f * heightmap.Step * SegmentCount;
                        
            center = Position + offset;

            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        internal virtual void Invalidate()
        {
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Updates the level of detail.
        /// </summary>
        internal void UpdateLevelOfDetail()
        {
            int startIndex, primitiveCount;
            {
                Surface.Geometry.GetLevel(DetailLevel,
                    GetLevelOfDetail(X - 1, Y), GetLevelOfDetail(X + 1, Y),
                    GetLevelOfDetail(X, Y - 1), GetLevelOfDetail(X, Y + 1), out startIndex, out primitiveCount);
            }
            StartIndex = startIndex;
            PrimitiveCount = primitiveCount;
            IndexBuffer = Surface.Geometry.IndexBuffer;
        }

        private int GetLevelOfDetail(int x, int y)
        {
            var patch = Surface.Patches[x, y];
            return patch != null ? patch.DetailLevel : DetailLevel;
        }
        #endregion

        #region Draw
        /// <summary>
        /// Perform any updates before this object is drawed.
        /// </summary>
        public void BeginDraw(DrawingContext context)
        {
            materialForRendering = Surface.Material ??
                Surface.MaterialLevels.UpdateLevelOfDetail(Vector3.Distance(context.EyePosition, Center));
        }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            material.world = Surface.AbsoluteTransform;
            material.BeginApply(context);
            GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, StartIndex, PrimitiveCount);
            material.EndApply(context);
        }

        void IDrawableObject.EndDraw(DrawingContext context) { }
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
            SegmentCount = Surface.PatchSegmentCount;

            UpdateVertexBuffer();
            UpdateLevelOfDetail();

            base.Invalidate();
        }

        private void UpdateVertexBuffer()
        {
            // Reuse the array to avoid creating large chunk of data.
            VertexCount = (SegmentCount + 1) * (SegmentCount + 1);
            int indexCount = 6 * SegmentCount * SegmentCount;

            T[] vertices = null;
            if (WeakVertices == null)
            {
                // Cannot initialize weak reference to null in silverlight
                WeakVertices = new WeakReference<T[]>(vertices = new T[VertexCount]);
            }
            vertices = WeakVertices.Target;
            if (vertices == null || vertices.Length < VertexCount)
            {
                WeakVertices.Target = vertices = new T[VertexCount];
            }

            // Fill vertices
            int i = 0;
            VertexPositionNormalTexture vertex = new VertexPositionNormalTexture();
            for (int y = 0; y <= SegmentCount; y++)
            {
                for (int x = 0; x <= SegmentCount; x++)
                {
                    Surface.PopulateVertex(X, Y, x, y, ref vertex, ref vertex);
                    if (FillVertex != null)
                        FillVertex(X, Y, x, y, ref vertex, ref vertices[i]);
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
