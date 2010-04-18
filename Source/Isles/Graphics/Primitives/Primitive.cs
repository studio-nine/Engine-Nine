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

namespace Isles.Graphics.Primitives
{
    #region Primitive
    /// <summary>
    /// Base class for simple geometric primitive models. This provides a vertex
    /// buffer, an index buffer, plus methods for drawing the model. Classes for
    /// specific types of primitive (CubePrimitive, SpherePrimitive, etc.) are
    /// derived from this common base, and use the AddVertex and AddIndex methods
    /// to specify their geometry.
    /// </summary>
    internal abstract class Primitive : IDisposable, IGeometry
    {
        #region Fields


        // During the process of constructing a primitive model, vertex
        // and index data is stored on the CPU in these managed lists.
        public List<VertexPositionNormalTexture> Vertices { get; private set; }
        public List<Vector3> Positions { get; private set; }
        public List<ushort> Indices { get; private set; }


        // Once all the geometry has been specified, the InitializePrimitive
        // method copies the vertex and index data into these buffers, which
        // store it on the GPU ready for efficient rendering.
        public VertexDeclaration VertexDeclaration { get; private set; }
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }
        public BasicEffect BasicEffect { get; private set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        #endregion

        #region IGeometry Members
        
        public IList<Vector3> Normals
        {
            get { throw new NotImplementedException(); }
        }

        public IList<Vector2> TextureCoordinates
        {
            get { throw new NotImplementedException(); }
        }

        IList<Vector3> IGeometry.Positions
        {
            get { return Positions.AsReadOnly(); }
        }

        IList<ushort> IGeometry.Indices
        {
            get { return Indices.AsReadOnly(); }
        }

        #endregion

        #region Initialization

        public Primitive()
        {
            Vertices = new List<VertexPositionNormalTexture>();
            Indices = new List<ushort>();
            Positions = new List<Vector3>();
        }


        /// <summary>
        /// Adds a new vertex to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddVertex(Vector3 position, Vector3 normal)
        {
            Vertices.Add(new VertexPositionNormalTexture(position, normal, Vector2.Zero));
            Positions.Add(position);
        }

        protected void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            Vertices.Add(new VertexPositionNormalTexture(position, normal, uv));
            Positions.Add(position);
        }


        /// <summary>
        /// Adds a new index to the primitive model. This should only be called
        /// during the initialization process, before InitializePrimitive.
        /// </summary>
        protected void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            Indices.Add((ushort)index);
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
        /// Once all the geometry has been specified by calling AddVertex and AddIndex,
        /// this method copies the vertex and index data into GPU format buffers, ready
        /// for efficient rendering.
        protected void InitializePrimitive(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;

            // Create a vertex declaration, describing the format of our vertex data.
            VertexDeclaration = new VertexDeclaration(graphicsDevice,
                                                VertexPositionNormalTexture.VertexElements);

            // Create a vertex buffer, and copy our vertex data into it.
            VertexBuffer = new VertexBuffer(graphicsDevice,
                                            typeof(VertexPositionNormalTexture),
                                            Vertices.Count, BufferUsage.None);
            
            VertexBuffer.SetData(Vertices.ToArray());

            // Create an index buffer, and copy our index data into it.
            if (Indices.Count > 0)
            {
                IndexBuffer = new IndexBuffer(graphicsDevice, typeof(ushort),
                                              Indices.Count, BufferUsage.None);

                IndexBuffer.SetData(Indices.ToArray());
            }

            // Create a BasicEffect, which will be used to render the primitive.
            BasicEffect = new BasicEffect(graphicsDevice, null);
            
            BasicEffect.EnableDefaultLighting();
            BasicEffect.PreferPerPixelLighting = false;
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
                if (VertexDeclaration != null)
                    VertexDeclaration.Dispose();

                if (VertexBuffer != null)
                    VertexBuffer.Dispose();

                if (IndexBuffer != null)
                    IndexBuffer.Dispose();

                if (BasicEffect != null)
                    BasicEffect.Dispose();
            }
        }


        #endregion

        #region Draw


        /// <summary>
        /// Draws the primitive model, using the specified effect. Unlike the other
        /// Draw overload where you just specify the world/view/projection matrices
        /// and color, this method does not set any renderstates, so you must make
        /// sure all states are set to sensible values before you call it.
        /// </summary>
        public virtual void Draw(Effect effect)
        {
            GraphicsDevice graphicsDevice = effect.GraphicsDevice;

            // Set our vertex declaration, vertex buffer, and index buffer.
            graphicsDevice.VertexDeclaration = VertexDeclaration;

            graphicsDevice.Vertices[0].SetSource(VertexBuffer, 0,
                                                 VertexPositionNormalTexture.SizeInBytes);

            graphicsDevice.Indices = IndexBuffer;

            // Draw the model, using the specified effect.
            effect.Begin();

            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Begin();

                if (Indices.Count > 0)
                {
                    graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                     Vertices.Count, 0, Indices.Count / 3);
                }
                else
                {
                    graphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Vertices.Count / 3);
                
                }

                effectPass.End();
            }

            effect.End();
        }


        public void Draw(GraphicsEffect effect, GameTime time, Matrix world, Matrix view, Matrix projection)
        {
            effect.World = world;
            effect.View = view;
            effect.Projection = projection;

            // Set our vertex declaration, vertex buffer, and index buffer.
            GraphicsDevice.VertexDeclaration = VertexDeclaration;
            GraphicsDevice.Vertices[0].SetSource(VertexBuffer, 0, VertexPositionNormalTexture.SizeInBytes);
            GraphicsDevice.Indices = IndexBuffer;

            effect.Begin(GraphicsDevice, time);

            if (Indices.Count > 0)
            {
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                                                 Vertices.Count, 0, Indices.Count / 3);
            }
            else
            {
                GraphicsDevice.DrawPrimitives(PrimitiveType.TriangleList, 0, Vertices.Count / 3);
            }

            effect.End();
        }


        /// <summary>
        /// Draws the primitive model, using a BasicEffect shader with default
        /// lighting. Unlike the other Draw overload where you specify a custom
        /// effect, this method sets important renderstates to sensible values
        /// for 3D model rendering, so you do not need to set these states before
        /// you call it.
        /// </summary>
        public virtual void Draw(Matrix world, Matrix view, Matrix projection, Color color)
        {
            // Set BasicEffect parameters.
            BasicEffect.World = world;
            BasicEffect.View = view;
            BasicEffect.Projection = projection;
            BasicEffect.DiffuseColor = color.ToVector3();
            BasicEffect.Alpha = color.A / 255.0f;

            // Set important renderstates.
            RenderState renderState = BasicEffect.GraphicsDevice.RenderState;

            renderState.AlphaTestEnable = false;
            renderState.DepthBufferEnable = true;
            renderState.DepthBufferFunction = CompareFunction.LessEqual;

            if (color.A < 255)
            {
                // Set renderstates for alpha blended rendering.
                renderState.AlphaBlendEnable = true;
                renderState.AlphaBlendOperation = BlendFunction.Add;
                renderState.SourceBlend = Blend.SourceAlpha;
                renderState.DestinationBlend = Blend.InverseSourceAlpha;
                renderState.SeparateAlphaBlendEnabled = false;
                renderState.DepthBufferWriteEnable = false;
            }
            else
            {
                // Set renderstates for opaque rendering.
                renderState.AlphaBlendEnable = false;
                renderState.DepthBufferWriteEnable = true;
            }

            // Draw the model, using BasicEffect.
            Draw(BasicEffect);
        }


        #endregion
    }
    #endregion
}
