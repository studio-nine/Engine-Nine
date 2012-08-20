namespace Nine.Graphics.Primitives
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

    /// <summary>
    /// Enables a group of dynamic primitives to be drawn.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("Primitives")]    
    public class DynamicPrimitive : Nine.Object, IDrawableObject
    {
        /// <summary>
        /// Gets the underlying graphics device.
        /// </summary>
        public GraphicsDevice GraphicsDevice
        {
            get { return graphics; }
        }

        /// <summary>
        /// Gets or sets whether this object is visible.
        /// </summary>
        public bool Visible { get; set; }

        /// <summary>
        /// Gets or sets the depth bias of this dynamic primitive.
        /// </summary>
        public float DepthBias
        {
            get { return -rasterizerState.DepthBias; }
            set { rasterizerState.DepthBias = -value; }
        }

        /// <summary>
        /// Gets the material of the object.
        /// </summary>
        Material IDrawableObject.Material
        {
            get { return material; }
        }

        /// <summary>
        /// Provides an optimization hint to skip comparison between primitives.
        /// </summary>
        internal bool AlwaysMergePrimitives = false;

        private bool hasPrimitiveBegin = false;

        private GraphicsDevice graphics;
        private Material material;
        private ThickLineMaterial thickLineMaterial;
        private RasterizerState rasterizerState;
        
        private DynamicVertexBuffer vertexBuffer;
        private DynamicIndexBuffer indexBuffer;
        private VertexPositionColorNormalTexture[] vertexData;
        private ushort[] indexData;

        private VertexPositionColorTexture[] lineVertices;
        private FastList<int> lineIndices = new FastList<int>();
        private FastList<PrimitiveGroupEntry> batches = new FastList<PrimitiveGroupEntry>();
        private FastList<int> vertexSegments = new FastList<int>();
        private FastList<int> indexSegments = new FastList<int>();

        private PrimitiveGroupEntry currentPrimitive;
        private int currentSegment;
        private int currentVertex;
        private int currentIndex;
        private int currentBaseVertex;
        private int currentBaseIndex;
        private int baseSegmentVertex;
        private int baseSegmentIndex;
        private int beginSegment;

        private int currentLineVertex;
        private int lastLineVertex;

        private int initialBufferCapacity;
        private int maxBufferSizePerPrimitive;

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPrimitive"/> class.
        /// </summary>
        public DynamicPrimitive(GraphicsDevice graphics) : this(graphics, 32, 32768)
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicPrimitive"/> class.
        /// </summary>
        public DynamicPrimitive(GraphicsDevice graphics, int initialBufferCapacity, int maxBufferSizePerPrimitive)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            this.Visible = true;
            this.graphics = graphics;
            this.initialBufferCapacity = initialBufferCapacity;
            this.maxBufferSizePerPrimitive = maxBufferSizePerPrimitive;
            this.rasterizerState = new RasterizerState()
            {
                CullMode = CullMode.None,
                DepthBias = 0f,
            };
            this.material = new BasicMaterial(graphics) 
            {
                LightingEnabled = false, 
                VertexColorEnabled = true, 
                TwoSided = true,
            };

            this.vertexData = new VertexPositionColorNormalTexture[512];
            this.indexData = new ushort[6];

            Clear();
        }
        
        /// <summary>
        /// Begins a new primitive.
        /// </summary>
        public void BeginPrimitive(PrimitiveType primitiveType, Texture2D texture)
        {
            BeginPrimitive(primitiveType, texture, null, 0);
        }

        /// <summary>
        /// Begins a new primitive.
        /// </summary>
        public void BeginPrimitive(PrimitiveType primitiveType, Texture2D texture, Matrix? world)
        {
            BeginPrimitive(primitiveType, texture, world, 0);
        }

        /// <summary>
        /// Begins a new primitive.
        /// </summary>
        public void BeginPrimitive(PrimitiveType primitiveType, Texture2D texture, Matrix? world, float lineWidth)
        {
            if (hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);
            
            hasPrimitiveBegin = true;

            currentPrimitive = new PrimitiveGroupEntry();
            currentPrimitive.LineWidth = lineWidth;
            currentPrimitive.World = world;
            currentPrimitive.PrimitiveType = primitiveType;
            currentPrimitive.Texture = texture;
            currentPrimitive.StartVertex = currentVertex;
            currentPrimitive.StartIndex = currentIndex;

            currentBaseVertex = currentVertex; 
            currentBaseIndex = currentIndex;

            beginSegment = currentSegment;

            lineIndices.Clear();
            currentLineVertex = 0;
            lastLineVertex = -1;
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        public void AddVertex(Vector3 position, Color color)
        {
            AddVertex(ref position, color);
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        public void AddVertex(ref Vector3 position, Color color)
        {
            VertexPositionColorTexture vertex = new VertexPositionColorTexture();
            vertex.Position = position;
            vertex.Color = color;
            AddVertex(ref vertex);
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        public void AddVertex(Vector3 position, Color color, Vector2 texCoord)
        {
            AddVertex(ref position, ref texCoord, color);
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        public void AddVertex(ref Vector3 position, ref Vector2 texCoord, Color color)
        {
            var vertex = new VertexPositionColorTexture();
            vertex.Position = position;
            vertex.Color = color;
            vertex.TextureCoordinate = texCoord;
            AddVertex(ref vertex);
        }
        
        /// <summary>
        /// Adds the vertex.
        /// </summary>
        public void AddVertex(ref VertexPositionColorTexture vertex)
        {
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineList ||
                currentPrimitive.PrimitiveType == PrimitiveType.LineStrip)
            {
                if (lineVertices == null)
                    lineVertices = new VertexPositionColorTexture[64];
                if (lineVertices.Length <= currentLineVertex)
                    Array.Resize(ref lineVertices, lineVertices.Length * 2);
                lineVertices[currentLineVertex++] = vertex;
                return;
            }

            Vector3 zero = new Vector3();
            AddVertexInternal(ref vertex, ref zero);
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        private int AddVertexInternal(ref VertexPositionColorTexture vertex, ref Vector3 normal)
        {
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            var index = baseSegmentVertex + currentVertex;
            if (index >= vertexData.Length)
                Array.Resize(ref vertexData, vertexData.Length * 2);

            if (currentVertex >= maxBufferSizePerPrimitive)
                AdvanceSegment();

            vertexData[index].Position = vertex.Position;
            vertexData[index].Color = vertex.Color;
            vertexData[index].Normal = normal;
            vertexData[index].TextureCoordinate = vertex.TextureCoordinate;
            currentVertex++;
            return index;
        }
        /// <summary>
        /// Adds the index.
        /// </summary>
        public void AddIndex(int index)
        {
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineList ||
                currentPrimitive.PrimitiveType == PrimitiveType.LineStrip)
            {
                lineIndices.Add(index);
                return;
            }

            AddIndexInternal(index);
        }

        /// <summary>
        /// Adds the index.
        /// </summary>
        private void AddIndexInternal(int index)
        {
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);        
            if (index > ushort.MaxValue)
                throw new ArgumentOutOfRangeException("index");

            if (baseSegmentIndex + currentIndex >= indexData.Length)
                Array.Resize(ref indexData, indexData.Length * 2);

            if (currentIndex >= maxBufferSizePerPrimitive)
                AdvanceSegment();

            indexData[baseSegmentIndex + currentIndex++] = (ushort)(currentBaseVertex + index);
        }

        private void AdvanceSegment()
        {
            for (int i = currentBaseIndex; i < currentIndex; ++i)
                indexData[baseSegmentIndex + i] -= (ushort)currentBaseVertex;

            currentSegment++;

            if (currentSegment - beginSegment >= 2)
                throw new ArgumentOutOfRangeException(Strings.PrimitiveTooLarge);

            indexSegments.Add(baseSegmentIndex += currentBaseIndex);
            vertexSegments.Add(baseSegmentVertex += currentBaseVertex);

            currentVertex -= currentBaseVertex;
            currentIndex -= currentBaseIndex;

            currentBaseIndex = 0;
            currentBaseVertex = 0;
            currentPrimitive.StartVertex = 0;
            currentPrimitive.StartIndex = 0;
        }

        /// <summary>
        /// Ends the primitive.
        /// </summary>
        public void EndPrimitive()
        {
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineList ||
                currentPrimitive.PrimitiveType == PrimitiveType.LineStrip)
            {
                ExpandLineVertices(currentPrimitive.PrimitiveType == PrimitiveType.LineList);
                for (var index = 0; index < (currentVertex - currentPrimitive.StartVertex) / 2; ++index)
                {
                    if (index % 2 == 0)
                    {
                        AddIndexInternal(index * 2);
                        AddIndexInternal(index * 2 + 1);
                        AddIndexInternal(index * 2 + 2);

                        AddIndexInternal(index * 2 + 2);
                        AddIndexInternal(index * 2 + 1);
                        AddIndexInternal(index * 2 + 3);
                    }
                }
                currentPrimitive.PrimitiveType = PrimitiveType.TriangleList;
            }

            hasPrimitiveBegin = false;
            currentPrimitive.Segment = currentSegment;
            currentPrimitive.VertexCount = currentVertex - currentPrimitive.StartVertex;
            currentPrimitive.IndexCount = currentIndex - currentPrimitive.StartIndex;

            vertexSegments[vertexSegments.Count - 1] = baseSegmentVertex + currentVertex;
            indexSegments[indexSegments.Count - 1] = baseSegmentIndex + currentIndex;

            var i = batches.Count - 1;
            if (batches.Count > 0 && (AlwaysMergePrimitives || CanMerge(ref batches.Elements[i], ref currentPrimitive)) 
                                  && batches.Elements[i].Segment == currentPrimitive.Segment)
            {
                batches.Elements[i].IndexCount += currentPrimitive.IndexCount;
                batches.Elements[i].VertexCount += currentPrimitive.VertexCount;
                return;
            }

            batches.Add(currentPrimitive);
        }

        /// <summary>
        /// Copy the line vertices to the real vertex data buffer.
        /// </summary>
        private void ExpandLineVertices(bool isLineList)
        {
            if (lineIndices.Count > 0)
            {
                // Need create a new vertex even when the two index points
                // to the same vertex. Because even if AB and BC shares 
                // vertex B, the pointing direction is different when
                // expanding B for line AB and BC.
                if (!isLineList)
                    throw new NotSupportedException("Line strip does not support indexing.");

                for (var x = 0; x < lineIndices.Count; ++x)
                {
                    var xx = lineIndices[x];
                    if (xx >= currentLineVertex)
                    {
                        throw new ArgumentOutOfRangeException(
                            string.Format("Cannot find a vertex from index {0}", xx));
                    }
                    ExpandLineVertex(ref lineVertices[xx]);
                }
            }
            else if (isLineList)
            {
                for (var x = 0; x < currentLineVertex; ++x)
                    ExpandLineVertex(ref lineVertices[x]);
            }
            else
            {
                for (var x = 0; x < currentLineVertex - 1; ++x)
                {
                    ExpandLineVertex(ref lineVertices[x]);
                    ExpandLineVertex(ref lineVertices[x + 1]);
                }
            }
        }

        /// <summary>
        /// Expands a line list to triangle strips.
        /// </summary>
        private void ExpandLineVertex(ref VertexPositionColorTexture vertex)
        {
            var normal1 = new Vector3();
            var normal2 = new Vector3();

            var v1 = new VertexPositionColorTexture();
            var v2 = new VertexPositionColorTexture();

            v1.Position = v2.Position = vertex.Position;
            v1.Color = v2.Color = vertex.Color;

            var storeIndex = true;
            if (lastLineVertex >= 0)
            {
                vertexData[lastLineVertex].Normal = vertexData[lastLineVertex + 1].Normal = vertex.Position;
                vertexData[lastLineVertex].TextureCoordinate.X = 0;
                vertexData[lastLineVertex + 1].TextureCoordinate.X = 1;

                normal1 = normal2 = vertexData[lastLineVertex + 1].Position;
                v1.TextureCoordinate.X = 1;
                v2.TextureCoordinate.X = 0;

                storeIndex = false;
            }

            var last = AddVertexInternal(ref v1, ref normal1);
            AddVertexInternal(ref v2, ref normal2);
            lastLineVertex = storeIndex ? last : -1;
        }

        /// <summary>
        /// Determines if two batches can be merged into a single batch.
        /// </summary>
        private static bool CanMerge(ref PrimitiveGroupEntry oldBatch, ref PrimitiveGroupEntry newBatch)
        {
            if (oldBatch.PrimitiveType != newBatch.PrimitiveType)
                return false;
            if (oldBatch.LineWidth != newBatch.LineWidth)
                return false;
            if (oldBatch.PrimitiveType != PrimitiveType.LineList && oldBatch.PrimitiveType != PrimitiveType.TriangleList)
                return false;
            if (oldBatch.Texture != newBatch.Texture)
                return false;
            if (oldBatch.World.HasValue || newBatch.World.HasValue)
                return false;
            if ((oldBatch.IndexCount > 0 && newBatch.IndexCount == 0) || (oldBatch.IndexCount == 0 && newBatch.IndexCount > 0))
                return false;
            return true;
        }

        /// <summary>
        /// Clears this instance.
        /// </summary>
        public void Clear()
        {
            batches.Clear();
            vertexSegments.Clear();
            indexSegments.Clear();

            vertexSegments.Add(0);
            indexSegments.Add(0);

            vertexSegments.Add(0);
            indexSegments.Add(0);

            currentSegment = 0;
            currentIndex = 0;
            currentVertex = 0;
            baseSegmentIndex = 0;
            baseSegmentVertex = 0;
        }

        /// <summary>
        /// Draws this instance.
        /// </summary>
        public void Draw(DrawingContext context, Material material)
        {
            if (hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            if (material == null)
                material = this.material;

            var count = batches.Count;
            if (count > 0)
            {
                var previousRasterizerState = graphics.RasterizerState;
                graphics.RasterizerState = rasterizerState;
                for (int i = 0; i < count; ++i)
                    DrawBatch(context, ref batches.Elements[i], material);
                graphics.RasterizerState = previousRasterizerState;
            }
        }

        private void DrawBatch(DrawingContext context, ref PrimitiveGroupEntry entry, Material material)
        {
            if (entry.VertexCount <= 0 && entry.IndexCount <= 0)
                return;

            material.texture = entry.Texture;
            material.world = entry.World.HasValue ? entry.World.Value : Matrix.Identity;
            material.BeginApply(context);

            if (entry.LineWidth > 0)
            {
                if (thickLineMaterial == null)
                    thickLineMaterial = new ThickLineMaterial(graphics);
                thickLineMaterial.Thickness = entry.LineWidth;
                thickLineMaterial.world = material.world;
                thickLineMaterial.BeginApply(context);
            }

            var vertexBufferSize = vertexSegments[entry.Segment + 1] - vertexSegments[entry.Segment];
#if SILVERLIGHT
            if (vertexBuffer == null || vertexBuffer.VertexCount < vertexBufferSize)
#else
            if (vertexBuffer == null || vertexBuffer.VertexCount < vertexBufferSize || vertexBuffer.IsContentLost)
#endif
            {
                if (vertexBuffer != null)
                    vertexBuffer.Dispose();
                vertexBuffer = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorNormalTexture), Math.Max(initialBufferCapacity, vertexBufferSize), BufferUsage.WriteOnly);
            }

            context.SetVertexBuffer(null, 0);
            vertexBuffer.SetData(vertexData, vertexSegments[entry.Segment], vertexBufferSize, SetDataOptions.Discard);

            // Previous segments are not used, so use SetDataOption.Discard to boost performance.            
            context.SetVertexBuffer(vertexBuffer, 0);

            if (entry.IndexCount > 0)
            {
                var indexBufferSize = indexSegments[entry.Segment + 1] - indexSegments[entry.Segment];
#if SILVERLIGHT
                if (indexBuffer == null || indexBuffer.IndexCount < indexBufferSize)
#else
                if (indexBuffer == null || indexBuffer.IndexCount < indexBufferSize || indexBuffer.IsContentLost)
#endif
                {
                    if (indexBuffer != null)
                        indexBuffer.Dispose();
                    indexBuffer = new DynamicIndexBuffer(graphics, typeof(ushort), Math.Max(initialBufferCapacity, indexBufferSize), BufferUsage.WriteOnly);
                }

                graphics.Indices = null;
                indexBuffer.SetData(indexData, indexSegments[entry.Segment], indexBufferSize, SetDataOptions.Discard);

                var primitiveCount = Helper.GetPrimitiveCount(entry.PrimitiveType, entry.IndexCount);
                graphics.Indices = indexBuffer;
                graphics.DrawIndexedPrimitives(entry.PrimitiveType, 0, entry.StartVertex, entry.VertexCount, entry.StartIndex, primitiveCount);
            }
            else
            {
                var primitiveCount = Helper.GetPrimitiveCount(entry.PrimitiveType, entry.VertexCount);
                graphics.DrawPrimitives(entry.PrimitiveType, entry.StartVertex, primitiveCount);
            }

            material.EndApply(context);
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

        ~DynamicPrimitive()
        {
            Dispose(false);
        }

        void IDrawableObject.OnAddedToView(DrawingContext context) { }
    }

    struct PrimitiveGroupEntry
    {
        public Matrix? World;
        public Texture2D Texture;
        public float LineWidth;
        public PrimitiveType PrimitiveType;
        public int StartVertex;
        public int VertexCount;
        public int StartIndex;
        public int IndexCount;
        public int Segment;
    }
}
