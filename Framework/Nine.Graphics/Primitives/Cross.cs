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

            for (int x = 0; x < tessellation; x++)
            {
                Vector3 position = new Vector3();

                position.X = x / (tessellation - 1) - 0.5f;
                position.Y = 0.5f;
                position.Z = 0;

                Vector2 uv = new Vector2();

                uv.X = 1.0f * x / (tessellation - 1);
                uv.Y = 1.0f;

                AddVertex(position, Vector3.UnitZ, uv);
            }

            for (int x = 0; x < tessellation - 1; x++)
            {
                AddIndex((ushort)(tessellation + x));
                AddIndex((ushort)(2 * tessellation + x + 1));
                AddIndex((ushort)(tessellation + x + 1));

                AddIndex((ushort)(tessellation + x));
                AddIndex((ushort)(2 * tessellation + x));
                AddIndex((ushort)(tessellation + x + 1));
            }

            tessellation--;
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}
