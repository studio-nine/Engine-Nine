namespace Nine.Graphics.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

    /// <summary>
    /// Base class for simple geometric primitive models. 
    /// </summary>
    [ContentProperty("Material")]
    public abstract class Primitive<T> : Transformable, ISpatialQueryable, IDrawableObject, IGeometry, ISupportInstancing, ILightable, IDisposable where T : struct, IVertexType
    {
        #region Properties
        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets whether the drawable is visible.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        private bool visible;

        /// <summary>
        /// Gets whether the object casts shadow.
        /// </summary>
        public bool CastShadow { get; set; }

        /// <summary>
        /// Gets a value indicating whether this primitive resides inside the view frustum last frame.
        /// </summary>
        public bool InsideViewFrustum { get; internal set; }

        /// <summary>
        /// Gets the material used by this drawable.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets a collection containing all the materials that are sorted based on level of detail.
        /// </summary>
        public MaterialLevelOfDetail MaterialLevels
        {
            get { return materialLevels; }
            set { materialLevels = value ?? new MaterialLevelOfDetail(); }
        }
        private MaterialLevelOfDetail materialLevels = new MaterialLevelOfDetail();

        Material IDrawableObject.Material
        {
            get { return materialForRendering; }
        }
        private Material materialForRendering;

        /// <summary>
        /// Gets the optional bounding sphere of the primitive.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                if (needsRebuild)
                    Rebuild();
                return boundingBox;
            }
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
        /// Gets or sets a value indicating whether winding order will be inverted.
        /// </summary>
        public bool InvertWindingOrder
        {
            get { return invertWindingOrder; }
            set
            {
                if (invertWindingOrder != value)
                {
                    invertWindingOrder = value;
                    needsRebuild = true;
                }
            }
        }
        private bool invertWindingOrder;

        /// <summary>
        /// Gets the primitive type of this primitive.
        /// </summary>
        public PrimitiveType PrimitiveType
        {
            get { return primitiveType; }
            protected set
            {
                if (primitiveType != value)
                {
                    primitiveType = value;
                    needsRebuild = true;
                }
            }
        }
        private PrimitiveType primitiveType = PrimitiveType.TriangleList;

        private bool needsRebuild;
        private PrimitiveCache cachedPrimitive;
        private float distanceToCamera;

        /// <summary>
        /// Properties used when building the primitives
        /// </summary>
        private static List<T> Vertices = new List<T>();
        private static List<Vector3> Positions = new List<Vector3>();
        private static List<ushort> Indices = new List<ushort>();

        /// <summary>
        /// Primitives sharing the same vb/ib are cached here.
        /// </summary>
        private static Dictionary<Type, List<PrimitiveCache>> PrimitiveCache = new Dictionary<Type, List<PrimitiveCache>>();
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="Primitive&lt;T&gt;"/> class.
        /// </summary>
        protected Primitive(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Visible = true;
            CastShadow = true;
            needsRebuild = true;
            GraphicsDevice = graphics;
        }

        /// <summary>
        /// Requests for a rebuild of the primitive vertices and indices.
        /// </summary>
        protected void Invalidate()
        {
            needsRebuild = true;
        }

        /// <summary>
        /// Rebuilds the primitive vertices and indices.
        /// </summary>
        private void Rebuild()
        {
            Dispose();

            // Check if we can find a cached vb/ib for this primitive
            Type type = GetType();
            List<PrimitiveCache> cachedPrimitives;
            if (PrimitiveCache.TryGetValue(type, out cachedPrimitives))
            {
                for (int i = 0; i < cachedPrimitives.Count; ++i)
                {
                    cachedPrimitive = cachedPrimitives[i];
                    if (cachedPrimitive.IsDisposed)
                    {
                        cachedPrimitives[i] = cachedPrimitives[cachedPrimitives.Count - 1];
                        cachedPrimitives.RemoveAt(cachedPrimitives.Count - 1);
                        i--;
                        continue;
                    }

                    var primitive = (Primitive<T>)cachedPrimitive.Primitive;
                    if (primitive.GraphicsDevice == GraphicsDevice &&
                        primitive.invertWindingOrder == invertWindingOrder &&
                        CanShareBufferWith((Primitive<T>)cachedPrimitive.Primitive))
                    {
                        cachedPrimitive.RefCount++;
                        UpdateBoundingBox();
                        needsRebuild = false;
                        return;
                    }
                }
            }

            OnBuild();

            cachedPrimitive = new PrimitiveCache();
            cachedPrimitive.RefCount = 1;
            cachedPrimitive.Primitive = this;
            cachedPrimitive.Positions = Positions.ToArray();
            cachedPrimitive.Indices = Indices.ToArray();

            // Create a vertex buffer, and copy our vertex data into it.            
            cachedPrimitive.VertexBuffer = new VertexBuffer(GraphicsDevice, typeof(T), Vertices.Count, BufferUsage.WriteOnly);
            cachedPrimitive.VertexBuffer.SetData(Vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            if (Indices.Count > 0)
            {
                cachedPrimitive.IndexBuffer = new IndexBuffer(GraphicsDevice, typeof(ushort), Indices.Count, BufferUsage.WriteOnly);

                // Handle winding order
                if (invertWindingOrder)
                {
                    for (int i = 0; i < Indices.Count; i += 3)
                    {
                        var temp = Indices[i + 1];
                        Indices[i + 1] = Indices[i + 2];
                        Indices[i + 2] = temp;
                    }
                }

                cachedPrimitive.IndexBuffer.SetData(Indices.ToArray());
            }

            cachedPrimitive.PrimitiveCount = Helper.GetPrimitiveCount(primitiveType,
                cachedPrimitive.IndexBuffer != null ? cachedPrimitive.IndexBuffer.IndexCount
                                                    : cachedPrimitive.VertexBuffer.VertexCount);

            cachedPrimitive.BoundingBox = BoundingBox.CreateFromPoints(Positions);
            UpdateBoundingBox();

            needsRebuild = false;

            // Add to primitive cache
            if (cachedPrimitives == null)
                PrimitiveCache.Add(type, cachedPrimitives = new List<PrimitiveCache>());
            cachedPrimitives.Add(cachedPrimitive);

            // Free up the list.
            Positions.Clear();
            Indices.Clear();
            Vertices.Clear();
        }

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            UpdateBoundingBox();
            base.OnTransformChanged();
        }

        private void UpdateBoundingBox()
        {
            if (cachedPrimitive != null)
            {
                boundingBox = BoundingBoxExtensions.CreateAxisAligned(cachedPrimitive.BoundingBox, AbsoluteTransform);
                if (boundingBoxChanged != null)
                    boundingBoxChanged(this, EventArgs.Empty);
            }
        }

        /// <summary>
        /// Determines whether this instance can share its vertex and index buffers with the specified primitive.
        /// </summary>
        protected virtual bool CanShareBufferWith(Primitive<T> primitive)
        {
            return false;
        }

        /// <summary>
        /// When implemented by derived classes. Call the AddVertex or AddIndex methods to build the prmitive.
        /// </summary>
        protected abstract void OnBuild();

        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, T vertex)
        {
            Vertices.Add(vertex);
            Positions.Add(position);
        }

        /// <summary>
        /// Adds a new index to the primitive model. This should only be called
        /// during the InitializePrimitive.
        /// </summary>
        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            Indices.Add((ushort)index);
        }

        /// <summary>
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex
        {
            get { return Vertices.Count; }
        }
        #endregion

        #region Draw
        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public bool OnAddedToView(DrawingContext context)
        {
            if (!visible)
                return false;

            InsideViewFrustum = true;

            var worldTransform = AbsoluteTransform;

            var xx = (context.matrices.cameraPosition.X - worldTransform.M41);
            var yy = (context.matrices.cameraPosition.Y - worldTransform.M42);
            var zz = (context.matrices.cameraPosition.Z - worldTransform.M43);

            distanceToCamera = (float)Math.Sqrt(xx * xx + yy * yy + zz * zz);

            materialForRendering = Material ?? materialLevels.UpdateLevelOfDetail(distanceToCamera);
            return true;
        }

        /// <summary>
        /// Gets the distance from the position of the object to the current camera.
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
            material.world = AbsoluteTransform;
            material.BeginApply(context);

            if (needsRebuild)
                Rebuild();

            context.SetVertexBuffer(cachedPrimitive.VertexBuffer, 0);

            if (cachedPrimitive.IndexBuffer != null)
            {
                context.graphics.Indices = cachedPrimitive.IndexBuffer;
                context.graphics.DrawIndexedPrimitives(primitiveType, 0, 0,
                    cachedPrimitive.VertexBuffer.VertexCount, 0, cachedPrimitive.PrimitiveCount);
            }
            else
            {
                context.graphics.DrawPrimitives(primitiveType, 0, cachedPrimitive.PrimitiveCount);
            }

            material.EndApply(context);
        }

        Matrix IGeometry.Transform
        {
            get { return AbsoluteTransform; }
        }

        /// <summary>
        /// Gets the triangle vertices of the target geometry.
        /// </summary>
        public bool TryGetTriangles(out Vector3[] vertices, out ushort[] indices)
        {
            vertices = cachedPrimitive.Positions;
            indices = cachedPrimitive.Indices;
            return true;
        }

        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (cachedPrimitive != null && !cachedPrimitive.IsDisposed && (--cachedPrimitive.RefCount) <= 0)
                    cachedPrimitive.Dispose();
            }
        }
        #endregion

        #region ISupportInstancing
        int ISupportInstancing.MeshCount
        {
            get { return 1; }
        }

        void ISupportInstancing.GetVertexBuffer(int subset, out VertexBuffer vertexBuffer, out int vertexOffset, out int numVertices)
        {
            VerifyInstancingPrimitveType();

            if (needsRebuild)
                Rebuild();
            if (!Visible)
            {
                vertexBuffer = null;
                vertexOffset = 0;
                numVertices = 0;
                return;
            }
            vertexBuffer = cachedPrimitive.VertexBuffer;
            numVertices = cachedPrimitive.VertexBuffer.VertexCount;
            vertexOffset = 0;
        }

        void ISupportInstancing.GetIndexBuffer(int subset, out IndexBuffer indexBuffer, out int startIndex, out int primitiveCount)
        {
            VerifyInstancingPrimitveType();

            if (needsRebuild)
                Rebuild();

            if (!Visible)
            {
                indexBuffer = null;
                startIndex = 0;
                primitiveCount = 0;
                return;
            }
            indexBuffer = cachedPrimitive.IndexBuffer;
            primitiveCount = cachedPrimitive.PrimitiveCount;
            startIndex = 0;
        }

        Material ISupportInstancing.GetMaterial(int subset)
        {
            VerifyInstancingPrimitveType();

            // Material Lod is not enabled when using instancing.
            return Material;
        }

        void ISupportInstancing.PrepareMaterial(int subset, Material material)
        {

        }

        void VerifyInstancingPrimitveType()
        {
            if (primitiveType != PrimitiveType.TriangleList)
                throw new NotSupportedException("The current primitive cannot be used for instancing");
        }
        #endregion

        #region ILightable
        bool ILightable.MultiPassLightingEnabled { get { return false; } }
        int ILightable.MaxAffectingLights { get { return 1; } }
        bool ILightable.MultiPassShadowEnabled { get { return false; } }
        int ILightable.MaxReceivedShadows { get { return 1; } }
        object ILightable.LightingData { get; set; }
        #endregion
    }

    class PrimitiveCache : IDisposable
    {
        public int RefCount;
        public bool IsDisposed;
        public bool InvertWindingOrder;
        public int PrimitiveCount;
        public VertexBuffer VertexBuffer;
        public IndexBuffer IndexBuffer;
        public BoundingBox BoundingBox;
        public object Primitive;
        public Vector3[] Positions;
        public ushort[] Indices;

        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();
            if (IndexBuffer != null)
                IndexBuffer.Dispose();
            IsDisposed = true;
        }
    }
}
