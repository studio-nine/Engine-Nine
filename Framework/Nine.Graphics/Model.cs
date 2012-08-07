namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Defines a basic model that can be rendered using the renderer with a designated material.
    /// </summary>
    [ContentProperty("Attachments")]
    public class Model : Transformable, Nine.IContainer, Nine.IUpdateable, ISpatialQueryable, IPickable, IGeometry, ISupportInstancing, INotifyCollectionChanged<object>
    {
        #region Source
        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the model meshes that made up of this model.
        /// </summary>
        public IList<ModelMesh> Meshes
        {
            get { return modelMeshes; }
        }

        IList Nine.IContainer.Children
        {
            get { return children; }
        }

        private List<ModelMesh> modelMeshes;
        internal List<object> children;

        /// <summary>
        /// Gets or sets the underlying model.
        /// </summary>
        [DependsOn("Meshes")]
        public Microsoft.Xna.Framework.Graphics.Model Source
        {
            get { return source; }
            set
            {
                if (source != value)
                {
                    source = value;
                    UpdateModel();
                }
            }
        }
        private Microsoft.Xna.Framework.Graphics.Model source;
        #endregion
        
        #region Material
        /// <summary>
        /// Gets or sets the material that is applied to the whole model. 
        /// Each model mesh can have its own materials that override this property.
        /// </summary>
        public Material Material
        {
            get { return material; }
            set { material = value; }
        }
        internal Material material;

        /// <summary>
        /// Gets a collection containning all the materials used by this model that are sorted based on level of detail.
        /// </summary>
        public MaterialLevelOfDetail MaterialLevels
        {
            get { return materialLevels; }
            set { materialLevels = value ?? new MaterialLevelOfDetail(); }
        }
        private MaterialLevelOfDetail materialLevels = new MaterialLevelOfDetail();
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Model"/> should be visible.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set { visible = value; }
        }
        internal bool visible;

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
        /// Gets a value indicating whether model animations should only be updated when the model
        /// is visible and inside the view frustum.
        /// </summary>
        public bool AnimationCullingEnabled { get; private set; }

        /// <summary>
        /// Gets or sets whether to use the default model diffuse texture, normal map, specular map, etc.
        /// Other than those specified in the material.
        /// The default behavior is to use the model default textures.
        /// </summary>
        public bool UseModelTextures { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lighting is enabled.
        /// </summary>
        public bool LightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multi-pass lighting is enabled.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MultiPassLightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets the max affecting lights.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int MaxAffectingLights { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model casts shadow.
        /// </summary>
        public bool CastShadow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model casts shadow.
        /// </summary>
        public bool ReceiveShadow { get; set; }

        /// <summary>
        /// Gets or sets the max received shadows.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public int MaxReceivedShadows { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether multi-pass shadowing is enabled.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool MultiPassShadowEnabled { get; set; }
        #endregion

        #region BoundingBox
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get { return boundingBox; }
        }
        private BoundingBox boundingBox;

        /// <summary>
        /// Gets the oriented bounding box.
        /// </summary>
        public BoundingBox OrientedBoundingBox
        {
            get { return orientedBoundingBox; }
        }
        private BoundingBox orientedBoundingBox;

        /// <summary>
        /// Called when transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            boundingBox = orientedBoundingBox.CreateAxisAligned(AbsoluteTransform);
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;
        #endregion

        #region Animation
        /// <summary>
        /// Gets the animations.
        /// </summary>
        [ContentSerializerIgnore]
        public AnimationPlayer Animations 
        {
            get { return animations; }
        }
        private AnimationPlayer animations;

        /// <summary>
        /// Gets the skeleton of this model.
        /// </summary>
        public Skeleton Skeleton
        {
            get { return sharedSkeleton ?? skeleton; }
        }
        private Skeleton skeleton;

        /// <summary>
        /// Gets or sets the shared skeleton.
        /// When a valid shared skeleton is set, the model will be rendered using this shared skeleton.
        /// </summary>
        public Skeleton SharedSkeleton
        {
            get { return sharedSkeleton; }
            set
            {
                if (sharedSkeleton != value)
                {
                    if (value != null && value.BoneTransforms.Length != skeleton.BoneTransforms.Length)
                        throw new InvalidOperationException(Strings.SkeletonMismatch);
                    sharedSkeleton = value;
                    boneTransformNeedUpdate = true;
                }
            }
        }
        private Skeleton sharedSkeleton;

        /// <summary>
        /// Gets a value indicating whether this instance is skinned.
        /// </summary>
        public bool IsSkinned { get; private set; }
        #endregion

        #region Attachments
        /// <summary>
        /// Gets a dictionary of objects that are attached to the specific bone of this model.
        /// </summary>
        public ModelAttachmentCollection Attachments
        {
            get { return attachments ?? (attachments = new ModelAttachmentCollection(this)); }
        }
        private ModelAttachmentCollection attachments;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        public Model(GraphicsDevice graphicsDevice) 
        {
            if (graphicsDevice == null)
                throw new ArgumentNullException("graphicsDevice");

            Visible = true;
            CastShadow = true;
            ReceiveShadow = false;
            LightingEnabled = true;
            UseModelTextures = true;
            MaxAffectingLights = 4;
            MaxReceivedShadows = 1;
            GraphicsDevice = graphicsDevice;
            modelMeshes = new List<ModelMesh>();
            children = new List<object>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        public Model(Microsoft.Xna.Framework.Graphics.Model source, Material material)
#if SILVERLIGHT
            : this(System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice)
#else      
            : this(source.Meshes[0].MeshParts[0].VertexBuffer.GraphicsDevice)
#endif
        {
            if (source == null)
                throw new ArgumentNullException("source");

            this.Source = source;
            this.Material = material;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Model"/> class.
        /// </summary>
        public Model(Microsoft.Xna.Framework.Graphics.Model source) : this(source, null) { }

        /// <summary>
        /// Refresh the internal states when a model changes.
        /// </summary>
        private void UpdateModel()
        {
            skeleton = null;
            positions = null;
            indices = null;
            animations = null;
            orientedBoundingBox = new BoundingBox();
            boundingBox = new BoundingBox();

            if (source != null)
            {
                if (source.Meshes.Count <= 0 || source.Meshes[0].MeshParts.Count <= 0 || source.Meshes[0].MeshParts[0].VertexBuffer == null)
                    throw new ArgumentException("The input model is must have at least 1 valid mesh part.");

                // Initialize bounds
                orientedBoundingBox = source.ComputeBoundingBox();
                boundingBox = orientedBoundingBox.CreateAxisAligned(AbsoluteTransform);

                // Initialize animations
                skeleton = new ModelSkeleton(source);
                IsSkinned = source.IsSkinned();

                var animationNames = source.GetAnimations();
                if (animationNames.Count > 0)
                {
                    animations = new AnimationPlayer();
                    animationNames.ForEach(anim => animations.Animations.Add(anim, new BoneAnimation(Skeleton, source.GetAnimation(anim))));
                    animations.Play();
                }

                // Initialize parts
                var iPart = 0;
                foreach (var mesh in source.Meshes)
                {
                    foreach (var part in mesh.MeshParts)
                    {
                        if (modelMeshes.Count <= iPart)
                            modelMeshes.Add(new ModelMesh());
                        if (modelMeshes[iPart] == null)
                            modelMeshes[iPart] = new ModelMesh();
                        modelMeshes[iPart].Attach(this, mesh, part);
                        iPart++;
                    }
                }

                // Populate child nodes
                children.Clear();
                children.AddRange(modelMeshes);
                if (attachments != null)
                    for (int i = 0; i < attachments.Count; i++)
                        if (attachments[i].Transformable != null)
                            children.Add(attachments[i].Transformable);
            }
        }
        #endregion

        #region BoneTransform
        bool boneTransformNeedUpdate = true;
        
        internal void UpdateBoneTransforms()
        {
            if (source != null && boneTransformNeedUpdate)
            {
                if (BoneTransforms == null || BoneTransforms.Length < Skeleton.BoneTransforms.Length)
                {
                    BoneTransforms = new Matrix[Skeleton.BoneTransforms.Length];
                    Skeleton.CopyAbsoluteBoneTransformsTo(BoneTransforms);
                }
                else if (boneTransformNeedUpdate)
                {
                    Skeleton.CopyAbsoluteBoneTransformsTo(BoneTransforms);
                }

                if (IsSkinned)
                {
                    if (SkinTransforms == null || SkinTransforms.Length < Skeleton.InverseAbsoluteBindPose.Count)
                    {
                        SkinTransforms = new Matrix[Skeleton.InverseAbsoluteBindPose.Count];
                        Skeleton.GetSkinTransforms(SkinTransforms);
                    }
                    else if (boneTransformNeedUpdate)
                    {
                        Skeleton.GetSkinTransforms(SkinTransforms);
                    }
                }

                boneTransformNeedUpdate = false;
            }
        }

        internal Matrix[] SkinTransforms;
        internal Matrix[] BoneTransforms;
        #endregion

        #region Update
        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public void Update(TimeSpan elapsedTime)
        {
            // Skip updating the animation when animation culling is enabled.
            if (animations != null && !(AnimationCullingEnabled && !InsideViewFrustum))
            {
                // Turn off the animation when skeleton is shared
                if ((IsSkinned && sharedSkeleton == null) || !IsSkinned)
                    animations.Update(elapsedTime);
                boneTransformNeedUpdate = true;
            }

            if (attachments != null)
            {
                if (boneTransformNeedUpdate)
                    attachments.UpdateTransforms();
                for (int i = 0; i < attachments.Count; i++)
                {
                    var updateable = attachments[i].Transformable as Nine.IUpdateable;
                    if (updateable != null)
                        updateable.Update(elapsedTime);
                }
            }
            insideViewFrustum = false;
        }
        #endregion

        #region IPickable
        /// <summary>
        /// Gets wether the object contains the given point.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public bool Contains(Vector3 point)
        {
            return source != null && source.Contains(AbsoluteTransform, point);
        }

        /// <summary>
        /// Gets the nearest intersection point from the specifed picking ray.
        /// </summary>
        /// <param name="ray"></param>
        /// <returns>
        /// Distance to the start of the ray.
        /// </returns>
        public float? Intersects(Ray ray)
        {
            if (source == null)
                return null;
            return source.Intersects(AbsoluteTransform, ray);
        }
        #endregion

        #region IGeometry
        Matrix? IGeometry.Transform { get { return AbsoluteTransform; } }

        Vector3[] IGeometry.Positions
        {
            get
            {
#if SILVERLIGHT
                throw new NotSupportedException();
#else
                if (positions == null && source != null)
                {
                    positions = new Vector3[source.CopyPositionsTo(null, 0)];
                    source.CopyPositionsTo(positions, 0);
                }
                return positions;
#endif
            }
        }
        Vector3[] positions;

        ushort[] IGeometry.Indices
        {
            get
            {
#if SILVERLIGHT
                throw new NotSupportedException();
#else
                if (indices == null && source != null)
                {
                    indices = new ushort[source.CopyIndicesTo(null, 0)];
                    source.CopyIndicesTo(indices, 0);
                }
                return indices;
#endif
            }
        }
        ushort[] indices;
        #endregion

        #region ISupportInstancing
        int ISupportInstancing.Count
        {
            get { return Math.Max(modelMeshes.Count, 1); }
        }

        void ISupportInstancing.GetVertexBuffer(int subset, out VertexBuffer vertexBuffer, out int vertexOffset, out int numVertices)
        {
            ModelMesh mesh;
            if (modelMeshes.Count <= 0 || !(mesh = modelMeshes[subset]).visible)
            {
                // If model meshes are not found, it means we are in content build mode.
                vertexBuffer = null;
                vertexOffset = 0;
                numVertices = 0;
                return;
            }

            vertexBuffer = mesh.vertexBuffer;
            vertexOffset = mesh.vertexOffset;
            numVertices = mesh.numVertices;
        }

        void ISupportInstancing.GetIndexBuffer(int subset, out IndexBuffer indexBuffer, out int startIndex, out int primitiveCount)
        {
            ModelMesh mesh;
            if (modelMeshes.Count <= 0 || !(mesh = modelMeshes[subset]).visible)
            {
                indexBuffer = null;
                startIndex = 0;
                primitiveCount = 0;
                return;
            }

            indexBuffer = mesh.indexBuffer;
            startIndex = mesh.startIndex;
            primitiveCount = mesh.primitiveCount;
        }

        Material ISupportInstancing.GetMaterial(int subset)
        {
            if (modelMeshes.Count <= 0)
                return Material;

            // Material Lod is not enabled when using instancing.
            return modelMeshes[subset].Material ?? Material;
        }

        void ISupportInstancing.PrepareMaterial(int subset, Material material)
        {
            if (modelMeshes.Count <= 0)
                return;
            var mesh = modelMeshes[subset];
            UpdateBoneTransforms();
            mesh.ApplyTextures(material);
            mesh.ApplySkinTransform(material);
        }
        #endregion
        
        #region INotifyCollectionChanged
        [EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<object> Added;

        [EditorBrowsable(EditorBrowsableState.Never)]
        public event Action<object> Removed;

        internal void NotifyAdded(object item)
        {
            var added = Added;
            if (added != null)
                added(item);
        }

        internal void NotifyRemoved(object item)
        {
            var removed = Removed;
            if (removed != null)
                removed(item);
        }
        #endregion
    }
}