#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
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
    internal sealed class LineSpriteBatch : IDisposable
    {
        private int capacity;
        private Matrix view;
        private Matrix projection;
        private bool hasBegin = false;
        private DynamicVertexBuffer vertices;
        private DynamicIndexBuffer indices;
        private Batch<Texture2D, ushort> batch;
        private VertexPositionColorTexture[] vertexArray;
        private int vertexCount = 0;
        private LineSpriteEffect effect;
        private Vector3 eyePosition;

        public GraphicsDevice GraphicsDevice { get; private set; }
        public bool IsDisposed { get; private set; }


        public LineSpriteBatch(GraphicsDevice graphics, int capacity)
        {
            GraphicsDevice = graphics;

            this.capacity = capacity;
        }


        public void Begin(Matrix view, Matrix projection)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("LineSpriteBatch");

            if (batch == null)
            {
                effect = new LineSpriteEffect(GraphicsDevice);

                vertexArray = new VertexPositionColorTexture[capacity * 4];

                batch = new Batch<Texture2D, ushort>(capacity * 6);

                vertices = new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), capacity * 4, BufferUsage.WriteOnly);

                indices = new DynamicIndexBuffer(GraphicsDevice, typeof(ushort), capacity * 6, BufferUsage.WriteOnly);        
            }

            hasBegin = true;

            this.view = view;
            this.projection = projection;

            eyePosition = Matrix.Invert(view).Translation;

            batch.Clear();
            
            vertexCount = 0;
        }


        public void Draw(Texture2D texture, Vector3 start, Vector3 end, float width, Color color)
        {
            Draw(texture, start, end, width, Vector2.One, Vector2.Zero, color);
        }


        public void Draw(Texture2D texture, Vector3 start, Vector3 end, float width, Vector2 textureScale, Vector2 textureOffset, Color color)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (textureScale.X <= 0 || textureScale.Y <= 0)
                throw new ArgumentException("textureScale");

            if (texture == null)
                throw new ArgumentNullException();


            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;


            ComputeBillboardPositions(start, end, width, out aa.Position, out ab.Position, 
                                                         out ba.Position, out bb.Position);


            aa.TextureCoordinate = TransformCoordinate(Vector2.Zero, textureScale, textureOffset);
            ab.TextureCoordinate = TransformCoordinate(Vector2.UnitX, textureScale, textureOffset);
            ba.TextureCoordinate = TransformCoordinate(Vector2.UnitY, textureScale, textureOffset);
            bb.TextureCoordinate = TransformCoordinate(Vector2.One, textureScale, textureOffset);

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;
            

            // Add new vertices and indices
            batch.Add(texture, (ushort)(vertexCount + 0));
            batch.Add(texture, (ushort)(vertexCount + 1));
            batch.Add(texture, (ushort)(vertexCount + 2));
            batch.Add(texture, (ushort)(vertexCount + 1));
            batch.Add(texture, (ushort)(vertexCount + 3));
            batch.Add(texture, (ushort)(vertexCount + 2));


            vertexArray[vertexCount++] = aa;
            vertexArray[vertexCount++] = ab;
            vertexArray[vertexCount++] = ba;
            vertexArray[vertexCount++] = bb;
        }

        public void Draw(Texture2D texture, Vector3[] lineStrip, float width, Color color)
        {
            Draw(texture, lineStrip, width, Vector2.One, Vector2.Zero, color);
        }


        public void Draw(Texture2D texture, Vector3[] lineStrip, float width, Vector2 textureScale, Vector2 textureOffset, Color color)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (lineStrip == null || lineStrip.Length < 2)
                throw new ArgumentException("lineStrip");

            if (texture == null)
                throw new ArgumentNullException();


            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;

            aa.Position = ab.Position =
            ba.Position = bb.Position = Vector3.Zero;


            // We want the texture to uniformly distrubute on the line
            // even if each line segment may have different length.
            float totalLength = 0;
            float percentage = 0;

            for (int i = 1; i < lineStrip.Length; i++)
                totalLength += Vector3.Subtract(lineStrip[i], lineStrip[i - 1]).Length();


            Vector3 start = lineStrip[0];
            Vector3 lastSegment1 = Vector3.Zero;
            Vector3 lastSegment2 = Vector3.Zero;


            for (int i = 1; i < lineStrip.Length; i++)
            {
                ComputeBillboardPositions(start, lineStrip[i], width, out aa.Position, out ab.Position,
                                                                      out ba.Position, out bb.Position);

                ba.TextureCoordinate = TransformCoordinate(new Vector2(0, 1 - percentage), textureScale, textureOffset);
                bb.TextureCoordinate = TransformCoordinate(new Vector2(1, 1 - percentage), textureScale, textureOffset);

                percentage += Vector3.Subtract(lineStrip[i], lineStrip[i - 1]).Length() / totalLength;

                if (i > 1)
                {
                    // Connect adjacent segments
                    ba.Position = (ba.Position + lastSegment1) / 2;
                    bb.Position = (bb.Position + lastSegment2) / 2;

                    // Adjust the connection points to the specified width
                    Vector3 append = Vector3.Subtract(bb.Position, ba.Position);

                    append.Normalize();
                    append *= width / 2;

                    ba.Position = start - append;
                    bb.Position = start + append;
                }

                lastSegment1 = aa.Position;
                lastSegment2 = ab.Position;

                int startIndex = vertexCount;

                batch.Add(texture, (ushort)(startIndex + 0));
                batch.Add(texture, (ushort)(startIndex + 3));
                batch.Add(texture, (ushort)(startIndex + 1));
                batch.Add(texture, (ushort)(startIndex + 0));
                batch.Add(texture, (ushort)(startIndex + 2));
                batch.Add(texture, (ushort)(startIndex + 3));

                vertexArray[vertexCount++] = ba;
                vertexArray[vertexCount++] = bb;

                start = lineStrip[i];
            }

            // Last segment
            aa.TextureCoordinate = TransformCoordinate(Vector2.Zero, textureScale, textureOffset);
            ab.TextureCoordinate = TransformCoordinate(Vector2.UnitX, textureScale, textureOffset);

            vertexArray[vertexCount++] = aa;
            vertexArray[vertexCount++] = ab;
        }


        private void ComputeBillboardPositions(Vector3 start, Vector3 end, float width,
                                               out Vector3 aa, out Vector3 ab,
                                               out Vector3 ba, out Vector3 bb)
        {
            // Compute billboard facing
            Vector3 v1 = Vector3.Subtract(end, start);
            Vector3 v2 = Vector3.Subtract(eyePosition, start);

            v1.Normalize();
            v2.Normalize();

            Vector3 right = Vector3.Cross(v1, v2);

            right *= width / 2;

            // Compute destination vertices
            aa = end - right;
            ab = end + right;
            ba = start - right;
            bb = start + right;
        }

        private static Vector2 TransformCoordinate(Vector2 input, Vector2 textureScale, Vector2 textureOffset)
        {
            input -= Vector2.One * 0.5f;
            input /= textureScale;
            input += Vector2.One * 0.5f;
            input.X -= textureOffset.X;
            input.Y += textureOffset.Y;

            return input;
        }

        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (IsDisposed)
                throw new ObjectDisposedException("LineSpriteBatch");

            hasBegin = false;

            if (batch.Count <= 0)
                return;
            

            effect.View = view;
            effect.Projection = projection;


            vertices.SetData<VertexPositionColorTexture>(vertexArray, 0, vertexCount, SetDataOptions.NoOverwrite);

            GraphicsDevice.SetVertexBuffer(vertices);


            foreach (BatchItem<Texture2D, ushort> batchItem in batch.Batches)
            {
                indices.SetData<ushort>(batchItem.Values, 0, batchItem.Count, SetDataOptions.NoOverwrite);
                
                GraphicsDevice.Indices = indices;

                effect.Texture = batchItem.Key;

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

            if (indices != null)
                indices.Dispose();

            IsDisposed = true;
        }
    }
}
