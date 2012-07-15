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
    public class Centrum : Primitive<VertexPositionNormalTexture>
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

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Centrum && ((Centrum)primitive).tessellation == tessellation;
        }

        protected override void  OnBuild()
        {
            AddVertex(Vector3.Up, Vector3.Up);
            AddVertex(Vector3.Zero, -Vector3.Up);


            for (int i = 0; i < tessellation; i++)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(normal, normal);
                
                AddIndex(0);
                AddIndex(2 + i);
                AddIndex(2 + (i + 1) % tessellation);

                AddIndex(1);
                AddIndex(2 + (i + 1) % tessellation);
                AddIndex(2 + i);
            }
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

        private void AddVertex(Vector3 position, Vector3 normal)
        {
            AddVertex(position, new VertexPositionNormalTexture()
            {
                Position = position,
                Normal = normal,
                TextureCoordinate = new Vector2((float)(Math.Asin(normal.X) / MathHelper.Pi + 0.5),
                                                (float)(Math.Asin(normal.X) / MathHelper.Pi + 0.5)),
            });
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
