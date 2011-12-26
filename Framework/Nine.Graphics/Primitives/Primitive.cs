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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Primitives
{
    /// <summary>
    /// Represents a custom primitive.
    /// </summary>
    public interface ICustomPrimitive
    {
        /// <summary>
        /// Gets the vertex buffer of the primitive.
        /// </summary>
        VertexBuffer VertexBuffer { get; }

        /// <summary>
        /// Gets the index buffer of the primitive.
        /// </summary>
        IndexBuffer IndexBuffer { get; }

        /// <summary>
        /// Gets the optional bounding sphere of the primitive.
        /// </summary>
        BoundingSphere? BoundingSphere { get; }
    }

    #region Primitive
    /// <summary>
    /// Base class for simple geometric primitive models. This provides a vertex
    /// buffer, an index buffer, plus methods for drawing the model. Classes for
    /// specific types of primitive (CubePrimitive, SpherePrimitive, etc.) are
    /// derived from this common base, and use the AddVertex and AddIndex methods
    /// to specify their geometry.
    /// </summary>
    public abstract class Primitive<T> : IDisposable, ICustomPrimitive, IGeometry where T : struct, IVertexType
    {
        #region Fields

        Matrix? IGeometry.Transform { get { return null; } }

        /// <summary>
        /// Gets a readonly list of vertex positions.
        /// </summary>
        public Vector3[] Positions { get; private set; }

        /// <summary>
        /// Gets a read-only list of geometry indices.
        /// </summary>
        public ushort[] Indices { get; private set; }


        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        List<T> vertices;
        List<Vector3> positions;
        List<ushort> indices;


        // Once all the geometry has been specified, the InitializePrimitive
        // method copies the vertex and index data into these buffers, which
        // store it on the GPU ready for efficient rendering.
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }

        /// <summary>
        /// Gets the vertex count.
        /// </summary>
        public int VertexCount { get { return VertexBuffer.VertexCount; } }

        /// <summary>
        /// Gets the primitive count.
        /// </summary>
        public int PrimitiveCount { get { return IndexBuffer.IndexCount / 3; } }

        /// <summary>
        /// Gets the graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets the optional bounding sphere of the primitive.
        /// </summary>
        public BoundingSphere? BoundingSphere { get; private set; }

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
                    if (invertWindingOrder)
                    {
                        for (int i = 0; i < Indices.Length; i += 3)
                        {
                            var temp = Indices[i + 1];
                            Indices[i + 1] = Indices[i + 2];
                            Indices[i + 2] = temp;
                        }
                        IndexBuffer.SetData(Indices);
                    }
                }
            }
        }
        private bool invertWindingOrder;

        #endregion

        #region Initialization

        /// <summary>
        /// Initializes a new instance of the <see cref="Primitive&lt;T&gt;"/> class.
        /// </summary>
        protected Primitive()
        {
            vertices = new List<T>();
            indices = new List<ushort>();
            positions = new List<Vector3>();
        }


        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, T vertex)
        {
            vertices.Add(vertex);
            positions.Add(position);
        }


        /// <summary>
        /// Adds a new index to the primitive model. This should only be called
        /// during the InitializePrimitive.
        /// </summary>
        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            indices.Add((ushort)index);
        }

        /// <summary>
        /// Adds a new index to the primitive model. This should only be called
        /// during the InitializePrimitive.
        /// </summary>
        protected void AddIndex(params int[] indices)
        {
            foreach (var index in indices)
                AddIndex(index);
        }

        /// <summary>
        /// Queries the index of the current vertex. This starts at
        /// zero, and increments every time AddVertex is called.
        /// </summary>
        protected int CurrentVertex
        {
            get { return vertices.Count; }
        }


        /// <summary>
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        /// </summary>
        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            Positions = positions.ToArray();
            Indices = indices.ToArray();

            // Create a vertex buffer, and copy our vertex data into it.
            VertexBuffer = new VertexBuffer(graphicsDevice,
                                            typeof(T),
                                            vertices.Count, BufferUsage.WriteOnly);

            VertexBuffer.SetData(vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            IndexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort),
                                          indices.Count, BufferUsage.WriteOnly);

            IndexBuffer.SetData(Indices);

            BoundingSphere = Microsoft.Xna.Framework.BoundingSphere.CreateFromPoints(positions);

            // Free up the list.
            positions = null;
            indices = null;
            vertices = null;
        }

        /// <summary>
        /// Draws the primitive model, using the specified effect. Unlike the other
        /// Draw overload where you just specify the world/view/projection matrices
        /// and color, this method does not set any renderstates, so you must make
        /// sure all states are set to sensible values before you call it.
        /// </summary>
        public void Draw(Effect effect)
        {
            // Set our vertex declaration, vertex buffer, and index buffer.
            GraphicsDevice.SetVertexBuffer(VertexBuffer);
            GraphicsDevice.Indices = IndexBuffer;

            // Draw the model, using the specified effect.
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                     vertices.Count, 0, indices.Count / 3);
            }
        }


        /// <summary>
        /// Finalizer.
        /// </summary>
        ~Primitive()
        {
            Dispose(false);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }


        /// <summary>
        /// Frees resources used by this object.
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (VertexBuffer != null)
                {
                    VertexBuffer.Dispose();
                    VertexBuffer = null;
                }
                if (IndexBuffer != null)
                {
                    IndexBuffer.Dispose();
                    IndexBuffer = null;
                }
            }
        }
        #endregion
    }
    #endregion
}
