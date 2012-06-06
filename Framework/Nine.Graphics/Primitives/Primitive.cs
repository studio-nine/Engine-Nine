#region File Description
//-----------------------------------------------------------------------------
// GeometricPrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Drawing;
#endregion

namespace Nine.Graphics.Primitives
{
    /// <summary>
    /// Base class for simple geometric primitive models. 
    /// </summary>
    [ContentProperty("Material")]
    public abstract class Primitive<T> : Transformable, IDrawableObject, IDisposable where T : struct, IVertexType
    {
        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets whether the drawable is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets a value indicating whether this primitive resides inside the view frustum last frame.
        /// </summary>
        public bool InsideViewFrustum { get; internal set; }
        
        /// <summary>
        /// Gets the material used by this drawable.
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

        Material IDrawableObject.Material
        {
            get { return materialForRendering; }
        }
        private Material materialForRendering;

        /// <summary>
        /// Gets the optional bounding sphere of the primitive.
        /// </summary>
        public BoundingBox BoundingBox { get; private set; }

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

        /// <summary>
        /// Initializes a new instance of the <see cref="Primitive&lt;T&gt;"/> class.
        /// </summary>
        protected Primitive(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Visible = true;
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
            try
            {
                Dispose();

                // Check if we can find a cached vb/ib for this primitive
                Type type = GetType();
                List<PrimitiveCache> cachedPrimitives;
                if (PrimitiveCache.TryGetValue(type, out cachedPrimitives))
                {
                    for (int i = 0; i < cachedPrimitives.Count; i++)
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
            }
            finally
            {
                // Free up the list.
                Positions.Clear();
                Indices.Clear();
                Vertices.Clear();
            }
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
                BoundingBox = BoundingBoxExtensions.CreateAxisAligned(cachedPrimitive.BoundingBox, AbsoluteTransform);
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
        /// Adds a new index to the primitive model. This should only be called
        /// during the InitializePrimitive.
        /// </summary>
        protected void AddIndex(params int[] indices)
        {
            for (int i = 0; i < indices.Length; i++)
            {
                AddIndex(indices[i]);
            }
        }

        /// <summary>
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex
        {
            get { return Vertices.Count; }
        }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public void BeginDraw(DrawingContext context)
        {
            InsideViewFrustum = true;
            materialForRendering = Material ?? 
                materialLevels.UpdateLevelOfDetail(
                    Vector3.Distance(context.EyePosition, AbsoluteTransform.Translation));
        }

        /// <summary>
        /// Draws this object with the specified material.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
            material.World = AbsoluteTransform;
            material.BeginApply(context);

            if (needsRebuild)
                Rebuild();

            context.SetVertexBuffer(cachedPrimitive.VertexBuffer, 0);

            if (cachedPrimitive.IndexBuffer != null)
            {
                GraphicsDevice.Indices = cachedPrimitive.IndexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(primitiveType, 0, 0,
                    cachedPrimitive.VertexBuffer.VertexCount, 0, cachedPrimitive.PrimitiveCount);
            }
            else
            {
                GraphicsDevice.DrawPrimitives(primitiveType, 0, cachedPrimitive.PrimitiveCount);
            }

            material.EndApply(context);
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

        ~Primitive()
        {
            Dispose(false);
        }

        void IDrawableObject.EndDraw(DrawingContext context) { }     
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
