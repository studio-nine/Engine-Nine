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
using Isles.Graphics.Vertices;
#endregion


namespace Isles.Graphics.Landscape
{
    public sealed class Terrain : IDisposable
    {
        public TerrainGeometry Geometry { get; private set; }
        public List<GraphicsEffect> Layers { get; private set; }
        public GraphicsDevice Graphics { get; private set; }


        private VertexBuffer vertices;
        private IndexBuffer indices;
        private VertexDeclaration declaration;
        private int vertexCount;
        private int primitiveCount;


        public Terrain(GraphicsDevice graphics, TerrainGeometry geometry)
        {
            if (graphics == null || geometry == null)
                throw new ArgumentNullException();


            Graphics = graphics;
            Geometry = geometry;


            Layers = new List<GraphicsEffect>();
            vertices = new VertexBuffer(graphics, VertexPositionNormalTexture.SizeInBytes * geometry.Positions.Count, BufferUsage.None);
            indices = new IndexBuffer(graphics, typeof(ushort), geometry.Indices.Count, BufferUsage.None);
            declaration = new VertexDeclaration(graphics, VertexPositionNormalTexture.VertexElements);
            

            // Fill vertices
            vertexCount = geometry.Positions.Count;

            VertexPositionNormalTexture[] vertexData = new VertexPositionNormalTexture[vertexCount];

            for (int i = 0; i < vertexCount; i++)
            {
                vertexData[i].Position = geometry.Positions[i];
                vertexData[i].Normal = -geometry.NormalData[i];
                vertexData[i].TextureCoordinate.X = 1.0f * (i % geometry.HeightmapWidth) / geometry.HeightmapWidth;
                vertexData[i].TextureCoordinate.Y = 1.0f * (i / geometry.HeightmapHeight) / geometry.HeightmapHeight; 
            }

            vertices.SetData<VertexPositionNormalTexture>(vertexData);


            // Fill indices
            primitiveCount = geometry.Indices.Count / 3;

            ushort[] indexData = new ushort[geometry.Indices.Count];

            geometry.Indices.CopyTo(indexData, 0);

            indices.SetData<ushort>(indexData);
        }


        public void Draw(GameTime time, Matrix view, Matrix projection)
        {
            if (Layers.Count > 0)
            {
                Graphics.Indices = indices;
                Graphics.VertexDeclaration = declaration;
                Graphics.Vertices[0].SetSource(vertices, 0, VertexPositionNormalTexture.SizeInBytes);


                foreach (GraphicsEffect layer in Layers)
                {
                    layer.View = view;
                    layer.Projection = projection;
                    layer.World = Geometry.Transform;

                    layer.Begin(Graphics, time);

                    Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);

                    layer.End();
                }
            }
        }


        public void Draw(GraphicsEffect effect, GameTime time, Matrix view, Matrix projection)
        {
            Graphics.Indices = indices;
            Graphics.VertexDeclaration = declaration;
            Graphics.Vertices[0].SetSource(vertices, 0, VertexPositionNormalTexture.SizeInBytes);


            effect.View = view;
            effect.Projection = projection;
            effect.World = Geometry.Transform;

            effect.Begin(Graphics, time);

            Graphics.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vertexCount, 0, primitiveCount);

            effect.End();
        }


        public void Dispose()
        {
            if (vertices != null)
                vertices.Dispose();
            if (indices != null)
                indices.Dispose();
            if (declaration != null)
                declaration.Dispose();
        }
    }
}
