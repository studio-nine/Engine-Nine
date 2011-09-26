#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.Effects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// A triangle mesh constructed from heightmap to represent game surface. 
    /// The up axis of the surface is Vector.UnitZ.
    /// </summary>
    [ContentSerializable]
    public class DrawableSurface : Transformable, IDrawableObject, ISurface, IPickable, IDisposable, IEnumerable<DrawableSurfacePatch>
    {
        #region Properties
        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the patches that made up this surface.
        /// </summary>
        [ContentSerializerIgnore]
        public DrawableSurfacePatchCollection Patches { get; private set; }

        /// <summary>
        /// Gets the number of segments of each patch.
        /// </summary>
        [ContentSerializer]
        public int PatchSegmentCount { get; internal set; }
        
        /// <summary>
        /// Gets the count of patches along the x axis.
        /// </summary>
        public int PatchCountX { get; private set; }

        /// <summary>
        /// Gets the count of patches along the y axis.
        /// </summary>
        public int PatchCountY { get; private set; }

        /// <summary>
        /// Gets the number of the smallest square block in X axis, or heightmap texture U axis.
        /// </summary>
        public int SegmentCountX { get; private set; }

        /// <summary>
        /// Gets the number of the smallest square block in Y axis, or heightmap texture V axis.
        /// </summary>
        public int SegmentCountY { get; private set; }

        /// <summary>
        /// Gets the size of the surface geometry in 3 axis.
        /// </summary>
        public Vector3 Size { get { return heightmap.Size; } }
        
        /// <summary>
        /// Gets the size of the smallest square block that made up the terrain.
        /// </summary>
        public float Step { get { return heightmap.Step; } }

        /// <summary>
        /// Gets or sets the transform matrix for vertex uv coordinates.
        /// </summary>
        public Matrix TextureTransform 
        {
            get { return textureTransform; }
            set { textureTransform = value; if (heightmap != null) { Invalidate(); } }
        }
        Matrix textureTransform = Matrix.Identity;

        /// <summary>
        /// Gets the underlying heightmap that contains height, normal, tangent data.
        /// </summary>
        [ContentSerializer]
        public Heightmap Heightmap
        {
            get { return heightmap; }
            internal set
            {
                if (heightmap != value)
                {
                    heightmap = value ?? new Heightmap(1, 32, 32);
                    UpdateHeightmap();
                }
            }
        }
        Heightmap heightmap;
        
        /// <summary>
        /// Gets the current vertex type used by this surface.
        /// </summary>
        [ContentSerializerIgnore]
        public Type VertexType
        {
            get { return vertexType; }
            set
            {
                if (vertexType != value)
                {
                    if (value == null || value == typeof(VertexPositionNormalTexture))
                        ConvertVertexType<VertexPositionNormalTexture>(PopulateVertex);
                    else if (value == typeof(VertexPositionColor))
                        ConvertVertexType<VertexPositionColor>(PopulateVertex);
                    else if (value == typeof(VertexPositionTexture))
                        ConvertVertexType<VertexPositionTexture>(PopulateVertex);
                    else if (value == typeof(VertexPositionNormal))
                        ConvertVertexType<VertexPositionNormal>(PopulateVertex);
                    else if (value == typeof(VertexPositionNormalDualTexture))
                        ConvertVertexType<VertexPositionNormalDualTexture>(PopulateVertex);
                    else if (value == typeof(VertexPositionNormalTangentBinormalTexture))
                        ConvertVertexType<VertexPositionNormalTangentBinormalTexture>(PopulateVertex);
                    else if (value == typeof(VertexPositionColorNormalTexture))
                        ConvertVertexType<VertexPositionColorNormalTexture>(PopulateVertex);
                    else
                        throw new NotSupportedException("Vertex type not supported. Try using DrawableSurface.ConvertVertexType<T> instead.");
                    vertexType = value;
                }
            }
        }
        private Type vertexType = typeof(VertexPositionNormalTexture);

        [ContentSerializer(ElementName = "VertexType")]
        internal string VertexTypeSerializer
        {
            get { return vertexType.AssemblyQualifiedName; }
            set 
            {
                var type = Type.GetType(value) ?? 
                           Type.GetType(string.Format("Nine.Graphics.{0}, Nine.Graphics", value)) ?? 
                           Type.GetType(string.Format("Microsoft.Xna.Framework.Graphics.{0}, Microsoft.Xna.Framework.Graphics", value));
                if (type == null)
                    throw new InvalidOperationException("Unknown vertex type " + value);
                VertexType = type;
            }
        }

        /// <summary>
        /// Gets the max level of detail of this surface.
        /// </summary>
        public int MaxLevelOfDetail { get { return Geometry.MaxLevelOfDetail; } }

        /// <summary>
        /// Gets or sets the distance at which the surface starts to switch to a lower resolution geometry.
        /// </summary>
        /// <value>
        /// The level of detail start.
        /// </value>
        public float LevelOfDetailStart { get; set; }

        /// <summary>
        /// Gets or sets the distance at which the surface has switched to the lowest resolution geometry.
        /// </summary>
        /// <value>
        /// The level of detail end.
        /// </value>
        public float LevelOfDetailEnd { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether level of detail is enabled.
        /// </summary>
        /// <value>
        /// 	<c>true</c> if level of detail is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool LevelOfDetailEnabled
        {
            get { return Geometry.LevelOfDetailEnabled; }
            set { if (value) { Geometry.EnableLevelOfDetail(); } }
        }

        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DrawableSurface"/> is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the material of this drawable surface.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether lighting is enabled.
        /// </summary>
        /// <value>
        ///   <c>true</c> if lighting is enabled; otherwise, <c>false</c>.
        /// </value>
        public bool LightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model casts shadow.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this model casts shadow; otherwise, <c>false</c>.
        /// </value>
        public bool CastShadow { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether this model casts shadow.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this model casts shadow; otherwise, <c>false</c>.
        /// </value>
        public bool ReceiveShadow { get; set; }
        #endregion
        
        #region ISpatialQueryable
        /// <summary>
        /// Gets or sets the local bottom left position of the surface.
        /// </summary>
        [ContentSerializerIgnore]
        public Vector3 Position
        {
            get { return Transform.Translation; }
            set { Transform = Matrix.CreateTranslation(value); }
        }

        /// <summary>
        /// Gets the absolute bottom left position of the surface.
        /// </summary>
        public Vector3 AbsolutePosition
        {
            get { return AbsoluteTransform.Translation; }
        }

        protected override void OnTransformChanged()
        {
            for (int i = 0; i < Patches.Count; i++)
                Patches[i].UpdatePosition();
            base.OnTransformChanged();
        }

        /// <summary>
        /// Gets the axis aligned bounding box of this surface.
        /// </summary>
        public override BoundingBox BoundingBox
        {
            get
            {
                BoundingBox box;

                box.Min = boundingBox.Min + AbsolutePosition;
                box.Max = boundingBox.Max + AbsolutePosition;

                return box;
            }
        }
        private BoundingBox boundingBox;
        #endregion

        #region Members
        /// <summary>
        /// Gets the underlying geometry.
        /// </summary>
        internal DrawableSurfaceGeometry Geometry
        {
            get { return geometry ?? (geometry = DrawableSurfaceGeometry.GetInstance(GraphicsDevice, PatchSegmentCount)); }
        }
        private DrawableSurfaceGeometry geometry;

        /// <summary>
        /// Gets the underlying uniform grid.
        /// </summary>
        internal UniformGrid Grid;
        #endregion

        #region Initialization
        /// <summary>
        /// For content serialization only.
        /// </summary>
        internal DrawableSurface(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
            LevelOfDetailStart = 100;
            LevelOfDetailEnd = 200;
            LightingEnabled = true;
            Visible = true;
            ReceiveShadow = true;
            PatchSegmentCount = 32;
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
        public DrawableSurface(GraphicsDevice graphics, float step, int segmentCountX, int segmentCountY, int patchSegmentCount)
            : this(graphics, new Heightmap(step, segmentCountX, segmentCountY), patchSegmentCount)
        { }
        
        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="heightmap">The heightmap geometry to create from.</param>
        public DrawableSurface(GraphicsDevice graphics, Heightmap heightmap)
            : this(graphics, heightmap, 32)
        { }

        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="heightmap">The heightmap geometry to create from.</param>
        /// <param name="patchSegmentCount">Number of the smallest square block that made up the surface patch.</param>
        public DrawableSurface(GraphicsDevice graphics, Heightmap heightmap, int patchSegmentCount)
            : this(graphics)
        {
            if (heightmap == null)
                throw new ArgumentNullException("heightmap");

            PatchSegmentCount = patchSegmentCount;
            Heightmap = heightmap;
        }

        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="heightmap">The heightmap geometry to create from.</param>
        /// <param name="patchSegmentCount">Number of the smallest square block that made up the surface patch.</param>
        public DrawableSurface(GraphicsDevice graphics, Heightmap heightmap, int patchSegmentCount, Type vertexType)
            : this(graphics)
        {
            if (heightmap == null)
                throw new ArgumentNullException("heightmap");

            PatchSegmentCount = patchSegmentCount;
            Heightmap = heightmap;
        }

        private void UpdateHeightmap()
        {
            if (PatchSegmentCount < 2 || PatchSegmentCount % 2 != 0 ||
                heightmap.Width % PatchSegmentCount != 0 ||
                heightmap.Height % PatchSegmentCount != 0)
            {
                throw new ArgumentOutOfRangeException(
                    "patchTessellation must be a even number, " +
                    "segmentCountX/segmentCountY must be a multiple of patchTessellation.");
            }

            boundingBox = heightmap.BoundingBox;

            Heightmap.Invalidate += (a, b) => Invalidate();

            // Create patches
            PatchCountX = heightmap.Width / PatchSegmentCount;
            PatchCountY = heightmap.Height / PatchSegmentCount;

            // Store these values in case they change
            SegmentCountX = Heightmap.Width;
            SegmentCountY = Heightmap.Height;
            
            ConvertVertexType<VertexPositionNormalTexture>(PopulateVertex);
        }

        /// <summary>
        /// Converts and fills the surface vertex buffer to another vertex full.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// This method must be called immediately after the surface is created.
        /// </summary>
        public void ConvertVertexType<T>(DrawSurfaceVertexConverter<T> fillVertex) where T : struct, IVertexType
        {
            if (fillVertex == null)
                throw new ArgumentNullException("fillVertex");

            if (vertexType != typeof(T) || Patches == null)
            {
                vertexType = typeof(T);

                if (Patches != null)
                {
                    foreach (DrawableSurfacePatch patch in Patches)
                        patch.Dispose();
                }

                DrawableSurfacePatch[] patches = new DrawableSurfacePatch[PatchCountX * PatchCountY];
                
                int i = 0;
                for (int y = 0; y < PatchCountY; y++)
                {
                    for (int x = 0; x < PatchCountX; x++)
                    {
                        patches[i++] = new DrawableSurfacePatch<T>(this, x, y, PatchSegmentCount);
                    }
                }

                Patches = new DrawableSurfacePatchCollection(this, patches);
            }

            foreach (DrawableSurfacePatch<T> patch in Patches)
                patch.FillVertex = fillVertex;

            Invalidate();
        }

        /// <summary>
        /// TODO: Passes dirty rectangle during heightmap invalidation.
        /// </summary>
        private void Invalidate()
        {
            boundingBox = Heightmap.BoundingBox;
            OnBoundingBoxChanged();

            foreach (DrawableSurfacePatch patch in Patches)
            {
                patch.Invalidate();
            }
        }

        /// <summary>
        /// Populates a single vertex using default settings.
        /// </summary>
        internal void PopulateVertex(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionNormalTexture vertex)
        {
            Vector2 uv = new Vector2();

            int xSurface = (x + (xPatch * PatchSegmentCount));
            int ySurface = (y + (yPatch * PatchSegmentCount));

            // Texture will map to the whole surface by default.
            uv.X = 1.0f * xSurface / SegmentCountX;
            uv.Y = 1.0f * ySurface / SegmentCountY;

            vertex.Position = Heightmap.GetPosition(xSurface, ySurface);
            vertex.Normal = Heightmap.GetNormal(xSurface, ySurface);
            vertex.TextureCoordinate = Nine.Graphics.TextureTransform.Transform(TextureTransform, uv);
        }

        private void PopulateVertex(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionColor vertex)
        {
            vertex.Position = input.Position;
            vertex.Color = Color.White;
        }

        private void PopulateVertex(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionNormal vertex)
        {
            vertex.Position = input.Position;
            vertex.Normal = input.Normal;
        }

        private void PopulateVertex(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionTexture vertex)
        {
            vertex.Position = input.Position;
            vertex.TextureCoordinate = input.TextureCoordinate;
        }

        private void PopulateVertex(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionNormalDualTexture vertex)
        {
            vertex.Position = input.Position;
            vertex.Normal = input.Normal;
            vertex.TextureCoordinate = input.TextureCoordinate;
            vertex.TextureCoordinate1 = input.TextureCoordinate;
        }

        private void PopulateVertex(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionNormalTangentBinormalTexture vertex)
        {
            int xSurface = (x + (xPatch * PatchSegmentCount));
            int ySurface = (y + (yPatch * PatchSegmentCount));

            vertex.Position = input.Position;
            vertex.TextureCoordinate = input.TextureCoordinate;
            vertex.Normal = input.Normal;
            vertex.Tangent = Heightmap.GetTangent(xSurface, ySurface);

            Vector3.Cross(ref vertex.Normal, ref vertex.Tangent, out vertex.Binormal);
        }

        private void PopulateVertex(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref VertexPositionColorNormalTexture vertex)
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
            if (!LevelOfDetailEnabled)
                return;

            float start = Math.Min(LevelOfDetailStart, LevelOfDetailEnd);
            float end = Math.Max(LevelOfDetailStart, LevelOfDetailEnd);

            // Ensure the lod difference between adjacent patches does not exceeds 1.
            float distanceBetweenPatches = Step * PatchSegmentCount;
            float lodDistance = Math.Max(end - start, distanceBetweenPatches * MaxLevelOfDetail * 1.414f);

            for (int i = 0; i < Patches.Count; i++)
            {
                float patchDistance = Vector3.Distance(Patches[i].Center, eyePosition);
                
                int patchLod = (int)(MaxLevelOfDetail * (patchDistance - start) / lodDistance);
                if (patchLod < 0)
                    patchLod = 0;
                else if (patchLod > MaxLevelOfDetail)
                    patchLod = MaxLevelOfDetail;
                
                Patches[i].LevelOfDetail = patchLod;
            }

            for (int i = 0; i < Patches.Count; i++)
            {
                Patches[i].UpdateLevelOfDetail();
            }
        }
        #endregion

        #region ISurface Members
        /// <summary>
        /// Gets the height.
        /// </summary>
        public float GetHeight(Vector3 position)
        {
            return GetHeight(position.X, position.Y);
        }

        /// <summary>
        /// Gets the height.
        /// </summary>
        public float GetHeight(float x, float y)
        {
            return AbsolutePosition.Z + Heightmap.GetHeight(x - AbsolutePosition.X, y - AbsolutePosition.Y);
        }

        /// <summary>
        /// Gets the normal.
        /// </summary>
        public Vector3 GetNormal(Vector3 position)
        {
            return GetNormal(position.X, position.Y);
        }

        /// <summary>
        /// Gets the normal.
        /// </summary>
        public Vector3 GetNormal(float x, float y)
        {
            return Heightmap.GetNormal(x - AbsolutePosition.X, y - AbsolutePosition.Y);
        }

        /// <summary>
        /// Gets the height and normal of the surface at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the surface.</returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            return TryGetHeightAndNormal(position.X, position.Y, out height, out normal);
        }

        /// <summary>
        /// Gets the height and normal of the surface at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the surface.</returns>
        public bool TryGetHeightAndNormal(float x, float y, out float height, out Vector3 normal)
        {
            float baseHeight;
            if (Heightmap.TryGetHeightAndNormal(x - AbsolutePosition.X, y - AbsolutePosition.Y, out baseHeight, out normal))
            {
                height = baseHeight + AbsolutePosition.Z;
                return true;
            }

            height = float.MinValue;
            return false;
        }
        #endregion

        #region IPickable Members
        /// <summary>
        /// Determines whether the specified point is above this surface.
        /// </summary>
        /// <param name="point">The point.</param>
        /// <returns>
        ///   <c>true</c> if the specified point is above; otherwise, <c>false</c>.
        /// </returns>
        public bool IsAbove(Vector3 point)
        {
            return GetHeight(point.X, point.Y) < point.Z;
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
            Vector3 start = ray.Position;
            Vector3 pickStep = ray.Direction * Step * 0.5f;

            float? intersection = ray.Intersects(BoundingBox);
            if (intersection.HasValue)
            {
                if (ray.Direction.X == 0 && ray.Direction.Y == 0)
                    return GetHeight(ray.Position.X, ray.Position.Y);

                bool? isAboveLastTime = null;
                start += ray.Direction * (intersection.Value + Step);
                while (true)
                {
                    ContainmentType containment;
                    BoundingBox.Contains(ref start, out containment);
                    if (containment == ContainmentType.Disjoint)
                        break;

                    bool isAbove = (GetHeight(start.X, start.Y) < start.Z);
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
        bool IDrawableObject.Visible { get { return true; } }
        Material IDrawableObject.Material { get { return null; } }

        /// <summary>
        /// We only want to hook to the pre draw event to update level of detail.
        /// </summary>
        void IDrawableObject.BeginDraw(GraphicsContext context)
        {
            UpdateLevelOfDetail(context.EyePosition);
        }

        void IDrawableObject.EndDraw(GraphicsContext context) { }
        void IDrawableObject.Draw(GraphicsContext context) { }
        void IDrawableObject.Draw(GraphicsContext context, Effect effect) { }
        #endregion

        #region IEnumerable
        IEnumerator<DrawableSurfacePatch> IEnumerable<DrawableSurfacePatch>.GetEnumerator()
        {
            return Patches.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Patches.GetEnumerator();
        }
        #endregion

        #region IDisposable Members
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
                foreach (DrawableSurfacePatch patch in Patches)
                {
                    patch.Dispose();
                }
            }
        }

        ~DrawableSurface()
        {
            Dispose(false);
        }
        #endregion
    }

    /// <summary>
    /// Fills a vertex data in a drawable surface.
    /// </summary>
    /// <typeparam name="T">The target vertex type.</typeparam>
    /// <param name="x">The x index of the vertex on the target patch, ranged from 0 to PatchSegmentCount inclusive.</param>
    /// <param name="y">The y index of the vertex on the target patch, ranged from 0 to PatchSegmentCount inclusive.</param>
    /// <param name="xPatch">The x index of the target patch.</param>
    /// <param name="yPatch">The y index of the target patch.</param>
    /// <param name="input">The input vertex contains the default position, normal and texture coordinates for the target vertex.</param>
    /// <param name="output">The output vertex to be set.</param>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void DrawSurfaceVertexConverter<T>(int xPatch, int yPatch, int x, int y, ref VertexPositionNormalTexture input, ref T output);
}
