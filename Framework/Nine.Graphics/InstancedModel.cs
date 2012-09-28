namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.Materials.MaterialParts;        
    
    #region InstancedModel
    /// <summary>
    /// Defines an instanced model that can be rendered using hardware instancing.
    /// </summary>
    [ContentProperty("Template")]
    public class InstancedModel : Transformable, IContainer, ISpatialQueryable, Nine.IUpdateable, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Model"/> should be visible.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        internal bool visible = true;

        /// <summary>
        /// Gets or sets the maximum rage of visible instances. Null means all instances are visible.
        /// </summary>
        public float? ViewDistance { get; set; }

        /// <summary>
        /// Gets a value indicating whether this model resides inside the view frustum last frame.
        /// </summary>
        /// <remarks>
        /// This value is only valid before the model is updated.
        /// </remarks>
        public bool InsideViewFrustum
        {
            get { return insideViewFrustum; }
        }
        internal bool insideViewFrustum;

        /// <summary>
        /// Gets the model meshes that made up of this model.
        /// </summary>
        public ISupportInstancing Template
        {
            get { return template; }
            set 
            {
                if (template != value)
                {
                    template = value;
                    UpdateTemplate();
                }
            }
        }
        internal ISupportInstancing template;
                
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }
        private BoundingBox boundingBox = new BoundingBox();
        private BoundingBox orientedBoundingBox = new BoundingBox();

        /// <summary>
        /// Gets or sets a value that is appended to the computed bounding box to
        /// compensate scale & rotation loss, since the auto computed bounding box 
        /// does not care about the scale & rotation of instance transforms.
        /// </summary>
        public Vector3 BoundingBoxPadding { get; set; }

        /// <summary>
        /// Called when transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            Matrix transform = AbsoluteTransform;
            orientedBoundingBox.CreateAxisAligned(ref transform, out boundingBox);

            if (boundingBoxChanged != null)
                boundingBoxChanged(this, EventArgs.Empty);
        }

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        event EventHandler<EventArgs> ISpatialQueryable.BoundingBoxChanged
        {
            add { boundingBoxChanged += value; }
            remove { boundingBoxChanged -= value; }
        }
        private EventHandler<EventArgs> boundingBoxChanged;


        IList IContainer.Children
        {
            get { return meshes; }
        }

        [ContentSerializer]
        internal Matrix[] instanceTransforms;

        private int instanceCount;
        private bool instanceTransformsNeedsUpdate;
        private DynamicVertexBuffer instanceBuffer;
        private VertexDeclaration vertexDeclaration;
        private List<InstancedModelMesh> meshes = new List<InstancedModelMesh>();
        #endregion
        
        #region Methods
        /// <summary>
        /// Initializes a new instance of the <see cref="InstancedModel"/> class.
        /// </summary>
        public InstancedModel(GraphicsDevice graphicsDevice) : this(graphicsDevice, null)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InstancedModel"/> class.
        /// </summary>
        public InstancedModel(GraphicsDevice graphicsDevice, ISupportInstancing template)
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            GraphicsDevice = graphicsDevice;
            Template = template;
        }

        /// <summary>
        /// Gets the instance transforms.
        /// </summary>
        public Matrix[] GetInstanceTransforms()
        {
            return instanceTransforms; 
        }

        /// <summary>
        /// Sets the instance transforms.
        /// </summary>
        public void SetInstanceTransforms(Matrix[] transforms)
        {
            instanceTransforms = transforms;
            instanceTransformsNeedsUpdate = true;
            UpdateBounds();
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            var updateable = template as Nine.IUpdateable;
            if (updateable != null)
                updateable.Update(elapsedTime);
        }

        private void UpdateTemplate()
        {
            meshes.Clear();
            if (template != null)
            {
                var count = template.Count;
                for (int i = 0; i < count; ++i)
                    meshes.Add(new InstancedModelMesh(this, i));
            }
            UpdateBounds();
        }

        private void UpdateBounds()
        {
            orientedBoundingBox = (template != null) ? template.BoundingBox : new BoundingBox();

            if (instanceTransforms != null && instanceTransforms.Length > 0)
            {
                BoundingBox instanceBounds;
                instanceBounds.Min = Vector3.One * float.MaxValue;
                instanceBounds.Max = Vector3.One * float.MinValue;

                for (int i = 0; i < instanceTransforms.Length; ++i)
                {
                    // TODO: Include scale & rotation
                    if (instanceTransforms[i].M41 > instanceBounds.Max.X)
                        instanceBounds.Max.X = instanceTransforms[i].M41;
                    else if (instanceTransforms[i].M41 < instanceBounds.Min.X)
                        instanceBounds.Min.X = instanceTransforms[i].M41;

                    if (instanceTransforms[i].M42 > instanceBounds.Max.Y)
                        instanceBounds.Max.Y = instanceTransforms[i].M42;
                    else if (instanceTransforms[i].M42 < instanceBounds.Min.Y)
                        instanceBounds.Min.Y = instanceTransforms[i].M42;

                    if (instanceTransforms[i].M43 > instanceBounds.Max.Z)
                        instanceBounds.Max.Z = instanceTransforms[i].M43;
                    else if (instanceTransforms[i].M43 < instanceBounds.Min.Z)
                        instanceBounds.Min.Z = instanceTransforms[i].M43;
                }

                orientedBoundingBox.Min -= instanceBounds.Min;
                orientedBoundingBox.Max += instanceBounds.Max;
            }

            Matrix transform = AbsoluteTransform;
            orientedBoundingBox.Min -= BoundingBoxPadding;
            orientedBoundingBox.Max += BoundingBoxPadding;
            orientedBoundingBox.CreateAxisAligned(ref transform, out boundingBox);

            if (boundingBoxChanged != null)
                boundingBoxChanged(this, EventArgs.Empty);
        }

        internal void GetInstanceBuffer(ref Vector3 cameraPosition, bool forceUpdate, out VertexBuffer vertexBuffer, out int vertexCount)
        {
            if (instanceTransforms == null || instanceTransforms.Length <= 0)
            {
                vertexBuffer = null;
                vertexCount = 0;
                return;
            }

            instanceTransformsNeedsUpdate |= forceUpdate;
            if (instanceBuffer == null || instanceTransformsNeedsUpdate || instanceBuffer.VertexCount < instanceTransforms.Length)
            {
                if (vertexDeclaration == null)
                {
                    vertexDeclaration = new VertexDeclaration
                    (
                        new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 4),
                        new VertexElement(16, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 5),
                        new VertexElement(32, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 6),
                        new VertexElement(48, VertexElementFormat.Vector4, VertexElementUsage.BlendWeight, 7)
                    );
                }

                if (instanceBuffer != null)
                    instanceBuffer.Dispose();

                instanceBuffer = new DynamicVertexBuffer(GraphicsDevice, vertexDeclaration, instanceTransforms.Length, BufferUsage.WriteOnly);                
                instanceTransformsNeedsUpdate = true;
            }

            vertexCount = instanceCount;
            vertexBuffer = instanceBuffer;

