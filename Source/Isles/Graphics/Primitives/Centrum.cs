#region File Description
//-----------------------------------------------------------------------------
// CylinderPrimitive.cs
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
    /// Geometric primitive class for drawing cylinders.
    /// </summary>
    internal class Centrum : Primitive
    {
        /// <summary>
        /// Constructs a new cylinder primitive, using default settings.
        /// </summary>
        public Centrum(GraphicsDevice graphicsDevice)
            : this(graphicsDevice, 1, 1, 32)
        {
        }


        /// <summary>
        /// Constructs a new cylinder primitive,
        /// with the specified size and tessellation level.
        /// </summary>
        public Centrum(GraphicsDevice graphicsDevice,
                                 float height, float diameter, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            height /= 2;

            float radius = diameter / 2;


            AddVertex(Vector3.Up * height, Vector3.Up);
            AddVertex(Vector3.Down * height, Vector3.Down);


            for (int i = 0; i < tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(normal * radius + Vector3.Down * height, normal);


                AddIndex(0);
                AddIndex(2 + i);
                AddIndex(2 + (i + 1) % tessellation);

                AddIndex(1);
                AddIndex(2 + (i + 1) % tessellation);
                AddIndex(2 + i);
            }


            InitializePrimitive(graphicsDevice);
        }

        /// <summary>
        /// Helper method computes a point on a circle.
        /// </summary>
        static Vector3 GetCircleVector(int i, int tessellation)
        {
            float angle = i * MathHelper.TwoPi / tessellation;

            float dx = (float)Math.Cos(angle);
            float dz = (float)Math.Sin(angle);

            return new Vector3(dx, 0, dz);
        }
    }
}
