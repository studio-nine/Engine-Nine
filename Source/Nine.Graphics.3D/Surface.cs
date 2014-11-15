namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Serialization;

    /// <summary>
    /// Defines how the triangles of the surface are organized.
    /// </summary>
    public enum SurfaceTopology
    {
        /// <summary>
        /// Triangle with a right angle at the (-i,-j) position and another at the (+i,+j) position.
        /// The topology of adjacent quads are mirrored.
        /// </summary>
        BottomLeftUpperRightCrossed,

        /// <summary>
        /// Triangle with a right angle at the (+i,-j) position and another at the high (-i,+j) position.
        /// /// The topology of adjacent quads are mirrored.
        /// </summary>
        BottomRightUpperLeftCrossed,

        /// <summary>
        /// Triangle with a right angle at the (-i,-j) position and another at the (+i,+j) position.
        /// </summary>
        BottomLeftUpperRight,

        /// <summary>
        /// Triangle with a right angle at the (+i,-j) position and another at the high (-i,+j) position.
        /// </summary>
        BottomRightUpperLeft
    }

    /// <summary>
    /// A triangle mesh constructed from heightmap to represent game surface. 
    /// The up axis of the surface is Vector.UnitY.
    /// </summary>
    [BinarySerializable]
    [ContentProperty("Material")]
    public class Surface : Transformable, Nine.IContainer, INotifyCollectionChanged<object>
                         , IDrawableObject, ISurface, IPickable, ISupportInitialize, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the patches that made up this surface.
        /// </summary>
        [NotBinarySerializable]
        public SurfacePatchCollection Patches { get { return patches; } }
        private SurfacePatchCollection patches;
        
        /// <summary>
        /// Gets the count of patches along the x axis.
        /// </summary>
        public int PatchCountX { get { return patchCountX; } }
        internal int patchCountX;

        /// <summary>
        /// Gets the count of patches along the z axis.
        /// </summary>
        public int PatchCountZ { get { return patchCountZ; } }
        internal int patchCountZ;

        /// <summary>
        /// Gets the number of the smallest square block in X axis, or heightmap texture U axis.
        /// </summary>
        public int SegmentCountX { get { return segmentCountX; } }
        private int segmentCountX;

        /// <summary>
        /// Gets the number of the smallest square block in Z axis, or heightmap texture V axis.
        /// </summary>
        public int SegmentCountZ { get { return segmentCountZ; } }
        private int segmentCountZ;
        
        /// <summary>
        /// Gets the step of the surface heightmap.
        /// </summary>
        public float Step { get { return step; } }
        private float step;

        /// <summary>
        /// Gets the number of segments of each patch.
        /// </summary>
        public int PatchSegmentCount
        {
            get { return patchSegmentCount; }
            set { if (patchSegmentCount != value) { patchSegmentCount = value; EnsureInitialized(); } }
        }
        private int patchSegmentCount = 32;

        /// <summary>
        /// Gets or sets the transform matrix for vertex uv coordinates.
        /// </summary>
        public Matrix TextureTransform 
        {
            get { return textureTransform; }
            set { textureTransform = value; EnsureInitialized(); }
        }
        private Matrix textureTransform = Matrix.Identity;

        /// <summary>
        /// Gets or sets the topology of the surface triangles.
        /// </summary>
        public SurfaceTopology Topology
        {
            get { return topology; }
            set { if (topology != value) { topology = value; EnsureInitialized(); } }
        }
        private SurfaceTopology topology;

        /// <summary>
        /// Gets the current vertex type used by this surface.
        /// </summary>
#if !MonoGame
        [TypeConverter(typeof(Nine.Design.SystemTypeConverter))]
