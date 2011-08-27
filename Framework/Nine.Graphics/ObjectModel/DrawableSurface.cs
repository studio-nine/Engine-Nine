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
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// A triangle mesh constructed from heightmap to represent game surface. 
    /// The up axis of the surface is Vector.UnitZ.
    /// </summary>
    [ContentSerializable]
    public class DrawableSurface : ISpatialQueryable, IDrawableObject, ISurface, IPickable, IDisposable, IEnumerable<DrawableSurfacePatch>
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
        public int PatchSegmentCount { get; private set; }
        
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
        /// Gets or sets the name of the surface.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the current vertex type used by this surface.
        /// </summary>
        public Type VertexType { get; private set; }

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

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }
        #endregion
        
        #region ISpatialQueryable
        /// <summary>
        /// Gets or sets the bottom left position of the surface.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set
            {
                position = value;
                Center = position + new Vector3(Size.X * 0.5f, Size.Y * 0.5f, 0);
                for (int i = 0; i < Patches.Count; i++)
                    Patches[i].UpdatePosition();
                if (BoundingBoxChanged != null)
                    BoundingBoxChanged(this, EventArgs.Empty);
            }
        }

        private Vector3 position;

        /// <summary>
        /// Gets or sets the center position of the surface
        /// </summary>
        public Vector3 Center { get; private set; }

        /// <summary>
        /// Gets the axis aligned bounding box of this surface.
        /// </summary>
        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox box;

                box.Min = boundingBox.Min + position;
                box.Max = boundingBox.Max + position;

                return box;
            }
        }

        private BoundingBox boundingBox;

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;

        object ISpatialQueryable.SpatialData { get; set; }
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
            
            ConvertVertexType<VertexPositionColorNormalTexture>(PopulateVertex);
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

            if (VertexType != typeof(T))
            {
                VertexType = typeof(T);

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
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);

            foreach (DrawableSurfacePatch patch in Patches)
            {
                patch.Invalidate();
            }
        }

        /// <summary>
        /// Populates a single vertex using default settings.
        /// </summary>
        internal void PopulateVertex(int x, int y, ref VertexPositionColorNormalTexture input,  ref VertexPositionColorNormalTexture vertex)
        {
            Vector2 uv = new Vector2();

            // Texture will map to the whole surface by default.
            uv.X = 1.0f * x / SegmentCountX;
            uv.Y = 1.0f * y / SegmentCountY;

            vertex.Color = Color.White;
            vertex.Position = Heightmap.GetPosition(x, y);
            vertex.Normal = Heightmap.Normals[Heightmap.GetIndex(x, y)];
            vertex.TextureCoordinate = Nine.Graphics.TextureTransform.Transform(TextureTransform, uv);
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
            return Position.Z + Heightmap.GetHeight(x - Position.X, y - Position.Y);
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
            return Heightmap.GetNormal(x - Position.X, y - Position.Y);
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
            if (Heightmap.TryGetHeightAndNormal(x - Position.X, y - Position.Y, out baseHeight, out normal))
            {
                height = baseHeight + Position.Z;
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
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void DrawSurfaceVertexConverter<T>(int x, int y, ref VertexPositionColorNormalTexture input, ref T output);
}
