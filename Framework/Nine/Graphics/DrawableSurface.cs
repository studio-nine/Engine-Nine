#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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

namespace Nine.Graphics
{
    /// <summary>
    /// A triangle mesh constructed from heightmap to represent game surface. 
    /// The up axis of the surface is Vector.UnitZ.
    /// </summary>
    public class DrawableSurface : IDisposable, ISurface, IPickable
    {
        /// <summary>
        /// Gets the underlying heightmap that contains height, normal, tangent data.
        /// </summary>
        public Heightmap Heightmap { get; private set; }

        /// <summary>
        /// Gets the effects used to draw the surface.
        /// </summary>
        public DrawableSurfaceEffectCollections Effects { get; private set; }

        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the patches that made up this surface.
        /// </summary>
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
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }

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
        public Vector3 Size { get; private set; }

        /// <summary>
        /// Gets whether this DrawableSurface is freeze.
        /// </summary>
        public bool IsFreezed { get; private set; }

        /// <summary>
        /// Gets or sets the center position of the surface.
        /// </summary>
        public Vector3 Position
        {
            get { return position; }
            set { position = value; UpdatePatchPositions(); }
        }

        private Vector3 position;
        
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
        /// Gets or sets the transform matrix for vertex uv coordinates.
        /// </summary>
        public Matrix TextureTransform { get; set; }

        /// <summary>
        /// Gets the triangles made up of this surface.
        /// </summary>
        public DrawableSurfaceTriangleCollection Triangles { get; private set; }

        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="step">Size of the smallest square block that made up the surface.</param>
        /// <param name="segmentCountX">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="segmentCountY">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
        /// <param name="patchTessellation">Number of the smallest square block that made up the surface patch.</param>
        public DrawableSurface(GraphicsDevice graphics, float step, int segmentCountX, int segmentCountY, int patchTessellation)
            : this(graphics, new Heightmap(step, segmentCountX, segmentCountY), patchTessellation)
        { }

        /// <summary>
        /// Creates a new instance of Surface.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        /// <param name="graphics">Graphics device.</param>
        /// <param name="heightmap">The heightmap geometry to create from.</param>
        /// <param name="patchTessellation">Number of the smallest square block that made up the surface patch.</param>
        public DrawableSurface(GraphicsDevice graphics, Heightmap heightmap, int patchTessellation)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            if (heightmap == null)
                throw new ArgumentNullException("heightmap");

            if (patchTessellation < 2 || patchTessellation % 2 != 0 ||
                heightmap.Width % patchTessellation != 0 ||
                heightmap.Height % patchTessellation != 0)
            {
                throw new ArgumentOutOfRangeException(
                    "patchTessellation must be a even number, " +
                    "segmentCountX/segmentCountY must be a multiplier of patchTessellation.");
            }

            TextureTransform = Matrix.Identity;
            Effects = new DrawableSurfaceEffectCollections(this);
            PatchSegmentCount = patchTessellation;
            GraphicsDevice = graphics;
            Heightmap = heightmap;
            Size = heightmap.Size;
            boundingBox = heightmap.BoundingBox;

            Heightmap.Invalidate += (a, b) => Invalidate();

            // Create patches
            PatchCountX = heightmap.Width / patchTessellation;
            PatchCountY = heightmap.Height / patchTessellation;

            // Store these values in case they change
            SegmentCountX = Heightmap.Width;
            SegmentCountY = Heightmap.Height;

            Triangles = new DrawableSurfaceTriangleCollection();
            Triangles.Surface = this;

