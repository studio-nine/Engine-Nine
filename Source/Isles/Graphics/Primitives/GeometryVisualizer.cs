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
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics.Primitives
{
    internal sealed class GeometryVisualizer
    {
        public VertexBuffer VertexBuffer { get; private set; }
        public IndexBuffer IndexBuffer { get; private set; }
        public VertexDeclaration VertexDeclaration { get; private set; }
        public BasicEffect BasicEffect { get; private set; }


        public GeometryVisualizer(GraphicsDevice graphics, IGeometry geometry)
            : this(graphics, geometry.Positions, geometry.Indices)
        {
        }


        public GeometryVisualizer(GraphicsDevice graphics, IEnumerable<Vector3> vertices, IEnumerable<ushort> indices)
        {
            List<VertexPositionColor> vb = new List<VertexPositionColor>();
            List<ushort> ib = new List<ushort>(indices);

            foreach (Vector3 v in vertices)
                vb.Add(new VertexPositionColor(v, Color.White));


            if (ib.Count <= 0)
                throw new InvalidOperationException("Can't visualize the geometry because it doesn't have an index buffer");


            // Vertex buffer
            VertexBuffer = new VertexBuffer(graphics, VertexPositionColor.SizeInBytes * vb.Capacity, BufferUsage.None);
            VertexBuffer.SetData<VertexPositionColor>(vb.ToArray());

            // Index buffer
            IndexBuffer = new IndexBuffer(graphics, typeof(ushort), ib.Count, BufferUsage.None);
            IndexBuffer.SetData<ushort>(ib.ToArray());

            // Vertex declaraction
            VertexDeclaration = new VertexDeclaration(graphics, VertexPositionColor.VertexElements);


            BasicEffect = new BasicEffect(graphics, null);
        }


        public void Draw(Matrix world, Matrix view, Matrix projection, Color faceColor, Color borderColor)
        {
            BasicEffect.World = world;
            BasicEffect.View = view;
            BasicEffect.Projection = projection;
            BasicEffect.DiffuseColor = faceColor.ToVector3();
            BasicEffect.LightingEnabled = false;

            BasicEffect.Begin();

            // We know it ahead that basic effect contains only one pass
            BasicEffect.CurrentTechnique.Passes[0].Begin();

            GraphicsDevice graphics = VertexBuffer.GraphicsDevice;

            graphics.Indices = IndexBuffer;
            graphics.VertexDeclaration = VertexDeclaration;
            graphics.Vertices[0].SetSource(VertexBuffer, 0, VertexPositionColor.SizeInBytes);
            
            // Draw face
            graphics.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, 0, 0, 
                VertexBuffer.SizeInBytes / VertexPositionColor.SizeInBytes, 0,
                IndexBuffer.SizeInBytes / 6);

            // Draw border
            graphics.RenderState.FillMode = FillMode.WireFrame;

            BasicEffect.DiffuseColor = borderColor.ToVector3();
            BasicEffect.CommitChanges();

            graphics.DrawIndexedPrimitives(
                PrimitiveType.TriangleList, 0, 0,
                VertexBuffer.SizeInBytes / VertexPositionColor.SizeInBytes, 0,
                IndexBuffer.SizeInBytes / 6);

            graphics.RenderState.FillMode = FillMode.Solid;


            BasicEffect.CurrentTechnique.Passes[0].End();

            BasicEffect.End();
        }


        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();

            if (IndexBuffer != null)
                IndexBuffer.Dispose();
        }
    }
}