#if SILVERLIGHT
            if (instanceTransformsNeedsUpdate)
#else
            if (instanceTransformsNeedsUpdate || instanceBuffer.IsContentLost)
#endif
            {
                instanceTransformsNeedsUpdate = false;

                var data = instanceTransforms;
                instanceCount = vertexCount = instanceTransforms.Length;

                if (ViewDistance.HasValue)
                {
                    Matrix worldToLocal = AbsoluteTransform;
                    Matrix.Invert(ref worldToLocal, out worldToLocal);
                    Vector3.Transform(ref cameraPosition, ref worldToLocal, out cameraPosition);

                    PopulateVisibleInstances(ref cameraPosition);

                    data = VisibleInstanceTransforms;
                    instanceCount = vertexCount = VisibleInstanceCount;
                    if (instanceCount <= 0)
                        return;
                }

                instanceBuffer.SetData(data, 0, instanceCount, SetDataOptions.Discard);
            }
        }

        /// <summary>
        /// Culls each instance based on camera position.
        /// </summary>
        private void PopulateVisibleInstances(ref Vector3 localCameraPosition)
        {
            if (VisibleInstanceTransforms == null || VisibleInstanceTransforms.Length < instanceTransforms.Length)
            {
                VisibleInstanceTransforms = new Matrix[instanceTransforms.Length];
            }

            VisibleInstanceCount = 0;

            var viewDistanceSq = ViewDistance.Value * ViewDistance.Value;
            var length = instanceTransforms.Length;

            for (var i = 0; i < length; ++i)
            {
                var transform = instanceTransforms[i];
                var xx = transform.M41 - localCameraPosition.X;
                var yy = transform.M42 - localCameraPosition.Y;
                var zz = transform.M43 - localCameraPosition.Z;
                var distanceSq = xx * xx + yy * yy + zz * zz;

                if (distanceSq <= viewDistanceSq)
                {
                    VisibleInstanceTransforms[VisibleInstanceCount++] = transform;
                }
            }
        }

        private static Matrix[] VisibleInstanceTransforms;
        private static int VisibleInstanceCount;
        
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
                if (instanceBuffer != null)
                {
                    instanceBuffer.Dispose();
                    instanceBuffer = null;
                }
                if (vertexDeclaration != null)
                {
                    vertexDeclaration.Dispose();
                    vertexDeclaration = null;
                }
                if (template is IDisposable)
                {
                    ((IDisposable)template).Dispose();
                    template = null;
                }
            }
        }

        ~InstancedModel()
        {
            Dispose(false);
        }
        #endregion
    }
    #endregion

    #region InstancedModelMesh
    /// <summary>
    /// Defines an instanced model that can be rendered using hardware instancing.
    /// </summary>
    class InstancedModelMesh : IDrawableObject
    {
        private InstancedModel model;
        private int index;
        
        static VertexBufferBinding[] Bindings = new VertexBufferBinding[2];

        public InstancedModelMesh(InstancedModel model, int index)
        {
            this.model = model;
            this.index = index;
        }

        void IDrawableObject.OnAddedToView(DrawingContext context)
        {
            VertexBuffer instanceBuffer;
            int instanceCount;
            model.GetInstanceBuffer(ref context.matrices.cameraPosition, true, out instanceBuffer, out instanceCount);
        }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
#if SILVERLIGHT
            throw new NotSupportedException();
#else
            if (model.template == null)
                return;

            VertexBuffer instanceBuffer;
            int instanceCount;
            model.GetInstanceBuffer(ref context.matrices.cameraPosition, false, out instanceBuffer, out instanceCount);
            if (instanceBuffer == null)
                return;

            var materialGroup = material as MaterialGroup;
            if (materialGroup == null)
                return;
            if (materialGroup.Find<InstancedMaterialPart>() == null)
                return;
            
            VertexBuffer vertexBuffer;
            IndexBuffer indexBuffer;
            int vertexOffset;
            int numVertices;
            int startIndex;
            int primitiveCount;

            model.template.GetVertexBuffer(index, out vertexBuffer, out vertexOffset, out numVertices);
            model.template.GetIndexBuffer(index, out indexBuffer, out startIndex, out primitiveCount);
            
            if (vertexBuffer == null)
                return;

            context.SetVertexBuffer(null, 0);

            Bindings[0] = new VertexBufferBinding(vertexBuffer, vertexOffset, 0);
            Bindings[1] = new VertexBufferBinding(instanceBuffer, 0, 1);
            
            model.GraphicsDevice.SetVertexBuffers(Bindings);
            model.GraphicsDevice.Indices = indexBuffer;

            model.template.PrepareMaterial(index, material);
            material.world = model.AbsoluteTransform;
            material.BeginApply(context);
            model.GraphicsDevice.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, numVertices, startIndex, primitiveCount, instanceCount);
            material.EndApply(context);
#endif
        }

        float IDrawableObject.GetDistanceToCamera(Vector3 cameraPosition)
        {
            return 0;
        }

        bool IDrawableObject.Visible
        {
            get { return model.visible; }
        }

        bool IDrawableObject.CastShadow
        {
            get { return false; } 
        
        }
        Material IDrawableObject.Material
        {
            get { return model.template.GetMaterial(index); }
        }
    }
    #endregion
}