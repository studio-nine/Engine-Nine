namespace Nine.Graphics.Primitives
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    [ContentSerializable]
    public class Sphere : Primitive<VertexPositionNormalTexture>
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
        private int tessellation = 8;

        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public Sphere(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Sphere && ((Sphere)primitive).tessellation == tessellation;
        }
        
        protected override void  OnBuild()
        {
            int verticalSegments = tessellation;
            int horizontalSegments = tessellation * 2;

            // Start with a single vertex at the bottom of the sphere.
            AddVertex(-Vector3.Up, -Vector3.Up);

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments - 1; ++i)
            {
                float latitude = ((i + 1) * MathHelper.Pi /
                                            verticalSegments) - MathHelper.PiOver2;

                float dy = (float)Math.Sin(latitude);
                float dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxz;
                    float dz = (float)Math.Sin(longitude) * dxz;

                    Vector3 normal = new Vector3(dx, dy, dz);

                    AddVertex(normal, normal);
                }
            }

            // Finish with a single vertex at the top of the sphere.
            AddVertex(Vector3.Up, Vector3.Up);

            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; ++i)
            {
                AddIndex(0);
                AddIndex(1 + (i + 1) % horizontalSegments);
                AddIndex(1 + i);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 2; ++i)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                }
            }

            // Create a fan connecting the top vertex to the top latitude ring.
            for (int i = 0; i < horizontalSegments; ++i)
            {
                AddIndex(CurrentVertex - 1);
                AddIndex(CurrentVertex - 2 - (i + 1) % horizontalSegments);
                AddIndex(CurrentVertex - 2 - i);
            }
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

    class SphereInvert : Sphere 
    {
        public SphereInvert(GraphicsDevice graphics) : base(graphics)
        {
            InvertWindingOrder = true;
        }
    }
}
