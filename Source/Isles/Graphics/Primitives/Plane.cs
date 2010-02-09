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

namespace Isles.Graphics.Primitives
{
    /// <summary>
    /// Geometric primitive class for drawing spheres.
    /// </summary>
    public class Plane : Primitive
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
                    Vector3 position;

                    position.Y = 0;
                    position.X = width * x / (tessellationX - 1) - width / 2;
                    position.Z = height * y / (tessellationY - 1) - height / 2;

                    Vector2 uv;

                    uv.X = 1.0f * x / (tessellationX - 1);
                    uv.Y = 1.0f * y / (tessellationY - 1);

                    AddVertex(position, Vector3.Up, uv);
                }
            }

            for (int y = 0; y < tessellationY - 1; y++)
            {
                for (int x = 0; x < tessellationX - 1; x++)
                {
                    AddIndex((ushort)(y * tessellationX + x));
                    AddIndex((ushort)(y * tessellationX + x + 1));
                    AddIndex((ushort)((y + 1) * tessellationX + x + 1));

                    AddIndex((ushort)(y * tessellationX + x));
                    AddIndex((ushort)((y + 1) * tessellationX + x + 1));
                    AddIndex((ushort)((y + 1) * tessellationX + x));
                }
            }

            InitializePrimitive(graphicsDevice);
        }
    }
}