            ConvertVertexType<VertexPositionColorNormalTexture>(DefaultFillVertex);
        }

        /// <summary>
        /// Converts and fills the surface vertex buffer to another vertex full.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// This method must be called immediately after the surface is created.
        /// </summary>
        public void ConvertVertexType<T>(DrawSurfaceVertexConverter<T> fillVertex) where T : struct, IVertexType
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freeze");

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
                        DrawableSurfacePatchImpl<T> patch;

                        patch = new DrawableSurfacePatchImpl<T>(this, GraphicsDevice, Heightmap, x, y, PatchSegmentCount);

                        patches[i] = patch;

                        i++;
                    }
                }

                Patches = new DrawableSurfacePatchCollection(this, patches);
            }

            foreach (DrawableSurfacePatchImpl<T> patch in Patches)
                patch.FillVertex = fillVertex;

            Invalidate();
        }

        internal void DefaultFillVertex(int x, int y, ref VertexPositionColorNormalTexture input,  ref VertexPositionColorNormalTexture vertex)
        {
            Vector2 uv = new Vector2();

            uv.X = 1.0f * x / PatchSegmentCount;
            uv.Y = 1.0f * y / PatchSegmentCount;

            vertex.Color = Color.White;
            vertex.Position = Heightmap.GetPosition(x, y);
            vertex.Normal = Heightmap.Normals[Heightmap.GetIndex(x, y)];
            vertex.TextureCoordinate = Nine.Graphics.TextureTransform.Transform(TextureTransform, uv);
        }

        /// <summary>
        /// Update internal vertex buffer, index buffer of this surface.
        /// Call this when you changed the visibility of a triangle.
        /// No need to call this when you reload heightmap using TerrainGeometry.LoadHeightmap.
        /// </summary>
        public void Invalidate()
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            Size = Heightmap.Size;
            boundingBox = Heightmap.BoundingBox;

            foreach (DrawableSurfacePatch patch in Patches)
            {
                patch.Invalidate();
            }
        }

        /// <summary>
        /// Freezes this DrawableSurface to release resources other then rendering.
        /// Freezed DrawableSurface cannot be altered or queried.
        /// This process is not revertible.
        /// </summary>
        public void Freeze()
        {
            if (!IsFreezed)
            {
                IsFreezed = true;
                Heightmap = null;
                foreach (DrawableSurfacePatch patch in Patches)
                {
                    patch.Freeze();
                }
            }
        }

        /// <summary>
        /// Gets the triangle on with the specified position.
        /// </summary>
        internal DrawableSurfaceTriangle GetTriangle(float x, float y)
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            // first we'll figure out where on the heightmap "position" is...
            x -= Position.X;
            y -= Position.Y;

            if (x == Size.X)
                x -= float.Epsilon;
            if (y == Size.Y)
                y -= float.Epsilon;

            // ... and then check to see if that value goes outside the bounds of the
            // heightmap.
            if (!(x >= 0 && x < Size.X && y >= 0 && y < Size.Y))
            {
                return null;
            }

            int pLeft = (int)x / (int)(Size.X / PatchCountX);
            int pTop = (int)y / (int)(Size.Y / PatchCountY);

            int left = (int)x / (int)(Size.X / Heightmap.Width);
            int top = (int)y / (int)(Size.Y / Heightmap.Height);

            left -= pLeft * PatchSegmentCount;
            top -= pTop * PatchSegmentCount;

            int partLeft = left / 2;
            int partTop = top / 2;

            DrawableSurfacePatch patch = Patches[pLeft, pTop];
            DrawableSurfacePatchPart part = patch.PatchParts[partTop * PatchSegmentCount / 2 + partLeft];

            int yy = top - partTop * 2;
            int xx = left - partLeft * 2;

            // NOTE: This is not the accurate position as seen in the rendered geometry.
            //       it is just an approximation.
            return part.Triangles[yy * 4 + xx];
        }
        
        private void UpdatePatchPositions()
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            foreach (DrawableSurfacePatch patch in Patches)
            {
                patch.Position = position;
            }
        }

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

        #region ISurface Members
        public float GetHeight(Vector3 position)
        {
            float height;
            Vector3 normal;
            
            if (TryGetHeightAndNormal(position, out height, out normal))
                return height;

            throw new ArgumentOutOfRangeException("position");
        }

        public Vector3 GetNormal(Vector3 position)
        {
            float height;
            Vector3 normal;

            if (TryGetHeightAndNormal(position, out height, out normal))
                return normal;

            throw new ArgumentOutOfRangeException("position");
        }

        /// <summary>
        /// Gets the height and normal of the surface at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the surface.</returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");
            
            float baseHeight;
            if (Heightmap.TryGetHeightAndNormal(position - Position, out baseHeight, out normal))
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
        /// Points under the heightmap and are within the boundary are picked.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            return BoundingBox.Contains(point) == ContainmentType.Contains;
        }
        
        /// <summary>
        /// Checks whether a ray intersects the surface mesh.
        /// </summary>
        public float? Intersects(Ray ray)
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            float minDistance = float.MaxValue;
            float? distance = null;
            int i = 0;
            Vector3[] vertices = new Vector3[3];                            

            foreach (DrawableSurfacePatch patch in Patches)
            {
                if (ray.Intersects(patch.BoundingBox).HasValue)
                {
                    foreach (DrawableSurfacePatchPart part in patch.PatchParts)
                    {
                        if (ray.Intersects(part.BoundingBox).HasValue)
                        {
                            i = 0;

                            foreach (Point pt in patch.PatchParts[i].GetIndices())
                            {
                                vertices[i++] = patch.GetPosition(part, pt);

                                if (i == 3)
                                {
                                    i = 0;

                                    RayExtensions.Intersects(ray, ref vertices[0],
                                                                  ref vertices[1],
                                                                  ref vertices[2],
                                                                  out distance);

                                    if (distance.HasValue && distance.Value < minDistance)
                                        minDistance = distance.Value;
                                }
                            }                    
                        }
                    }
                }
            }

            if (minDistance < float.MaxValue)
            {
                distance = minDistance;
            }

            return distance;
        }
        #endregion
    }
    
    /// <summary>
    /// Fills a vertex data in a drawable surface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public delegate void DrawSurfaceVertexConverter<T>(int x, int y, ref VertexPositionColorNormalTexture input, ref T output);
}
