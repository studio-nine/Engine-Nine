#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// A square block made up of surface patch parts. The whole surface is rendered patch by patch.
    /// </summary>
    public class DrawableSurfacePatch : IDisposable
    {
        /// <summary>
        /// Gets the number of tessellation of this patch.
        /// </summary>
        public int Tessellation { get; internal set; }

        /// <summary>
        /// Gets the x index of the patch on the parent surface.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Gets the y index of the patch on the parent surface.
        /// </summary>
        public int Y { get; private set; }
        
        /// <summary>
        /// Gets vertex buffer of this patch.
        /// </summary>
        public VertexBuffer VertexBuffer { get; internal set; }

        /// <summary>
        /// Gets index buffer of this patch.
        /// </summary>
        public IndexBuffer IndexBuffer { get; internal set; }
        
        /// <summary>
        /// Gets the number of primitives that made up the patch.
        /// </summary>
        public int PrimitiveCount { get; internal set; }

        /// <summary>
        /// Gets the number of vertices that made up the patch.
        /// </summary>
        public int VertexCount { get { return (Tessellation + 1) * (Tessellation + 1); } }

        /// <summary>
        /// Gets all the patch parts of this patch.
        /// </summary>
        internal ReadOnlyCollection<DrawableSurfacePatchPart> PatchParts { get; private set; }

        /// <summary>
        /// Gets all the effects used to draw the surface patch.
        /// </summary>
        public Collection<Effect> Effects { get; internal set; }

        /// <summary>
        /// Gets the underlying GraphicsDevice.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the transform matrix used to draw the patch.
        /// </summary>
        public Matrix Transform { get { return Matrix.CreateTranslation(position); } }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }

        #region BoundingBox & Position
        /// <summary>
        /// Gets the axis aligned bounding box of this surface patch.
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

        /// <summary>
        /// Gets the center position of the surface patch.
        /// </summary>
        public Vector3 Position
        {
            get { return position + offset; }
            internal set { position = value; UpdatePartPositions(); }
        }

        private Heightmap heightmap;
        private BoundingBox boundingBox;
        private Vector3 position;
        private Vector3 offset;
        private DrawableSurface surface;
        #endregion

        /// <summary>
        /// Constructor is for internal use only.
        /// </summary>
        internal DrawableSurfacePatch(DrawableSurface surface, GraphicsDevice graphics, Heightmap geometry, int xPatch, int yPatch, int tessellation)
        {
            if (6 * tessellation * tessellation > ushort.MaxValue)
                throw new ArgumentOutOfRangeException();

            if (tessellation < 2 || tessellation % 2 == 1)
                throw new ArgumentOutOfRangeException("Patch tessellation must be even.");

            this.surface = surface;
            this.GraphicsDevice = graphics;
            this.heightmap = geometry;
            this.Tessellation = tessellation;
            this.X = xPatch;
            this.Y = yPatch;
            this.Effects = new Collection<Effect>();

            
            // Initialize patch parts
            DrawableSurfacePatchPart[] parts = new DrawableSurfacePatchPart[tessellation * tessellation / 4];

            int i = 0;
            int part = tessellation / 2;

            for (int y = 0; y < part; y++)
            {
                for (int x = 0; x < part; x++)
                {
                    DrawableSurfacePatchPart p = new DrawableSurfacePatchPart();

                    p.Patch = this;
                    p.Mask = 0xFF;
                    p.X = x;
                    p.Y = y;

                    parts[i] = p;

                    i++;
                }
            }

            PatchParts = new ReadOnlyCollection<DrawableSurfacePatchPart>(parts);
            
            // Compute bounding box
            boundingBox = BoundingBox.CreateFromPoints(EnumeratePositions());

            UpdatePartPositions();
        }

        internal virtual void Invalidate() 
        {
            offset = new Vector3();

            int xPatchCount = surface.TessellationX / Tessellation;
            int yPatchCount = surface.TessellationY / Tessellation;

            float step = surface.Size.X / surface.TessellationX;

            offset.X = (X + 0.5f - xPatchCount * 0.5f) * step * Tessellation;
            offset.Y = (Y + 0.5f - yPatchCount * 0.5f) * step * Tessellation;
            offset.Z = 0;
        }

        internal void Freeze()
        {
            heightmap = null;
            PatchParts = null;
        }

        internal Vector3 GetPosition(DrawableSurfacePatchPart part, Point pt)
        {
            if (surface.IsFreezed)
                throw new InvalidOperationException(
                    "Cannot perform this operation when DrawableSurface is freezed");

            return position + heightmap.GetPosition(
                pt.X + part.X * 2 + X * Tessellation, 
                pt.Y + part.Y * 2 + Y * Tessellation);
        }

        private IEnumerable<Vector3> EnumeratePositions()
        {
            for (int x = X * Tessellation; x <= (X + 1) * Tessellation; x++)
                for (int y = Y * Tessellation; y <= (Y + 1) * Tessellation; y++)
                    yield return heightmap.GetPosition(x, y);
        }
        
        private void UpdatePartPositions()
        {
            int i = 0;
            int part = Tessellation / 2;

            for (int y = 0; y < part; y++)
            {
                for (int x = 0; x < part; x++)
                {
                    float max = float.MinValue;
                    Vector3 position = new Vector3();

                    position.X = (x + 0.5f - part * 0.5f) * heightmap.Step * 2 + Position.X;
                    position.Y = (y + 0.5f - part * 0.5f) * heightmap.Step * 2 + Position.Y;
                    position.Z = Position.Z;

                    foreach (Point pt in PatchParts[i].GetIndices())
                    {
                        Vector3 v = GetPosition(PatchParts[i], pt);

                        if (v.Z > max)
                            max = v.Z;
                    }

                    PatchParts[i].BoundingBox = new BoundingBox(
                        new Vector3(position.X - heightmap.Step, position.Y - heightmap.Step, position.Z),
                        new Vector3(position.X + heightmap.Step, position.Y + heightmap.Step, max));

                    i++;
                }
            }
        }

        /// <summary>
        /// Draws this surface patch.
        /// </summary>
        public void Draw()
        {
            if (Effects.Count <= 0 || PrimitiveCount <= 0)
                return;            

            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffer(VertexBuffer);

            foreach (Effect effect in Effects)
            {
                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Apply();

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);
                }
            }
        }

        /// <summary>
        /// Draws this surface patch using the specified effect.
        /// </summary>
        public void Draw(Effect effect)
        {
            if (effect == null || PrimitiveCount <= 0)
                return;

            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);
            }
        }

        /// <summary>
        /// Disposes any resources associated with this instance.
        /// </summary>
        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();

            if (IndexBuffer != null)
                IndexBuffer.Dispose();
        }
    }

    internal class DrawableSurfacePatchImpl<T> : DrawableSurfacePatch where T: struct, IVertexType
    {
        public DrawSurfaceFillVertex<T> FillVertex;

        internal DrawableSurfacePatchImpl(DrawableSurface surface, GraphicsDevice graphics, Heightmap geometry, int xPatch, int yPatch, int tessellation)
            : base(surface, graphics, geometry, xPatch, yPatch, tessellation)
        {
            // Initialize vertices and indices
            VertexBuffer = new VertexBuffer(GraphicsDevice,
                                            typeof(T),
                                            VertexCount, BufferUsage.WriteOnly);

            IndexBuffer = new IndexBuffer(GraphicsDevice,
                                            typeof(ushort), 6 * Tessellation * Tessellation, BufferUsage.WriteOnly);
        }

        private static T[] vertexPool;
        private static ushort[] indexPool;

        internal override void Invalidate()
        {
            base.Invalidate();

            // Reuse the array to avoid creating large chunk of data.
            int indexCount = 6 * Tessellation * Tessellation;

            if (vertexPool == null || vertexPool.Length < VertexCount)
                vertexPool = new T[VertexCount];
            if (indexPool == null || indexPool.Length < indexCount)
                indexPool = new ushort[indexCount];
            
            // Fill vertices
            int i = 0;

            for (int x = X * Tessellation; x <= (X + 1) * Tessellation; x++)
            {
                for (int y = Y * Tessellation; y <= (Y + 1) * Tessellation; y++)
                {
                    FillVertex(x, y, ref vertexPool[i]);

                    i++;
                }
            }

            VertexBuffer.SetData<T>(vertexPool, 0, VertexCount);

            // Fill indices
            i = 0;
            PrimitiveCount = 0;
            int part = Tessellation / 2;

            for (int y = 0; y < part; y++)
            {
                for (int x = 0; x < part; x++)
                {
                    foreach (Point pt in PatchParts[i++].GetIndices())
                    {
                        indexPool[PrimitiveCount++] = (ushort)(pt.X + x * 2 + (pt.Y + y * 2) * (Tessellation + 1));
                    }
                }
            }

            if (PrimitiveCount > 0)
                IndexBuffer.SetData<ushort>(indexPool, 0, PrimitiveCount);

            PrimitiveCount /= 3;
        }
    }
}
