#region File Description
//-----------------------------------------------------------------------------
// SpherePrimitive.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing spheres.
    /// </summary>
    public class Plane : Primitive<VertexPositionNormalTexture>
    {
        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public Plane(GraphicsDevice graphicsDevice) : this(graphicsDevice, 1, 1, 1, 1) { }


        /// <summary>
        /// Constructs a new sphere primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public Plane(GraphicsDevice graphicsDevice, float width, float height, int tessellationX, int tessellationY)
        {
            tessellationX++;
            tessellationY++;

            for (int y = 0; y < tessellationY; y++)
            {
                for (int x = 0; x < tessellationX; x++)
                {
                    Vector3 position = new Vector3();

                    position.X = width * x / (tessellationX - 1) - width / 2;
                    position.Y = height * y / (tessellationY - 1) - height / 2;
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

            InitializePrimitive(graphicsDevice);
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}
