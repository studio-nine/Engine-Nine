#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Graphics.Primitives
{ 
    /// <summary>
    /// Geometric primitive class for drawing cylinders.
    /// </summary>
    public class Centrum : Primitive<VertexPositionNormal>
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
                                 float height, float radius, int tessellation)
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            AddVertex(Vector3.UnitZ * height, Vector3.UnitZ);
            AddVertex(Vector3.Zero, -Vector3.UnitZ);


            for (int i = 0; i < tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(normal * radius, normal);
                
                AddIndex(0);
                AddIndex(2 + (i + 1) % tessellation);
                AddIndex(2 + i);

                AddIndex(1);
                AddIndex(2 + i);
                AddIndex(2 + (i + 1) % tessellation);
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
            float dy = (float)Math.Sin(angle);

            return new Vector3(dx, dy, 0);
        }

        private void AddVertex(Vector3 position, Vector3 normal)
        {
            AddVertex(position, new VertexPositionNormal() { Position = position, Normal = normal });
        }
    }

    class CentrumInvert : Centrum
    {
        public CentrumInvert(GraphicsDevice graphics) : base(graphics)
        {
            InvertWindingOrder = true;
        }
    }
}
