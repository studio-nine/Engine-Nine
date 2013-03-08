namespace Nine.Graphics.Primitives
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Geometric primitive class for drawing cylinders.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    public class Cylinder : Primitive<VertexPositionNormalTexture>
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
        public Cylinder(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Cylinder && ((Cylinder)primitive).tessellation == tessellation;
        }

        protected override void  OnBuild()
        {
            AddVertex(Vector3.Up * 0.5f, Vector3.Up);
            AddVertex(Vector3.Down * 0.5f, Vector3.Down);

            // Create a ring of triangles around the outside of the cylinder.
            for (int i = 0; i < tessellation; ++i)
            {
                Vector3 normal = GetCircleVector(i, tessellation);

                AddVertex(normal + 0.5f * Vector3.Up, normal);
                AddVertex(normal - 0.5f * Vector3.Up, normal);

                AddIndex(0);
                AddIndex(2 + i * 2);
                AddIndex(2 + (i * 2 + 2) % (tessellation * 2));

                AddIndex(2 + i * 2);
                AddIndex(2 + i * 2 + 1);
                AddIndex(2 + (i * 2 + 2) % (tessellation * 2));

                AddIndex(1);
                AddIndex(2 + (i * 2 + 3) % (tessellation * 2));
                AddIndex(2 + i * 2 + 1);

                AddIndex(2 + i * 2 + 1);
                AddIndex(2 + (i * 2 + 3) % (tessellation * 2));
                AddIndex(2 + (i * 2 + 2) % (tessellation * 2));
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
}
