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
    /// Geometric primitive class for drawing planes.
    /// </summary>
    [ContentSerializable]
    public class Plane : Primitive<VertexPositionNormalTexture>
    {
        /// <summary>
        /// Gets or sets the tessellation on x axis of this primitive.
        /// </summary>
        public int TessellationX
        {
            get { return tessellationX; }
            set
            {
                if (tessellationX != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("tessellationX");
                    tessellationX = value;
                    Invalidate();
                }
            }
        }
        private int tessellationX = 1;

        /// <summary>
        /// Gets or sets the tessellation on y axis of this primitive.
        /// </summary>
        public int TessellationY
        {
            get { return tessellationY; }
            set
            {
                if (tessellationY != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("tessellationY");
                    tessellationY = value;
                    Invalidate();
                }
            }
        }
        private int tessellationY = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> class.
        /// </summary>
        public Plane(GraphicsDevice graphicsDevice) : base(graphicsDevice) 
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Plane && ((Plane)primitive).tessellationX == tessellationX && ((Plane)primitive).tessellationY == tessellationY;
        }
        
        protected override void OnBuild()
        {
            tessellationX++;
            tessellationY++;

            for (int y = 0; y < tessellationY; y++)
            {
                for (int x = 0; x < tessellationX; x++)
                {
                    Vector3 position = new Vector3();

                    position.X = x / (tessellationX - 1) - 0.5f;
                    position.Y = y / (tessellationY - 1) - 0.5f;
                    position.Z = 0;

                    Vector2 uv = new Vector2();

                    uv.X = 1.0f * x / (tessellationX - 1);
                    uv.Y = 1.0f * y / (tessellationY - 1);

                    AddVertex(position, Vector3.UnitZ, uv);
                }
            }

            for (int y = 0; y < tessellationY - 1; y++)
            {
                for (int x = 0; x < tessellationX - 1; x++)
                {
                    AddIndex((ushort)(y * tessellationX + x));
                    AddIndex((ushort)((y + 1) * tessellationX + x + 1));
                    AddIndex((ushort)(y * tessellationX + x + 1));

                    AddIndex((ushort)(y * tessellationX + x));
                    AddIndex((ushort)((y + 1) * tessellationX + x));
                    AddIndex((ushort)((y + 1) * tessellationX + x + 1));
                }
            }

            tessellationX--;
            tessellationY--;
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}
