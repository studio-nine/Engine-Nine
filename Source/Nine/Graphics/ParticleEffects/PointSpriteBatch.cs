#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    internal sealed class PointSpriteBatch : IDisposable
    {
        #region Vertex
        internal struct Vertex : IVertexType
        {
            public Vector3 Position;
            public Color Color;
            public Vector2 Size;
            public float Rotation;
            public Vector2 TextureCoordinates;

            public static int SizeInBytes = 4 * (3 + 1 + 2 + 1 + 2);
            public static VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement(0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
                new VertexElement(12, VertexElementFormat.Color, VertexElementUsage.Color, 0),
                new VertexElement(16, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 1),
                new VertexElement(24, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 2),
                new VertexElement(28, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            };

            public VertexDeclaration VertexDeclaration
            {
                get { return new VertexDeclaration(VertexElements); }
            }
        }
        #endregion

        #region Key
        internal struct Key
        {
            public Texture2D Texture;
            public EffectTechnique Technique;
        }
        #endregion

        private int capacity;
        private Matrix view;
        private Matrix projection;
        private bool hasBegin = false;
        private DynamicVertexBuffer vertices;
        private DynamicIndexBuffer indices;
        private Batch<Key, ushort> batch;
        private PointSpriteEffect effect;
        private Vertex[] vertexArray;
        private int vertexCount = 0;


        public GraphicsDevice GraphicsDevice { get; private set; }
        public bool IsDisposed { get; private set; }
        

        public PointSpriteBatch(GraphicsDevice graphics, int capacity)
        {
            GraphicsDevice = graphics;
            this.capacity = capacity;
        }


        public void Begin(Matrix view, Matrix projection)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("PointSpriteBatch");

            if (batch == null)
            {
                effect = new PointSpriteEffect(GraphicsDevice);

                vertexArray = new Vertex[capacity * 4];

                batch = new Batch<Key, ushort>(capacity);

                vertices = new DynamicVertexBuffer(GraphicsDevice, typeof(Vertex), capacity * 4, BufferUsage.WriteOnly);

                indices = new DynamicIndexBuffer(GraphicsDevice, typeof(ushort), capacity * 6, BufferUsage.WriteOnly);  
            }

            hasBegin = true;

            this.view = view;
            this.projection = projection;

            batch.Clear();
        
            vertexCount = 0;
        }


        public void Draw(Texture2D texture, Vector3 position, float size, float rotation, Color color)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (IsDisposed)
                throw new ObjectDisposedException("PointSpriteBatch");

            if (texture == null)
                throw new ArgumentNullException();


            Key key;

            key.Texture = texture;

            if (Math.Abs(rotation) <= float.Epsilon)
                key.Technique = effect.Techniques["NonRotatingParticles"];
            else
                key.Technique = effect.Techniques["RotatingParticles"];


            batch.Add(key, (ushort)(vertexCount + 0));
            batch.Add(key, (ushort)(vertexCount + 1));
            batch.Add(key, (ushort)(vertexCount + 2));
            batch.Add(key, (ushort)(vertexCount + 1));
            batch.Add(key, (ushort)(vertexCount + 3));
            batch.Add(key, (ushort)(vertexCount + 2));

            Vertex vertex;

            vertex.Position = position;
            vertex.Color = color;
            vertex.Size = new Vector2(size, size * texture.Height / texture.Width);
            vertex.Rotation = rotation;
            
            vertex.TextureCoordinates = Vector2.UnitY;
            vertexArray[vertexCount++] = vertex;

            vertex.TextureCoordinates = Vector2.One;
            vertexArray[vertexCount++] = vertex;

            vertex.TextureCoordinates = Vector2.Zero;
            vertexArray[vertexCount++] = vertex;

            vertex.TextureCoordinates = Vector2.UnitX;
            vertexArray[vertexCount++] = vertex;
        }


        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (IsDisposed)
                throw new ObjectDisposedException("PointSpriteBatch");

            hasBegin = false;

            if (batch.Count <= 0)
                return;


            effect.View = view;
            effect.Projection = projection;

            vertices.SetData<Vertex>(vertexArray, 0, vertexCount, SetDataOptions.NoOverwrite);

            GraphicsDevice.SetVertexBuffer(vertices);

            foreach (BatchItem<Key, ushort> batchItem in batch.Batches)
            {
                indices.SetData<ushort>(batchItem.Values, 0, batchItem.Count, SetDataOptions.NoOverwrite);
                
                GraphicsDevice.Indices = indices;

                effect.Texture = batchItem.Key.Texture;

                effect.CurrentTechnique = batchItem.Key.Technique;
                effect.CurrentTechnique.Passes[0].Apply();
                
                GraphicsDevice.DrawIndexedPrimitives(
                    PrimitiveType.TriangleList, 0, 0, vertexCount, 0, batchItem.Count / 3);
                
                GraphicsDevice.Indices = null;
            }
        }


        public void Dispose()
        {
            if (vertices != null)
                vertices.Dispose();

            IsDisposed = true;
        }
    }
}
