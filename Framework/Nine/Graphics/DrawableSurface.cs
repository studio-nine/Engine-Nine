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
        public DrawableSurfaceCollections Effects { get; private set; }

        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the patches that made up this surface.
        /// </summary>
        public DrawableSurfacePatchCollection Patches { get; private set; }

        /// <summary>
        /// Gets the number of tessellation of each patch.
        /// </summary>
        public int PatchTessellation { get; private set; }
        
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
        public int TessellationX { get; private set; }

        /// <summary>
        /// Gets the number of the smallest square block in Y axis, or heightmap texture V axis.
        /// </summary>
        public int TessellationY { get; private set; }

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
        /// Gets whether this DrawableSurface is freezed.
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
        /// <param name="tessellationU">Number of the smallest square block in X axis, or heightmap texture U axis.</param>
        /// <param name="tessellationV">Number of the smallest square block in Y axis, or heightmap texture V axis.</param>
        /// <param name="patchTessellation">Number of the smallest square block that made up the surface patch.</param>
        public DrawableSurface(GraphicsDevice graphics, float step, int tessellationU, int tessellationV, int patchTessellation)
            : this(graphics, new Heightmap(step, tessellationU, tessellationV), patchTessellation)
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
                heightmap.TessellationU % patchTessellation != 0 ||
                heightmap.TessellationV % patchTessellation != 0)
            {
                throw new ArgumentOutOfRangeException(
                    "patchTessellation must be a even number, " +
                    "tessellationU/tessellationV must be a multiplier of patchTessellation.");
            }

            TextureTransform = Matrix.Identity;
            Effects = new DrawableSurfaceCollections(this);
            PatchTessellation = patchTessellation;
            GraphicsDevice = graphics;
            Heightmap = heightmap;
            Size = heightmap.Size;
            boundingBox = heightmap.BoundingBox;

            Heightmap.Invalidate += (a, b) => Invalidate();

            // Create patches
            PatchCountX = heightmap.TessellationU / patchTessellation;
            PatchCountY = heightmap.TessellationV / patchTessellation;

            // Store these values in case they change
            TessellationX = Heightmap.TessellationU;
            TessellationY = Heightmap.TessellationV;

            Triangles = new DrawableSurfaceTriangleCollection();
            Triangles.Surface = this;

            ConvertVertexType<VertexPositionColorNormalTexture>(DefaultFillVertex);
        }

        /// <summary>
        /// Converts and fills the surface vertex buffer to another vertex full.
        /// The default vertex format is VertexPositionColorNormalTexture.
        /// </summary>
        public void ConvertVertexType<T>(DrawSurfaceFillVertex<T> fillVertex) where T : struct, IVertexType
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

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

                        patch = new DrawableSurfacePatchImpl<T>(this, GraphicsDevice, Heightmap, x, y, PatchTessellation);

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

        private void DefaultFillVertex(int x, int y, ref VertexPositionColorNormalTexture vertex)
        {
            Vector2 uv = new Vector2();

            uv.X = 1.0f * x / PatchTessellation;
            uv.Y = 1.0f * y / PatchTessellation;

            vertex.Color = Color.White;
            vertex.Position = Heightmap.GetPosition(x, y);
            vertex.Normal = Heightmap.Normals[Heightmap.GetIndex(x, y)];
            vertex.TextureCoordinate = Nine.Graphics.TextureTransform.Transform(TextureTransform, uv);
        }

        /// <summary>
        /// Update internal vertex buffer, index buffer of this surface.
        /// Call this when you changed the visiblity of a triangle.
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
        /// This process is not invertable.
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

        internal DrawableSurfaceTriangle GetTriangle(float x, float y)
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            // first we'll figure out where on the heightmap "position" is...
            x -= Position.X;
            y -= Position.Y;
            x += Size.X / 2;
            y += Size.Y / 2;

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

            int left = (int)x / (int)(Size.X / Heightmap.TessellationU);
            int top = (int)y / (int)(Size.Y / Heightmap.TessellationV);

            float tx = x - (Size.X / Heightmap.TessellationU) * left;
            float ty = y - (Size.Y / Heightmap.TessellationV) * top;

            left -= pLeft * PatchTessellation;
            top -= pTop * PatchTessellation;

            int partLeft = left / 2;
            int partTop = top / 2;

            DrawableSurfacePatch patch = Patches[pLeft, pTop];
            DrawableSurfacePatchPart part = patch.PatchParts[partTop * PatchTessellation / 2 + partLeft];

            int yy = top - partTop * 2;
            int xx = left - partLeft * 2;

            // FIXME: This is incorrect
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
            foreach (DrawableSurfacePatch patch in Patches)
            {
                patch.Dispose();
            }
        }
        #endregion

        #region ISurface Members
        /// <summary>
        /// Gets the height and normal of the surface at a given location.
        /// </summary>
        /// <returns>False if the location is outside the boundary of the surface.</returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            return Heightmap.TryGetHeightAndNormal(position - Position, out height, out normal);
        }
        #endregion

        #region IPickable Members
        /// <summary>
        /// Points under the heightmap and are within the boundary are picked.
        /// </summary>
        public bool Contains(Vector3 point)
        {
            if (IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            return Heightmap.Pick(point - position);
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
    public delegate void DrawSurfaceFillVertex<T>(int x, int y, ref T vertex);
}
