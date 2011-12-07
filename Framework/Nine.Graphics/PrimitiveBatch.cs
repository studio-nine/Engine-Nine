#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines primitive sort-rendering options.
    /// </summary>
    public enum PrimitiveSortMode
    {
        /// <summary>
        /// Primitives are not drawn until End is called. End will apply graphics device
        /// settings and draw all the sprites in one batch, in the same order calls to
        /// Draw were received. This mode allows Draw calls to two or more instances
        /// of ModelBatch without introducing conflicting graphics device settings.
        /// ModelBatch defaults to Deferred mode.
        /// </summary>
        Deferred = 0,

        /// <summary>
        /// Begin will apply new graphics device settings, and primitives will be drawn
        /// within each Draw call. In Immediate mode there can only be one active 
        /// ModelBatch instance without introducing conflicting device settings.
        /// </summary>
        Immediate = 1,
    }
    
    /// <summary>
    /// Enables a group of dynamic primitives to be drawn.
    /// </summary>
    public class PrimitiveBatch : IDisposable
    {
        private int VertexBufferSize;
        private int IndexBufferSize;

        private Effect effect;
        private BasicEffect basicEffect;
        private PrimitiveSortMode sort;
        private bool hasBegin = false;
        private bool hasPrimitiveBegin = false;
        private Vector3 cameraPosition;

        private BlendState blendState;
        private SamplerState samplerState;
        private DepthStencilState depthStencilState;
        private RasterizerState rasterizerState;

        private DynamicVertexBuffer vertexBuffer;
        private DynamicIndexBuffer indexBuffer;
        private VertexPositionColorTexture[] vertexData;
        private ushort[] indexData;
        
        private List<PrimitiveBatchEntry> batches = new List<PrimitiveBatchEntry>();
        private List<int> vertexSegments = new List<int>();
        private List<int> indexSegments = new List<int>();

        private PrimitiveBatchEntry currentPrimitive;
        private int currentSegment;
        private int currentVertex;
        private int currentIndex;
        private int currentBaseVertex;
        private int currentBaseIndex;
        private int baseSegmentVertex;
        private int baseSegmentIndex;
        private int beginSegment;
        
        private Matrix view;
        private Matrix projection;

        internal int VertexCount { get; private set; }
        internal int PrimitiveCount { get; private set; }

        /// <summary>
        /// Gets the underlying graphics device used by this PrimitiveBatch.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Creates a new PrimitiveBatch instance.
        /// </summary>
#if WINDOWS_PHONE
        public PrimitiveBatch(GraphicsDevice graphics) : this(graphics, 512)
#else
        public PrimitiveBatch(GraphicsDevice graphics) : this(graphics, 2048)
#endif
        {
        }

        /// <summary>
        /// Creates a new PrimitiveBatch instance.
        /// </summary>
        public PrimitiveBatch(GraphicsDevice graphics, int maxVertexCountPerPrimitive)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            VertexBufferSize = maxVertexCountPerPrimitive;
            IndexBufferSize = maxVertexCountPerPrimitive * 2;

            GraphicsDevice = graphics;
            basicEffect = new BasicEffect(graphics);
            basicEffect.VertexColorEnabled = true;

            vertexData = new VertexPositionColorTexture[VertexBufferSize];
            indexData = new ushort[IndexBufferSize];
            
            vertexBuffer = new DynamicVertexBuffer(GraphicsDevice, typeof(VertexPositionColorTexture), VertexBufferSize, BufferUsage.WriteOnly);
            indexBuffer = new DynamicIndexBuffer(GraphicsDevice, typeof(ushort), IndexBufferSize, BufferUsage.WriteOnly);
        }

        public void Begin(Matrix view, Matrix projection)
        {
            Begin(PrimitiveSortMode.Deferred, view, projection);
        }

        public void Begin(PrimitiveSortMode sortMode, Matrix view, Matrix projection)
        {
            Begin(sortMode, view, projection, null , null, null, null);
        }

        public void Begin(PrimitiveSortMode sortMode, Matrix view, Matrix projection, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
        {
            Begin(sortMode, view, projection, blendState, samplerState, depthStencilState, rasterizerState, null);
        }

        public void Begin(PrimitiveSortMode sortMode, Matrix view, Matrix projection, BlendState blendState, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState, Effect effect)
        {
            if (hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            this.view = view;
            this.projection = projection;
            this.sort = sortMode;
            this.hasBegin = true;
            this.effect = effect ?? basicEffect;
            this.VertexCount = 0;
            this.PrimitiveCount = 0;

            Matrix.Invert(ref view, out view);
            cameraPosition.X = view.M41;
            cameraPosition.Y = view.M42;
            cameraPosition.Z = view.M43;
            
            this.blendState = blendState != null ? blendState : BlendState.AlphaBlend;
            this.samplerState = samplerState != null ? samplerState : SamplerState.LinearWrap;
            this.depthStencilState = depthStencilState != null ? depthStencilState : DepthStencilState.DepthRead;
            this.rasterizerState = rasterizerState != null ? rasterizerState : RasterizerState.CullCounterClockwise;
            
            Flush();

            var matrices = this.effect as IEffectMatrices;
            if (matrices != null)
            {
                matrices.World = Matrix.Identity;
                matrices.View = this.view;
                matrices.Projection = this.projection;
            }
        }

        public void DrawBillboard(Texture2D texture, Vector3 position, float size, Color color)
        {
            DrawBillboard(texture, position, size, size, 0, Vector3.UnitZ, null, null, color);
        }

        public void DrawBillboard(Texture2D texture, Vector3 position, float width, float height, float rotation, Vector3 up, Matrix? textureTransform, Matrix? world, Color color)
        {
            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;

            Matrix transform = new Matrix();
            Matrix billboard = new Matrix();

            Matrix.CreateRotationZ(rotation, out transform);
            Matrix.CreateBillboard(ref position, ref cameraPosition, ref up, null, out billboard);
            Matrix.Multiply(ref transform, ref billboard, out transform);

            if (float.IsNaN(transform.M11))
            {
                transform = Matrix.Identity;
                transform.M11 = -1;
            }

            transform.M41 = position.X;
            transform.M42 = position.Y;
            transform.M43 = position.Z;

            aa.Position.X = +width * 0.5f; aa.Position.Y = +height * 0.5f; aa.Position.Z = 0;
            ab.Position.X = -width * 0.5f; ab.Position.Y = +height * 0.5f; ab.Position.Z = 0;
            ba.Position.X = +width * 0.5f; ba.Position.Y = -height * 0.5f; ba.Position.Z = 0;
            bb.Position.X = -width * 0.5f; bb.Position.Y = -height * 0.5f; bb.Position.Z = 0;

#if XBOX || WINDOWS_PHONE
            aa.Position = new Vector3();
            ab.Position = new Vector3();
            ba.Position = new Vector3();
            bb.Position = new Vector3();
#endif
            Vector3.Transform(ref aa.Position, ref transform, out aa.Position);
            Vector3.Transform(ref ab.Position, ref transform, out ab.Position);
            Vector3.Transform(ref ba.Position, ref transform, out ba.Position);
            Vector3.Transform(ref bb.Position, ref transform, out bb.Position);

            if (textureTransform != null)
            {
                aa.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.Zero);
                ab.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitX);
                ba.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitY);
                bb.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.One);
            }
            else
            {
                aa.TextureCoordinate = Vector2.Zero;
                ab.TextureCoordinate = Vector2.UnitX;
                ba.TextureCoordinate = Vector2.UnitY;
                bb.TextureCoordinate = Vector2.One;
            }

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;

            BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                AddIndex((ushort)(0));
                AddIndex((ushort)(1));
                AddIndex((ushort)(2));
                AddIndex((ushort)(1));
                AddIndex((ushort)(3));
                AddIndex((ushort)(2));

                AddVertex(aa);
                AddVertex(ab);
                AddVertex(ba);
                AddVertex(bb);
            }
            EndPrimitive();
        }

        public void DrawLine(Vector3 v1, Vector3 v2, Color color)
        {
            DrawLine(v1, v2, null, color);
        }

        public void DrawLine(Vector3 v1, Vector3 v2, Matrix? world, Color color)
        {
            BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                AddVertex(new VertexPositionColorTexture() { Position = v1, Color = color });
                AddVertex(new VertexPositionColorTexture() { Position = v2, Color = color });
            }
            EndPrimitive();
        }
        
        public void DrawLine(IEnumerable<Vector3> lineStrip, Matrix? world, Color color)
        {
            BeginPrimitive(PrimitiveType.LineList, null, world);
            {
                foreach (Vector3 position in lineStrip)
                {
                    AddVertex(new VertexPositionColorTexture() { Position = position, Color = color });
                }
            }
            EndPrimitive();
        }

        public void DrawConstrainedBillboard(Texture2D texture, Vector3 start, Vector3 end, float width, Matrix? textureTransform, Matrix? world, Color color)
        {
            //      aa --- ab
            //       |     |
            //       |     |
            //      ba --- bb
            VertexPositionColorTexture aa;
            VertexPositionColorTexture ab;
            VertexPositionColorTexture ba;
            VertexPositionColorTexture bb;

            CreateBillboard(start, end, width, out aa.Position, out ab.Position,
                                               out ba.Position, out bb.Position);

            if (textureTransform != null)
            {
                aa.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.One);
                ab.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitX);
                ba.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitY);
                bb.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.Zero);
            }
            else
            {
                aa.TextureCoordinate = Vector2.One;
                ab.TextureCoordinate = Vector2.UnitX;
                ba.TextureCoordinate = Vector2.UnitY;
                bb.TextureCoordinate = Vector2.Zero;
            }

            aa.Color = ab.Color =
            ba.Color = bb.Color = color;

            BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // Add new vertices and indices
                AddIndex((ushort)(0));
                AddIndex((ushort)(1));
                AddIndex((ushort)(2));
                AddIndex((ushort)(1));
                AddIndex((ushort)(3));
                AddIndex((ushort)(2));
                
                AddVertex(aa);
                AddVertex(ab);
                AddVertex(ba);
                AddVertex(bb);
            }
            EndPrimitive();
        }

        public void DrawConstrainedBillboard(Texture2D texture, IEnumerable<Vector3> lineStrip, float width, Matrix? textureTransform, Matrix? world, Color color)
        {
            if (lineStrip == null)
                throw new ArgumentNullException("lineStrip");


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


            // We want the texture to uniformly distribute on the line
            // even if each line segment may have different length.
            int i = 0;
            float totalLength = 0;
            float percentage = 0;

            IEnumerator<Vector3> enumerator = lineStrip.GetEnumerator();
            enumerator.Reset();
            enumerator.MoveNext();
            Vector3 previous = enumerator.Current;
            while (enumerator.MoveNext())
            {
                totalLength += Vector3.Subtract(enumerator.Current, previous).Length();
                previous = enumerator.Current;
            }
            
            BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                enumerator.Reset();
                enumerator.MoveNext();

                int vertexCount = 0;
                Vector3 start = enumerator.Current;
                Vector3 lastSegment1 = Vector3.Zero;
                Vector3 lastSegment2 = Vector3.Zero;
                
                while (enumerator.MoveNext())
                {
                    CreateBillboard(start, enumerator.Current, width, out aa.Position, out ab.Position,
                                                                      out ba.Position, out bb.Position);

                    if (textureTransform != null)
                    {
                        ba.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, new Vector2(percentage, 1));
                        bb.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, new Vector2(percentage, 0));
                    }
                    else
                    {
                        ba.TextureCoordinate = new Vector2(percentage, 1);
                        bb.TextureCoordinate = new Vector2(percentage, 0);
                    }

                    percentage += Vector3.Subtract(enumerator.Current, start).Length() / totalLength;

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

                    AddIndex((ushort)(startIndex + 0));
                    AddIndex((ushort)(startIndex + 3));
                    AddIndex((ushort)(startIndex + 1));
                    AddIndex((ushort)(startIndex + 0));
                    AddIndex((ushort)(startIndex + 2));
                    AddIndex((ushort)(startIndex + 3));

                    AddVertex(ba);
                    AddVertex(bb);

                    vertexCount += 2;

                    i++;

                    start = enumerator.Current;
                }

                // Last segment
                if (textureTransform != null)
                {
                    aa.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.One);
                    ab.TextureCoordinate = TextureTransform.Transform(textureTransform.Value, Vector2.UnitX);
                }
                else
                {
                    aa.TextureCoordinate = Vector2.One;
                    ab.TextureCoordinate = Vector2.UnitX;
                }

                AddVertex(aa);
                AddVertex(ab);
            }
            EndPrimitive();
        }

        public void Draw(IEnumerable<Vector3> vertices, IEnumerable<int> indices, Matrix? world, Color color)
        {
            BeginPrimitive(PrimitiveType.TriangleList, null, world);
            {
                foreach (Vector3 position in vertices)
                    AddVertex(new VertexPositionColorTexture() { Position = position, Color = color });

                foreach (ushort index in indices)
                    AddIndex(index);
            }
            EndPrimitive();
        }

        public void Draw(PrimitiveType primitiveType, Texture2D texture, IEnumerable<VertexPositionColorTexture> vertices, IEnumerable<ushort> indices, Matrix? world)
        {
            if (vertices == null)
                throw new ArgumentNullException("vertices");
            
            BeginPrimitive(primitiveType, texture, world);
            {
                foreach (var vertex in vertices)
                    AddVertex(vertex);

                if (indices != null)
                {
                    foreach (var index in indices)
                        AddIndex(index);
                }
            }
            EndPrimitive();
        }

        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            vertexSegments.Add(baseSegmentVertex + currentVertex);
            indexSegments.Add(baseSegmentIndex + currentIndex);

            for (int i = 0; i < batches.Count; i++)
                InternalDraw(batches[i]);

            hasBegin = false;
        }
        
        internal void BeginPrimitive(PrimitiveType primitiveType, Texture2D texture, Matrix? world)
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            currentPrimitive = new PrimitiveBatchEntry();
            currentPrimitive.World = world;
            currentPrimitive.PrimitiveType = primitiveType;
            currentPrimitive.Texture = texture;
            currentPrimitive.StartVertex = currentVertex;
            currentPrimitive.StartIndex = currentIndex;

            currentBaseVertex = currentVertex;
            currentBaseIndex = currentIndex;

            beginSegment = currentSegment;

            hasPrimitiveBegin = true;
        }

        internal void AddVertex(Vector3 position, Color color)
        {
            AddVertex(new VertexPositionColorTexture() { Position = position, Color = color });
        }

        internal void AddVertex(Vector3 position, Color color,Vector2 texCoord)
        {
            AddVertex(new VertexPositionColorTexture() { Position = position, Color = color, TextureCoordinate = texCoord });
        }

        internal void AddVertex(VertexPositionColorTexture vertex)
        {
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            if (baseSegmentVertex + currentVertex >= vertexData.Length)
                Array.Resize(ref vertexData, vertexData.Length * 2);

            if (currentVertex >= VertexBufferSize)
            {
                AdvanceSegment();
            }

            vertexData[baseSegmentVertex + currentVertex++] = vertex;
        }

        internal void AddIndex(int index)
        {
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            AddIndex((ushort)index);
        }

        internal void AddIndex(ushort index)
        {
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            if (baseSegmentIndex + currentIndex >= indexData.Length)
                Array.Resize(ref indexData, indexData.Length * 2);

            if (currentIndex >= IndexBufferSize)
            {
                AdvanceSegment();
            }

            indexData[baseSegmentIndex + currentIndex++] = (ushort)(currentBaseVertex + index);
        }

        private void AdvanceSegment()
        {
            for (int i = currentBaseIndex; i < currentIndex; i++)
            {
                indexData[baseSegmentIndex + i] -= (ushort)currentBaseVertex;
            }

            currentSegment++;

            if (currentSegment - beginSegment >= 2)
                throw new ArgumentOutOfRangeException(Strings.PrimitiveTooLarge);

            indexSegments.Add(baseSegmentIndex = baseSegmentIndex + currentBaseIndex);
            vertexSegments.Add(baseSegmentVertex = baseSegmentVertex + currentBaseVertex);

            currentVertex -= currentBaseVertex;
            currentIndex -= currentBaseIndex;

            currentBaseIndex = 0;
            currentBaseVertex = 0;
            currentPrimitive.StartVertex = 0;
            currentPrimitive.StartIndex = 0;
        }

        internal void EndPrimitive()
        {
            currentPrimitive.Segment = currentSegment;
            currentPrimitive.VertexCount = currentVertex - currentPrimitive.StartVertex;
            currentPrimitive.IndexCount = currentIndex - currentPrimitive.StartIndex;

            if (sort == PrimitiveSortMode.Deferred)
            {
                if (batches.Count > 0)
                {
                    var lastBatch = batches[batches.Count - 1];

                    if ((lastBatch.PrimitiveType == PrimitiveType.LineList ||
                         lastBatch.PrimitiveType == PrimitiveType.TriangleList) &&
                        lastBatch.PrimitiveType == currentPrimitive.PrimitiveType &&
                        lastBatch.Segment == currentPrimitive.Segment &&
                        lastBatch.Texture == currentPrimitive.Texture &&
                        lastBatch.World == currentPrimitive.World &&
                        ((lastBatch.IndexCount <= 0 && currentPrimitive.IndexCount <= 0) ||
                         (lastBatch.IndexCount > 0 && currentPrimitive.IndexCount > 0)))
                    {
                        lastBatch.IndexCount += currentPrimitive.IndexCount;
                        lastBatch.VertexCount += currentPrimitive.VertexCount;
                        batches[batches.Count - 1] = lastBatch;
                        return;
                    }
                }

                batches.Add(currentPrimitive);
            }
            else if (sort == PrimitiveSortMode.Immediate)
            {
                vertexSegments.Add(baseSegmentVertex + currentVertex);
                indexSegments.Add(baseSegmentIndex + currentIndex);

                InternalDraw(currentPrimitive);

                Flush();
            }

            hasPrimitiveBegin = false;
        }

        private void Flush()
        {
            batches.Clear();
            vertexSegments.Clear();
            indexSegments.Clear();
            vertexSegments.Add(0);
            indexSegments.Add(0);

            currentSegment = 0;
            currentIndex = 0;
            currentVertex = 0;
            baseSegmentIndex = 0;
            baseSegmentVertex = 0;
        }

        private void InternalDraw(PrimitiveBatchEntry entry)
        {
            if (entry.VertexCount <= 0 && entry.IndexCount <= 0)
                return;

            var matrices = effect as IEffectMatrices;
            if (matrices != null)
                matrices.World = entry.World.HasValue ? entry.World.Value : Matrix.Identity;

            effect.SetTexture(entry.Texture);
            effect.CurrentTechnique.Passes[0].Apply();

            // Setup state
            GraphicsDevice.BlendState = blendState;
            GraphicsDevice.DepthStencilState = depthStencilState;
            GraphicsDevice.SamplerStates[0] = samplerState;
            GraphicsDevice.RasterizerState = rasterizerState;

            // Previous segments are not used, so use SetDataOption.Discard to boost performance.
            vertexBuffer.SetData(vertexData, vertexSegments[entry.Segment], vertexSegments[entry.Segment + 1] - vertexSegments[entry.Segment], SetDataOptions.Discard);
            GraphicsDevice.SetVertexBuffer(vertexBuffer);

            if (entry.IndexCount > 0)
            {
                var primitiveCount = GetPrimitiveCount(entry.PrimitiveType, entry.IndexCount);
                indexBuffer.SetData(indexData, indexSegments[entry.Segment], indexSegments[entry.Segment + 1] - indexSegments[entry.Segment], SetDataOptions.Discard);
                GraphicsDevice.Indices = indexBuffer;
                GraphicsDevice.DrawIndexedPrimitives(entry.PrimitiveType, 0, entry.StartVertex, entry.VertexCount, entry.StartIndex, primitiveCount);
                GraphicsDevice.Indices = null;

                VertexCount += entry.VertexCount;
                PrimitiveCount = primitiveCount;
            }
            else
            {
                var primitiveCount = GetPrimitiveCount(entry.PrimitiveType, entry.VertexCount);
                GraphicsDevice.DrawPrimitives(entry.PrimitiveType, entry.StartVertex, primitiveCount);

                VertexCount += entry.VertexCount;
                PrimitiveCount = primitiveCount;
            }

            GraphicsDevice.SetVertexBuffer(null);
        }

        // NumVertsPerPrimitive is a boring helper function that tells how many vertices
        // it will take to draw each kind of primitive.
        static private int GetPrimitiveCount(PrimitiveType primitive, int indexCount)
        {
            switch (primitive)
            {
                case PrimitiveType.LineStrip:
                    return indexCount - 1;
                case PrimitiveType.LineList:
                    return indexCount / 2;
                case PrimitiveType.TriangleList:
                    return indexCount / 3;
                case PrimitiveType.TriangleStrip:
                    return indexCount - 2;
                default:
                    throw new InvalidOperationException(Strings.InvalidPrimitive);
            }
        }

        /// <summary>
        /// Matrix.CreateConstraintBillboard has a sudden change effect that is not
        /// desirable, so rolling out our own version.
        /// </summary>
        internal void CreateBillboard(Vector3 start, Vector3 end, float width,
                                               out Vector3 aa, out Vector3 ab,
                                               out Vector3 ba, out Vector3 bb)
        {
            // Compute billboard facing
            Vector3 v1 = Vector3.Subtract(end, start);
            Vector3 v2 = Vector3.Subtract(cameraPosition, start);

            Vector3 right = Vector3.Cross(v1, v2);

            right.Normalize();
            right *= width / 2;

            // Compute destination vertices
            aa = end - right;
            ab = end + right;
            ba = start - right;
            bb = start + right;
        }


        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (vertexBuffer != null)
                    vertexBuffer.Dispose();
                if (indexBuffer != null)
                    indexBuffer.Dispose();
            }
        }

        ~PrimitiveBatch()
        {
            Dispose(false);
        }
    }

    internal struct PrimitiveBatchEntry
    {
        public Matrix? World;
        public Texture2D Texture;
        public PrimitiveType PrimitiveType;
        public int StartVertex;
        public int VertexCount;
        public int StartIndex;
        public int IndexCount;
        public int Segment;
    }
}
