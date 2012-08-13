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
        /// Gets or sets the material of the object.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets a list of contained primitives
        /// </summary>
        public IList<Primitive<VertexPositionColorTexture>> Primitives
        {
            get { return primitives ?? (primitives = new List<Primitive<VertexPositionColorTexture>>()); }
        }

        /// <summary>
        /// Provides an optimization hint to skip comparison between primitives.
        /// </summary>
        internal bool AlwaysMergePrimitives = false;

        private bool hasPrimitiveBegin = false;

        private GraphicsDevice graphics;
        private List<Primitive<VertexPositionColorTexture>> primitives;
        
        private DynamicVertexBuffer vertexBuffer;
        private DynamicIndexBuffer indexBuffer;
        private VertexPositionColorTexture[] vertexData;
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
            this.Material = new BasicMaterial(graphics) { LightingEnabled = false, VertexColorEnabled = true };

            this.vertexData = new VertexPositionColorTexture[4];
            this.indexData = new ushort[6];

            Clear();
        }

        /// <summary>
        /// Begins a new primitive.
        /// </summary>
        public void BeginPrimitive(PrimitiveType primitiveType, Texture2D texture, Matrix? world)
        {
            if (hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);
            hasPrimitiveBegin = true;

            currentPrimitive = new PrimitiveGroupEntry();
            currentPrimitive.World = world;
            currentPrimitive.PrimitiveType = primitiveType;
            currentPrimitive.Texture = texture;
            currentPrimitive.StartVertex = currentVertex;
            currentPrimitive.StartIndex = currentIndex;

            currentBaseVertex = currentVertex;
            currentBaseIndex = currentIndex;

            beginSegment = currentSegment;
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
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            var index = baseSegmentVertex + currentVertex;
            if (index >= vertexData.Length)
                Array.Resize(ref vertexData, vertexData.Length * 2);

            if (currentVertex >= maxBufferSizePerPrimitive)
                AdvanceSegment();

            vertexData[index].Position = position;
            vertexData[index].Color = color;
            vertexData[index].TextureCoordinate = Vector2.Zero;
            currentVertex++;
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
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            var index = baseSegmentVertex + currentVertex;
            if (index >= vertexData.Length)
                Array.Resize(ref vertexData, vertexData.Length * 2);

            if (currentVertex >= maxBufferSizePerPrimitive)
                AdvanceSegment();

            vertexData[index].Position = position;
            vertexData[index].Color = color;
            vertexData[index].TextureCoordinate = texCoord;
            currentVertex++;
        }

        /// <summary>
        /// Adds the vertex.
        /// </summary>
        public void AddVertex(ref VertexPositionColorTexture vertex)
        {
            if (!hasPrimitiveBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            var index = baseSegmentVertex + currentVertex;
            if (index >= vertexData.Length)
                Array.Resize(ref vertexData, vertexData.Length * 2);

            if (currentVertex >= maxBufferSizePerPrimitive)
                AdvanceSegment();

            vertexData[index] = vertex;
            currentVertex++;
        }

        /// <summary>
        /// Adds the index.
        /// </summary>
        public void AddIndex(int index)
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
                material = this.Material;
            var count = batches.Count;
            for (int i = 0; i < count; ++i)
                DrawBatch(context, ref batches.Elements[i], material);
        }

        private void DrawBatch(DrawingContext context, ref PrimitiveGroupEntry entry, Material material)
        {
            if (entry.VertexCount <= 0 && entry.IndexCount <= 0)
                return;

            material.texture = entry.Texture;
            material.World = entry.World.HasValue ? entry.World.Value : Matrix.Identity;
            material.BeginApply(context);

            var vertexBufferSize = vertexSegments[entry.Segment + 1] - vertexSegments[entry.Segment];
            if (vertexBuffer == null || vertexBuffer.VertexCount < vertexBufferSize || vertexBuffer.IsContentLost)
            {
                if (vertexBuffer != null)
                    vertexBuffer.Dispose();
                vertexBuffer = new DynamicVertexBuffer(graphics, typeof(VertexPositionColorTexture), Math.Max(initialBufferCapacity, vertexBufferSize), BufferUsage.WriteOnly);
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
        public PrimitiveType PrimitiveType;
        public int StartVertex;
        public int VertexCount;
        public int StartIndex;
        public int IndexCount;
        public int Segment;
    }
}
