namespace Nine.Graphics.Primitives
{ 
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Geometric primitive class for drawing cones.
    /// </summary>
    [ContentSerializable]
    public class Cone : Primitive<VertexPositionNormalTexture>
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
        public Cone(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Cone && ((Cone)primitive).tessellation == tessellation;
        }

        protected override void OnBuild()
        {
            var offset = new Vector3(0, -0.25f, 0);

            AddVertex(Vector3.Up + offset, Vector3.Up);
            AddVertex(Vector3.Zero + offset, -Vector3.Up);

            for (int i = 0; i < tessellation; ++i)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(normal + offset, normal);
                
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

    [NotContentSerializable]
    class ConeInvert : Cone
    {
        public ConeInvert(GraphicsDevice graphics) : base(graphics)
        {
            InvertWindingOrder = true;
        }
    }
}
