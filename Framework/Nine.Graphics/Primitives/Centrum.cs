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
    [ContentSerializable]
    public class Centrum : Primitive<VertexPositionNormal>
    {
        /// <summary>
        /// Gets or sets the tessellation of this primitive.
        /// </summary>
        public int Tessellation
        {
            get { return tessellation; }
            set
            {
                if (tessellation != value)
                {
                    if (value < 3)
                        throw new ArgumentOutOfRangeException("tessellation");
                    tessellation = value;
                    Invalidate();
                }
            }
        }
        private int tessellation = 32;

        /// <summary>
        /// Constructs a new cylinder primitive, using default settings.
        /// </summary>
        public Centrum(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormal> primitive)
        {
            return primitive is Centrum && ((Centrum)primitive).tessellation == tessellation;
        }

        protected override void  OnBuild()
        {
            AddVertex(Vector3.UnitZ, Vector3.UnitZ);
            AddVertex(Vector3.Zero, -Vector3.UnitZ);


            for (int i = 0; i < tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(normal, normal);
                
                AddIndex(0);
                AddIndex(2 + (i + 1) % tessellation);
                AddIndex(2 + i);

                AddIndex(1);
                AddIndex(2 + i);
                AddIndex(2 + (i + 1) % tessellation);
            }
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
