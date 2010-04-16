#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics.Landscape
{
    #region TerrainPatchPart
    /// <summary>
    /// A terrain patch part is 8 triangles that makes up the following shape:
    ///  ____ ____
    /// |0 / | \ 3|
    /// |_/_1|2_\_|
    /// | \ 5|6 / |
    /// |4_\_|_/_7|
    /// </summary>
    public sealed class TerrainPatchPart
    {
        /// <summary>
        /// Whether each triangle is visible.
        /// </summary>
        public byte Mask { get; set; }

        /// <summary>
        /// Center position of this patch part.
        /// </summary>
        public Vector3 Position { get; internal set; }

        /// <summary>
        /// Constructor is for internal use only.
        /// </summary>
        internal TerrainPatchPart() { }

        #region Indices
        internal IEnumerable<Point> Indices
        {
            get 
            {
                if ((Mask > 0) & 0x01 == 1)
                {
                    yield return points[0];
                    yield return points[1];
                    yield return points[3];
                }
                if ((Mask > 1) & 0x01 == 1)
                {
                    yield return points[3];
                    yield return points[1];
                    yield return points[4];
                }
                if ((Mask > 2) & 0x01 == 1)
                {
                    yield return points[1];
                    yield return points[5];
                    yield return points[4];
                }
                if ((Mask > 3) & 0x01 == 1)
                {
                    yield return points[1];
                    yield return points[2];
                    yield return points[5];
                }
                if ((Mask > 4) & 0x01 == 1)
                {
                    yield return points[3];
                    yield return points[7];
                    yield return points[6];
                }
                if ((Mask > 5) & 0x01 == 1)
                {
                    yield return points[3];
                    yield return points[4];
                    yield return points[7];
                }
                if ((Mask > 6) & 0x01 == 1)
                {
                    yield return points[4];
                    yield return points[5];
                    yield return points[7];
                }
                if ((Mask > 7) & 0x01 == 1)
                {
                    yield return points[5];
                    yield return points[8];
                    yield return points[7];
                }
            }
        }

        private static Point[] points = new Point[]
        {
            new Point(0, 0), new Point(1, 0), new Point(2, 0),
            new Point(0, 1), new Point(1, 1), new Point(2, 1),
            new Point(0, 2), new Point(1, 2), new Point(2, 2),
        };
        #endregion
    }
    #endregion


    #region TerrainPatchPart
    /// <summary>
    /// A square block made up of terrain patch parts.
    /// </summary>
    public sealed class TerrainPatch : IDisposable
    {
        public int Tessellation { get; internal set; }

        public VertexBuffer VertexBuffer { get; private set; }

        public IndexBuffer IndexBuffer { get; private set; }

        public VertexDeclaration VertexDeclaration { get; private set; }

        public int PrimitiveCount { get; private set; }

        public int VertexCount { get { return (Tessellation + 1) * (Tessellation + 1); } }

        public ReadOnlyCollection<TerrainPatchPart> PatchParts { get; private set; }

        public Collection<Effect> Effects { get; internal set; }

        public GraphicsDevice GraphicsDevice { get; private set; }

        public Matrix Transform { get { return Matrix.CreateTranslation(position); } }

        public object Tag { get; set; }

        #region BoundingBox & Position
        public BoundingBox BoundingBox
        {
            get
            {
                BoundingBox box;

                box.Min = boundingBox.Min + position;
                box.Max = boundingBox.Max + position;

                return box;
            }
        }

        public Vector3 Position
        {
            get 
            {
                Vector3 v;
                
                int xPatchCount = geometry.TessellationU / Tessellation;
                int yPatchCount = geometry.TessellationV / Tessellation;
                
                v.X = (xPatch + 0.5f - xPatchCount * 0.5f) * geometry.Step * Tessellation + position.X;
                v.Y = (yPatch + 0.5f - yPatchCount * 0.5f) * geometry.Step * Tessellation + position.Y;
                v.Z = position.Z;

                return v;
            }

            internal set { position = value; UpdatePartPositions(); }
        }

        private int xPatch, yPatch;
        private TerrainGeometry geometry;
        private BoundingBox boundingBox;
        private Vector3 position;
        internal Vector2 TextureScale = Vector2.One;
        #endregion

        /// <summary>
        /// Constructor is for internal use only.
        /// </summary>
        internal TerrainPatch(GraphicsDevice graphics, TerrainGeometry geometry, int xPatch, int yPatch, int tessellation)
        {
            if (6 * tessellation * tessellation > ushort.MaxValue)
                throw new ArgumentOutOfRangeException();

            if (tessellation < 2 || tessellation % 2 == 1)
                throw new ArgumentOutOfRangeException("Patch tessellation must be even.");


            this.GraphicsDevice = graphics;
            this.geometry = geometry;
            this.Tessellation = tessellation;
            this.xPatch = xPatch;
            this.yPatch = yPatch;
            this.Effects = new Collection<Effect>();


            // Initialize vertices and indices
            VertexDeclaration = new VertexDeclaration(graphics,
                                            Vertices.VertexPositionNormalTangentTexture.VertexElements);

            VertexBuffer = new VertexBuffer(graphics,
                                            typeof(Vertices.VertexPositionNormalTangentTexture),
                                            VertexCount, BufferUsage.WriteOnly);

            IndexBuffer = new IndexBuffer(graphics,
                                            typeof(ushort), 6 * tessellation * tessellation, BufferUsage.WriteOnly);

            
            // Initialize patch parts
            TerrainPatchPart[] parts = new TerrainPatchPart[tessellation * tessellation / 4];

            for (int i = 0; i < parts.Length; i++)
            {
                TerrainPatchPart part = new TerrainPatchPart();

                part.Mask = 0xFF;

                parts[i] = part;
            }

            PatchParts = new ReadOnlyCollection<TerrainPatchPart>(parts);


            // Fill vertices and indices
            Invalidate();
        }

        private static Vertices.VertexPositionNormalTangentTexture[] vertexPool;
        private static ushort[] indexPool;


        internal void Invalidate()
        {
            // Reuse the array to avoid creating large chunk of data.
            int indexCount = 6 * Tessellation * Tessellation;

            if (vertexPool == null || vertexPool.Length < VertexCount)
                vertexPool = new Vertices.VertexPositionNormalTangentTexture[VertexCount];
            if (indexPool == null || indexPool.Length < indexCount)
                indexPool = new ushort[indexCount];


            // Fill vertices
            int i = 0;

            for (int x = xPatch * Tessellation; x <= (xPatch + 1) * Tessellation; x++)
            {
                for (int y = yPatch * Tessellation; y <= (yPatch + 1) * Tessellation; y++)
                {
                    vertexPool[i].Position = geometry.GetPosition(x, y);
                    vertexPool[i].Normal = geometry.Normals[geometry.GetIndex(x, y)];
                    vertexPool[i].Tangent = geometry.Tangents[geometry.GetIndex(x, y)];
                    vertexPool[i].TextureCoordinate.X = 1.0f * x / Tessellation / TextureScale.X;
                    vertexPool[i].TextureCoordinate.Y = 1.0f * y / Tessellation / TextureScale.Y;

                    i++;
                }
            }

            VertexBuffer.SetData<Vertices.VertexPositionNormalTangentTexture>(vertexPool, 0, VertexCount);

            
            // Fill indices
            i = 0;
            PrimitiveCount = 0;
            int part = Tessellation / 2;

            for (int y = 0; y < part; y++)
            {
                for (int x = 0; x < part; x++)
                {
                    foreach (Point pt in PatchParts[i++].Indices)
                    {
                        indexPool[PrimitiveCount++] = (ushort)(pt.X + x * 2 + (pt.Y + y * 2) * (Tessellation + 1));
                    }
                }
            }

            IndexBuffer.SetData<ushort>(indexPool, 0, PrimitiveCount);

            PrimitiveCount /= 3;

            
            // Compute bounding box
            boundingBox = BoundingBox.CreateFromPoints(EnumeratePositions());


            UpdatePartPositions();
        }

        private IEnumerable<Vector3> EnumeratePositions()
        {
            for (int i = 0; i < VertexCount; i++)
                yield return vertexPool[i].Position;
        }
        
        private void UpdatePartPositions()
        {
            int i = 0;
            int part = Tessellation / 2;

            for (int y = 0; y < part; y++)
            {
                for (int x = 0; x < part; x++)
                {
                    PatchParts[i++].Position = new Vector3(
                        (x + 0.5f - part * 0.5f) * geometry.Step * 2 + Position.X,
                        (y + 0.5f - part * 0.5f) * geometry.Step * 2 + Position.Y, Position.Z);
                }
            }
        }

        public void Draw()
        {
            if (Effects.Count <= 0 || PrimitiveCount <= 0)
                return;            

            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.VertexDeclaration = VertexDeclaration;
            GraphicsDevice.Vertices[0].SetSource(VertexBuffer, 0, Vertices.VertexPositionNormalTangentTexture.SizeInBytes);

            foreach (Effect effect in Effects)
            {
                effect.Begin();

                foreach (EffectPass pass in effect.CurrentTechnique.Passes)
                {
                    pass.Begin();

                    GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);

                    pass.End();
                }

                effect.End();
            }
        }

        public void Draw(Effect effect)
        {
            if (effect == null || PrimitiveCount <= 0)
                return;

            GraphicsDevice.Indices = IndexBuffer;
            GraphicsDevice.VertexDeclaration = VertexDeclaration;
            GraphicsDevice.Vertices[0].SetSource(VertexBuffer, 0, Vertices.VertexPositionNormalTangentTexture.SizeInBytes);

            effect.Begin();

            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();

                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, VertexCount, 0, PrimitiveCount);

                pass.End();
            }

            effect.End();
        }

        public void Dispose()
        {
            if (VertexBuffer != null)
                VertexBuffer.Dispose();

            if (IndexBuffer != null)
                IndexBuffer.Dispose();

            if (VertexDeclaration != null)
                VertexDeclaration.Dispose();
        }
    }
    #endregion
}
