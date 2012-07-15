#region File Description
//-----------------------------------------------------------------------------
// SpherePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
#endregion

namespace Nine.Graphics.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing two perpendicular planes.
    /// </summary>
    [ContentSerializable]
    public class Cross : Primitive<VertexPositionNormalTexture>
    {
        /// <summary>
        /// Gets or sets the tessellation this primitive.
        /// </summary>
        public int Tessellation
        {
            get { return tessellation; }
            set
            {
                if (tessellation != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("Tessellation");
                    tessellation = value;
                    Invalidate();
                }
            }
        }
        private int tessellation = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Cross"/> class.
        /// </summary>
        public Cross(GraphicsDevice graphicsDevice) : base(graphicsDevice) 
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Cross && ((Cross)primitive).tessellation == tessellation;
        }
        
        protected override void OnBuild()
        {
            tessellation++;

            Vector2 uv = new Vector2();
            Vector3 position = new Vector3();

            for (int y = 0; y < tessellation; y++)
            {
                for (int x = 0; x < 2; x++)
                {
                    position.X = x - 0.5f;
                    position.Y = y / (tessellation - 1);
                    position.Z = 0;

                    uv.X = 1.0f * x;
                    uv.Y = 1 - 1.0f * y / (tessellation - 1);

                    AddVertex(position, Vector3.UnitZ, uv);
                }
            }

            for (int y = 0; y < tessellation; y++)
            {
                for (int z = 0; z < 2; z++)
                {
                    position.Z = z - 0.5f;
                    position.Y = y / (tessellation - 1);
                    position.X = 0;

                    uv.X = 1.0f * z;
                    uv.Y = 1 - 1.0f * y / (tessellation - 1);

                    AddVertex(position, Vector3.UnitX, uv);
                }
            }

            for (int y = 0; y < tessellation - 1; y++)
            {
                AddIndex((ushort)(y * tessellation));
                AddIndex((ushort)((y + 1) * tessellation + 1));
                AddIndex((ushort)(y * tessellation + 1));

                AddIndex((ushort)(y * tessellation));
                AddIndex((ushort)((y + 1) * tessellation));
                AddIndex((ushort)((y + 1) * tessellation + 1));

                AddIndex((ushort)(tessellation * 2 + y * tessellation));
                AddIndex((ushort)(tessellation * 2 + (y + 1) * tessellation + 1));
                AddIndex((ushort)(tessellation * 2 + y * tessellation + 1));

                AddIndex((ushort)(tessellation * 2 + y * tessellation));
                AddIndex((ushort)(tessellation * 2 + (y + 1) * tessellation));
                AddIndex((ushort)(tessellation * 2 + (y + 1) * tessellation + 1));
            }

            tessellation--;
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}