#endif
        public Type VertexType
        {
            get { return vertexType; }
            set
            {
                value = value ?? typeof(VertexPositionNormalTexture);
                if (vertexType != value)
                {
                    vertexType = value;
                    EnsureInitialized();
                }
            }
        }
        private Type vertexType = typeof(VertexPositionNormalTexture);

        /// <summary>
        /// Gets the underlying heightmap that contains height, normal, tangent data.
        /// </summary>
        public IHeightmap Heightmap
        {
            get { return heightmap; }
            set
            {
                if (heightmap != value)
                {
                    if (heightmap != null)
                        heightmap.HeightmapChanged -= OnHeightmapHeightChanged;
                    heightmap = value;
                    if (heightmap != null)
                        heightmap.HeightmapChanged += OnHeightmapHeightChanged;
                    EnsureInitialized();
                }
            }
        }
        internal IHeightmap heightmap;

        /// <summary>
        /// Gets or sets the distance at which the surface starts to switch to a lower resolution geometry.
        /// </summary>
        public float LevelOfDetailStart { get; set; }

        /// <summary>
        /// Gets or sets the distance at which the surface has switched to the lowest resolution geometry.
        /// </summary>
        public float LevelOfDetailEnd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether level of detail is enabled.
        /// </summary>
        public bool LevelOfDetailEnabled
        {
            get { return lodEnabled; }
            set { if (lodEnabled != value) { lodEnabled = value; EnsureInitialized(); } }
        }
        private bool lodEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="Surface"/> is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the material of this drawable surface.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets a collection containning all the materials that are sorted based on level of detail.
        /// </summary>
        public MaterialLevelOfDetail MaterialLevels
        {
            get { return materialLevels; }
            set { materialLevels = value ?? new MaterialLevelOfDetail(); }
        }
        private MaterialLevelOfDetail materialLevels = new MaterialLevelOfDetail();

        /// <summary>
        /// Gets or sets a value indicating whether this object casts shadow.
        /// </summary>
        public bool CastShadow { get; set; }

        /// <summary>
        /// Gets the underlying geometry.
        /// </summary>
        internal SurfaceGeometry Geometry;
        #endregion
        
        #region ISpatialQueryable
        /// <summary>
        /// Gets or sets the local bottom left position of the surface.
        /// </summary>
        [NotBinarySerializable]
        public Vector3 Position
        {
            get { return transform.Translation; }
            set
            {
                var transform = this.transform;
                transform.M41 = value.X;
                transform.M42 = value.Y;
                transform.M43 = value.Z;
                Transform = transform;
            }
        }

        /// <summary>
        /// Gets the absolute bottom left position of the surface.
        /// </summary>
        public Vector3 AbsolutePosition { get { return absolutePosition; } }
        internal Vector3 absolutePosition;

        /// <summary>
        /// Gets the local center position of the surface.
        /// </summary>
        public Vector3 Center 
        {
            get { return new Vector3(transform.M41 + size.X * 0.5f, transform.M42, transform.M43 + size.Z * 0.5f); } 
        }

        /// <summary>
        /// Gets the absolute center position of the surface.
        /// </summary>
        public Vector3 AbsoluteCenter
        {
            get { return new Vector3(absolutePosition.X + size.X * 0.5f, absolutePosition.Y, absolutePosition.Z + size.Z * 0.5f); }
        }

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            absolutePosition = AbsoluteTransform.Translation;

            worldBounds.Min = localBounds.Min + absolutePosition - boundingBoxPadding;
            worldBounds.Max = localBounds.Max + absolutePosition + boundingBoxPadding;

            if (patches != null)
                for (int i = 0; i < patches.Count; ++i)
                    patches[i].UpdatePosition();
        }

        /// <summary>
        /// Gets the axis aligned bounding box of this surface.
        /// </summary>
        public BoundingBox BoundingBox { get { return worldBounds; } }
        private BoundingBox worldBounds;
        private BoundingBox localBounds;
        private Vector3 size;
        
        /// <summary>
        /// Gets or sets a value that is appended to the computed bounding box
        /// of each surface patch.
        /// </summary>
        public Vector3 BoundingBoxPadding
        {
            get { return boundingBoxPadding; }
            set { boundingBoxPadding = value; OnTransformChanged(); }
        }
        internal Vector3 boundingBoxPadding;
        #endregion

        #region Initialization
        /// <summary>
        /// Initializes a new instance of the <see cref="Surface"/> class.
        /// </summary>
        public Surface(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
            LevelOfDetailStart = 100;
            LevelOfDetailEnd = 1000;
            Visible = true;
        }

        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="step">Size of the smallest square block that made up the surface.</param>
        /// <param name="segmentCountX">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="segmentCountY">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
        /// <param name="patchSegmentCount">Number of the smallest square block that made up the surface patch.</param>
        public Surface(GraphicsDevice graphics, float step, int segmentCountX, int segmentCountY, int patchSegmentCount)
            : this(graphics, new FlatHeightmap { Step = step, Width = segmentCountX, Height = segmentCountY }, patchSegmentCount)
        { }
        
        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="heightmap">The heightmap geometry to create from.</param>
        public Surface(GraphicsDevice graphics, IHeightmap heightmap)
            : this(graphics, heightmap, 32)
        { }

        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="heightmap">The heightmap geometry to create from.</param>
        /// <param name="patchSegmentCount">Number of the smallest square block that made up the surface patch.</param>
        public Surface(GraphicsDevice graphics, IHeightmap heightmap, int patchSegmentCount)
            : this(graphics, heightmap, patchSegmentCount, null)
        { }

        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="heightmap">The heightmap geometry to create from.</param>
        /// <param name="patchSegmentCount">Number of the smallest square block that made up the surface patch.</param>
        public Surface(GraphicsDevice graphics, IHeightmap heightmap, int patchSegmentCount, Type vertexType)
            : this(graphics)
        {
            var supportInitialize = (ISupportInitialize)this;
            supportInitialize.BeginInit();
            {
                PatchSegmentCount = patchSegmentCount;
                VertexType = VertexType;
                Heightmap = heightmap;
            }
            supportInitialize.EndInit();
        }

        /// <summary>
        /// This method is called either when the surface is batch initialized using ISupportInitialize interface
        /// or when a property has modified that needs the surface to be re-initialized.
        /// </summary>
        private void OnInitialized()
        {
            var removedPatches = patches;
            if (heightmap == null)
            {
                localBounds = new BoundingBox();
                patchCountX = patchCountZ = 0;
                segmentCountX = segmentCountZ = 0;
                patches = null;

                if (removed != null && removedPatches != null)
                    foreach (var patch in removedPatches)
                        removed(patch);
                return;
            }

            if (patchSegmentCount < 2 || patchSegmentCount % 2 != 0 ||
                heightmap.Width % patchSegmentCount != 0 ||
                heightmap.Height % patchSegmentCount != 0)
            {
                throw new ArgumentOutOfRangeException(
                    "patchSegmentCount must be a even number, " +
                    "segmentCountX/segmentCountY must be a multiple of patchSegmentCount.");
            }
            
            // Create patches
            var newPatchCountX = heightmap.Width / patchSegmentCount;
            var newPatchCountZ = heightmap.Height / patchSegmentCount;

            // Invalid patches when patch count has changed
            if (newPatchCountX != patchCountX || newPatchCountZ != patchCountZ)
            {
                patches = null;
                if (removed != null && removedPatches != null)
                    foreach (var patch in removedPatches)
                        removed(patch);
            }

            patchCountX = newPatchCountX;
            patchCountZ = newPatchCountZ;

            // Store these values in case they change
            segmentCountX = heightmap.Width;
            segmentCountZ = heightmap.Height;

            step = heightmap.Step;
            size = new Vector3(heightmap.Width * step, 0, heightmap.Height * step);

            // Initialize geometry
            Geometry = SurfaceGeometry.GetInstance(GraphicsDevice, patchSegmentCount, topology);
            if (lodEnabled)
                Geometry.EnableLevelOfDetail();

            // Convert vertex type
            if (vertexType == null || vertexType == typeof(VertexPositionNormalTexture))
                ConvertVertexType<VertexPositionNormalTexture>(PopulateVertex);
            else if (vertexType == typeof(VertexPositionColor))
                ConvertVertexType<VertexPositionColor>(PopulateVertex);
            else if (vertexType == typeof(VertexPositionTexture))
                ConvertVertexType<VertexPositionTexture>(PopulateVertex);
            else if (vertexType == typeof(VertexPositionNormal))
                ConvertVertexType<VertexPositionNormal>(PopulateVertex);
            else if (vertexType == typeof(VertexPositionNormalDualTexture))
                ConvertVertexType<VertexPositionNormalDualTexture>(PopulateVertex);
            else if (vertexType == typeof(VertexPositionNormalTangentBinormalTexture))
                ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(PopulateVertex);
            else if (vertexType == typeof(VertexPositionColorNormalTexture))
                ConvertVertexType<VertexPositionColorNormalTexture>(PopulateVertex);
            else
                throw new NotSupportedException("Vertex type not supported. Try using Surface.ConvertVertexType<T> instead.");
        }

        /// <summary>
        /// Converts and fills the surface vertex buffer to another vertex full.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// This method must be called immediately after the surface is created.
        /// </summary>
        public void ConvertVertexType<T>(SurfaceVertexConverter<T> fillVertex) where T : struct, IVertexType
        {
            if (fillVertex == null)
                throw new ArgumentNullException("fillVertex");

            if (vertexType != typeof(T) || patches == null)
            {
                vertexType = typeof(T);
                if (patches != null)
                {
                    foreach (SurfacePatch patch in patches)
                    {
                        if (removed != null)
                            removed(patch);
                        patch.Dispose();
                    }
                }

                var i = 0;
                var patchArray = new SurfacePatch[patchCountX * patchCountZ];

                for (int z = 0; z < patchCountZ; z++)
                    for (int x = 0; x < patchCountX; ++x)
                        patchArray[i++] = new SurfacePatch<T>(this, x, z);
                patches = new SurfacePatchCollection(this, patchArray);

                if (added != null)
                    foreach (SurfacePatch patch in patches)
                        added(patch);
            }

            foreach (SurfacePatch<T> patch in patches)
                patch.FillVertex = fillVertex;

            Invalidate(null);
        }

        private void OnHeightmapHeightChanged(Rectangle? dirtyRegion)
        {
            Invalidate(dirtyRegion);
        }

        private void Invalidate(Rectangle? dirtyRegion)
        {
            if (heightmap != null)
            {
                localBounds.Min.X = localBounds.Min.Y = localBounds.Min.Z = float.MaxValue;
                localBounds.Max.X = localBounds.Max.Y = localBounds.Max.Z = float.MinValue;

                if (patches != null)
                {
                    foreach (SurfacePatch patch in patches)
                    {
                        patch.Invalidate();

                        if (localBounds.Min.X > patch.localBounds.Min.X)
                            localBounds.Min.X = patch.localBounds.Min.X;
                        if (localBounds.Min.Y > patch.localBounds.Min.Y)
                            localBounds.Min.Y = patch.localBounds.Min.Y;
                        if (localBounds.Min.Z > patch.localBounds.Min.Z)
                            localBounds.Min.Z = patch.localBounds.Min.Z;

                        if (localBounds.Max.X < patch.localBounds.Max.X)
                            localBounds.Max.X = patch.localBounds.Max.X;
                        if (localBounds.Max.Y < patch.localBounds.Max.Y)
                            localBounds.Max.Y = patch.localBounds.Max.Y;
                        if (localBounds.Max.Z < patch.localBounds.Max.Z)
                            localBounds.Max.Z = patch.localBounds.Max.Z;
                    }
                }

                OnTransformChanged();
            }
        }

        /// <summary>
        /// Populates a single vertex using default settings.
        /// </summary>
        internal void PopulateVertex(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref VertexPositionNormalTexture vertex)
        {
            Vector2 uv = new Vector2();

            int xSurface = (x + (xPatch * patchSegmentCount));
            int zSurface = (z + (zPatch * patchSegmentCount));

            // Texture will map to the whole surface by default.
            uv.X = 1.0f * xSurface / segmentCountX;
            uv.Y = 1.0f * zSurface / segmentCountZ;

            vertex.Position = new Vector3(xSurface * step, heightmap.GetHeight(xSurface, zSurface), zSurface * step);
            vertex.Normal = heightmap.GetNormal(xSurface, zSurface);
            vertex.TextureCoordinate = Nine.Graphics.TextureTransform.Transform(textureTransform, uv);
        }

        private void PopulateVertex(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref VertexPositionColor vertex)
        {
            vertex.Position = input.Position;
            vertex.Color = Color.White;
        }

        private void PopulateVertex(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref VertexPositionNormal vertex)
        {
            vertex.Position = input.Position;
            vertex.Normal = input.Normal;
        }

        private void PopulateVertex(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref VertexPositionTexture vertex)
        {
            vertex.Position = input.Position;
            vertex.TextureCoordinate = input.TextureCoordinate;
        }

        private void PopulateVertex(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref VertexPositionNormalDualTexture vertex)
        {
            vertex.Position = input.Position;
            vertex.Normal = input.Normal;
            vertex.TextureCoordinate = input.TextureCoordinate;
            vertex.TextureCoordinate1 = input.TextureCoordinate;
        }

        private void PopulateVertex(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref VertexPositionNormalTangentBinormalTexture vertex)
        {
            int xSurface = (x + (xPatch * patchSegmentCount));
            int ySurface = (z + (zPatch * patchSegmentCount));

            vertex.Position = input.Position;
            vertex.TextureCoordinate = input.TextureCoordinate;
            vertex.Normal = input.Normal;
            vertex.Tangent = Heightmap.GetTangent(xSurface, ySurface);

            Vector3.Cross(ref vertex.Normal, ref vertex.Tangent, out vertex.Binormal);
        }

        private void PopulateVertex(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref VertexPositionColorNormalTexture vertex)
        {
            vertex.Position = input.Position;
            vertex.Normal = input.Normal;
            vertex.Color = Color.White;
            vertex.TextureCoordinate = input.TextureCoordinate;
        }
        #endregion

        #region Methods
        /// <summary>
        /// Manually updates the level of detail of each surface patch.
        /// </summary>
        /// <param name="eyePosition">The eye position.</param>
        /// <remarks>
        /// If you are draw the surface using scene, level of details are automatically updated.
        /// </remarks>
        public void UpdateLevelOfDetail(Vector3 eyePosition)
        {
            if (!LevelOfDetailEnabled || heightmap == null)
                return;

            var start = Math.Min(LevelOfDetailStart, LevelOfDetailEnd);
            var end = Math.Max(LevelOfDetailStart, LevelOfDetailEnd);

            // Ensure the lod difference between adjacent patches does not exceeds 1.
            var maxDetailLevels = Geometry.MaxLevelOfDetail;
            var distanceBetweenPatches = heightmap.Step * patchSegmentCount;
            var lodDistance = Math.Max(end - start, distanceBetweenPatches * maxDetailLevels * 1.414f);

            float patchDistance;

            for (int i = 0; i < patches.Count; ++i)
            {
                Vector3.Distance(ref patches[i].center, ref eyePosition, out patchDistance);

                int patchLod = (int)(maxDetailLevels * (patchDistance - start) / lodDistance);
                if (patchLod < 0)
                    patchLod = 0;
                else if (patchLod > maxDetailLevels)
                    patchLod = maxDetailLevels;
                
                patches[i].DetailLevel = patchLod;
            }

            for (int i = 0; i < patches.Count; ++i)
            {
                patches[i].UpdateLevelOfDetail();
            }
        }
        #endregion

        #region ISurface Members
        /// <summary>
        /// Gets the height of the terrain at a given location.
        /// </summary>
        /// <returns>NaN if the location is outside the boundary of the terrain.</returns>
        public float GetHeight(float x, float z)
        {
            var height = 0.0f;
            var normal = new Vector3();
            var absolutePosition = AbsolutePosition;
            if (TryGetHeightAndNormal(x - absolutePosition.X, z - absolutePosition.Z, true, false, out height, out normal))
                return height + absolutePosition.Y;

            return float.NaN;
        }

        /// <summary>
        /// Gets the normal of the terrain at a given location.
        /// </summary>
        /// <returns>NaN if the location is outside the boundary of the terrain.</returns>
        public Vector3 GetNormal(float x, float z)
        {
            var height = 0.0f;
            var normal = new Vector3();
            var absolutePosition = AbsolutePosition;
            if (TryGetHeightAndNormal(x - absolutePosition.X, z - absolutePosition.Z, false, true, out height, out normal))
                return normal;

            return new Vector3(float.NaN, float.NaN, float.NaN);
        }

        /// <summary>
        /// Gets the height and normal of the terrain at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the terrain.</returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            return TryGetHeightAndNormal(position.X, position.Z, out height, out normal);
        }

        /// <summary>
        /// Gets the height and normal of the terrain at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the terrain.</returns>
        public bool TryGetHeightAndNormal(float x, float z, out float height, out Vector3 normal)
        {
            var absolutePosition = AbsolutePosition;
            if (TryGetHeightAndNormal(x - absolutePosition.X, z - absolutePosition.Z, true, true, out height, out normal))
            {
                height += absolutePosition.Y;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Gets the height and normal of a given point in heightmap local space.
        /// </summary>
        private bool TryGetHeightAndNormal(float x, float z, bool getHeight, bool getNormal, out float height, out Vector3 normal)
        {
            // first we'll figure out where on the heightmap "position" is...
            if (x == size.X)
                x -= float.Epsilon;
            if (z == size.Z)
                z -= float.Epsilon;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            if (heightmap == null || !(x >= 0 && x < size.X && z >= 0 && z < size.Z))
            {
                height = float.MinValue;
                normal = Vector3.Up;
                return false;
            }

            // we'll use integer division to figure out where in the "heights" array
            // positionOnHeightmap is. Remember that integer division always rounds
            // down, so that the result of these divisions is the indices of the "upper
            // left" of the 4 corners of that cell.
            var left = (int)Math.Floor(x / step);
            var top = (int)Math.Floor(z / step);

            // next, we'll use modulus to find out how far away we are from the upper
            // left corner of the cell. Mod will give us a value from 0 to terrainScale,
            // which we then divide by terrainScale to normalize 0 to 1.
            var xNormalized = (x - left * step) / step;
            var zNormalized = (z - top * step) / step;

            if (getHeight)
            {
                // Now that we've calculated the indices of the corners of our cell, and
                // where we are in that cell, we'll use bilinear interpolation to calculate
                // our height. This process is best explained with a diagram, so please see
                // the accompanying doc for more information.
                // First, calculate the heights on the bottom and top edge of our cell by
                // interpolating from the left and right sides.
                float topHeight = MathHelper.Lerp(
                    heightmap.GetHeight(left, top), heightmap.GetHeight(left + 1, top), xNormalized);
                float bottomHeight = MathHelper.Lerp(
                    heightmap.GetHeight(left, top + 1), heightmap.GetHeight(left + 1, top + 1), xNormalized);

                // next, interpolate between those two values to calculate the height at our
                // position.
                height = MathHelper.Lerp(topHeight, bottomHeight, zNormalized);
            }
            else
            {
                height = 0;
            }

            if (getNormal)
            {
                // We'll repeat the same process to calculate the normal.
                Vector3 topNormal = Vector3.Lerp(
                    heightmap.GetNormal(left, top), heightmap.GetNormal(left + 1, top), xNormalized);
                Vector3 bottomNormal = Vector3.Lerp(
                    heightmap.GetNormal(left, top + 1), heightmap.GetNormal(left + 1, top + 1), xNormalized);

                Vector3.Lerp(ref topNormal, ref bottomNormal, zNormalized, out normal);                
                normal.Normalize();
            }
            else
            {
                normal = Vector3.Up;
            }
            return true;
        }
        #endregion

        #region IPickable Members
        /// <summary>
        /// Determines whether the specified point is above this surface.
        /// </summary>
        public bool IsAbove(Vector3 point)
        {
            return GetHeight(point.X, point.Z) <= point.Y;
        }

        /// <summary>
        /// Points under the heightmap and are within the boundary are picked.
        /// </summary>
        bool IPickable.Contains(Vector3 point)
        {
            return BoundingBox.Contains(point) == ContainmentType.Contains;
        }
        
        /// <summary>
        /// Checks whether a ray intersects the surface mesh.
        /// </summary>
        public float? Intersects(Ray ray)
        {
            float height;
            Vector3 start = ray.Position;
            Vector3 pickStep = ray.Direction * heightmap.Step * 0.5f;

            float? intersection = ray.Intersects(BoundingBox);
            if (intersection.HasValue)
            {
                if (ray.Direction.X == 0 && ray.Direction.Z == 0)
                {
                    if (!float.IsNaN(height = GetHeight(ray.Position.X, ray.Position.Z)))
                    {
                        float distance = ray.Position.Y - height;
                        if (distance * ray.Direction.Y < 0)
                            return distance;
                    }
                    return null;
                }

                bool? isAboveLastTime = null;
                start += ray.Direction * intersection.Value;
                while (true)
                {
                    if (float.IsNaN(height = GetHeight(start.X, start.Z)))
                        return null;

                    bool isAbove = (height <= start.Y);
                    if (isAboveLastTime == null)
                    {
                        if (!isAbove)
                            break;
                        isAboveLastTime = isAbove;
                    }
                    else if (isAboveLastTime.Value && !isAbove)
                    {
                        float distance;
                        Vector3.Multiply(ref pickStep, -0.5f, out pickStep);
                        Vector3.Subtract(ref start, ref pickStep, out start);
                        Vector3.Distance(ref ray.Position, ref start, out distance);
                        return distance;
                    }
                    Vector3.Add(ref start, ref pickStep, out start);
                }
            }
            return null;
        }
        #endregion

        #region IDrawable
        Material IDrawableObject.Material { get { return null; } }

        /// <summary>
        /// We only want to hook to the pre draw event to update level of detail.
        /// </summary>
        bool IDrawableObject.OnAddedToView(DrawingContext context)
        {
            UpdateLevelOfDetail(context.CameraPosition);
            return false;
        }

        void IDrawableObject.Draw(DrawingContext context, Material material) 
        {

        }

        float IDrawableObject.GetDistanceToCamera(ref Vector3 cameraPosition)
        {
            return 0; 
        }
        #endregion

        #region IContainer
        IList Nine.IContainer.Children
        {
            get { return patches; }
        }
        #endregion

        #region INotifyCollectionChanged
        private Action<object> added;
        private Action<object> removed;
        
        event Action<object> INotifyCollectionChanged<object>.Added
        {
            add { added += value; }
            remove { added -= value; }
        }

        event Action<object> INotifyCollectionChanged<object>.Removed
        {
            add { removed += value; }
            remove { removed -= value; }
        }
        #endregion

        #region ISupportInitialize
        void ISupportInitialize.BeginInit() { initializing = true; }
        void ISupportInitialize.EndInit() { if (initializing) { initializing = false; OnInitialized(); } }
        void EnsureInitialized() { if (!initializing) OnInitialized(); }
        private bool initializing = false;
        #endregion

        #region IDisposable
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
                if (patches != null)
                    foreach (SurfacePatch patch in patches)
                        patch.Dispose();
            }
        }
        #endregion
    }

    /// <summary>
    /// Fills a vertex data in a drawable surface.
    /// </summary>
    /// <typeparam name="T">The target vertex type.</typeparam>
    /// <param name="x">The x index of the vertex on the target patch, ranged from 0 to PatchSegmentCount inclusive.</param>
    /// <param name="z">The z index of the vertex on the target patch, ranged from 0 to PatchSegmentCount inclusive.</param>
    /// <param name="xPatch">The x index of the target patch.</param>
    /// <param name="zPatch">The z index of the target patch.</param>
    /// <param name="input">The input vertex contains the default position, normal and texture coordinates for the target vertex.</param>
    /// <param name="output">The output vertex to be set.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void SurfaceVertexConverter<T>(int xPatch, int zPatch, int x, int z, ref VertexPositionNormalTexture input, ref T output);
}
