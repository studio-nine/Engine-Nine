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
        /// Gets a list of contained primitives
        /// </summary>
        public IList<Primitive<VertexPositionColorTexture>> Primitives
        {
            get { return primitives ?? (primitives = new List<Primitive<VertexPositionColorTexture>>()); }
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
        private List<Primitive<VertexPositionColorTexture>> primitives;
        
        private DynamicVertexBuffer vertexBuffer;
        private DynamicIndexBuffer indexBuffer;
        private VertexPositionColorNormalTexture[] vertexData;
        private ushort[] indexData;

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
                DepthBias = -0.001f,
            };
            this.material = new BasicMaterial(graphics) 
            {
                LightingEnabled = false, 
                VertexColorEnabled = true, 
                TwoSided = true,
            };

            this.DepthBias = 0.001f;
            this.vertexData = new VertexPositionColorNormalTexture[512];
            this.indexData = new ushort[6];

            Clear();
        }
        
        /// <summary>
        /// Begins a new primitive.
        /// </summary>
        public void BeginPrimitive(PrimitiveType primitiveType, Texture2D texture)
        {
            BeginPrimitive(primitiveType, texture, null, 1);
        }

        /// <summary>
        /// Begins a new primitive.
        /// </summary>
        public void BeginPrimitive(PrimitiveType primitiveType, Texture2D texture, Matrix? world)
        {
            BeginPrimitive(primitiveType, texture, null, 1);
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
            AddVertex(ref position, color, ref texCoord);
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        public void AddVertex(ref Vector3 position, Color color, ref Vector2 texCoord)
        {
            VertexPositionColorTexture vertex = new VertexPositionColorTexture();
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
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineList)
            {
                ExpandLineVertex(ref vertex, true);
                return;
            }
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineStrip)
            {
                ExpandLineVertex(ref vertex, false);
                return;
            }
            Vector3 zero = new Vector3();
            AddVertexInternal(ref vertex, ref zero);
        }

        /// <summary>
        /// Expands a line list to triangle strips.
        /// </summary>
        private void ExpandLineVertex(ref VertexPositionColorTexture vertex, bool isLineList)
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
                vertexData[lastLineVertex].Normal = vertex.Position;
                vertexData[lastLineVertex + 1].Normal =
                    vertexData[lastLineVertex + 1].Position * 2 - vertex.Position;

                normal1 = v1.Position * 2 - vertexData[lastLineVertex].Position;
                normal2 = vertexData[lastLineVertex + 1].Position;
                if (isLineList)
                    storeIndex = false;
                    lastLineVertex = -1;
            }

            var last = AddVertexInternal(ref v1, ref normal1);
            AddVertexInternal(ref v2, ref normal2);
            lastLineVertex = storeIndex ? last : -1;
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
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineList)
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
                return;
            }
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineStrip)
            {
                throw new NotImplementedException();
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
            if (currentPrimitive.PrimitiveType == PrimitiveType.LineList)
            {
                if (currentPrimitive.IndexCount <= 0)
                    for (var index = 0; index < (currentVertex - currentPrimitive.StartVertex) / 2; ++index)
                        AddIndex(index);
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

            if (material == null || material == this.material)
            {
                var count = batches.Count;
                if (count > 0)
                {
                    var previousRasterizerState = graphics.RasterizerState;
                    graphics.RasterizerState = rasterizerState;
                    for (int i = 0; i < count; ++i)
                        DrawBatch(context, ref batches.Elements[i]);
                    graphics.RasterizerState = previousRasterizerState;
                }
            }
        }

        private void DrawBatch(DrawingContext context, ref PrimitiveGroupEntry entry)
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
            if (vertexBuffer == null || vertexBuffer.VertexCount < vertexBufferSize || vertexBuffer.IsContentLost)
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
                if (indexBuffer == null || indexBuffer.IndexCount < indexBufferSize || indexBuffer.IsContentLost)
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

        void IDrawableObject.BeginDraw(DrawingContext context) { }
        void IDrawableObject.EndDraw(DrawingContext context) { }
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
