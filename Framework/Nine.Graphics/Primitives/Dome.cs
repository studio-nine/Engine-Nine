namespace Nine.Graphics.Primitives
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Geometric primitive class for drawing spheres.
    /// </summary>
    [ContentSerializable]
    public class Dome : Primitive<VertexPositionNormalTexture>
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
        private int tessellation = 16;

        /// <summary>
        /// Constructs a new sphere primitive, using default settings.
        /// </summary>
        public Dome(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Dome && ((Dome)primitive).tessellation == tessellation;
        }

        protected override void OnBuild()
        {
            if (tessellation < 3)
                throw new ArgumentOutOfRangeException("tessellation");

            float angle = (float)Math.Atan2(1, 0.5);
            angle = MathHelper.PiOver2 - angle * 2;
            float radius = (float)(0.5f / Math.Cos(angle));
            float percentage = (MathHelper.PiOver2 - angle) / MathHelper.Pi;
            Vector3 sink = -Vector3.Up * (radius - 1);


            int verticalSegments = tessellation / 2;
            int horizontalSegments = tessellation * 2;

            // Start with a single vertex at the bottom of the sphere.
            AddVertex(Vector3.Up * radius + sink, Vector3.Up, Vector2.One * 0.5f);

            // Create rings of vertices at progressively higher latitudes.
            for (int i = 0; i < verticalSegments; ++i)
            {
                float latitude = ((i + 1) * MathHelper.Pi / verticalSegments) * percentage + MathHelper.PiOver2;

                float dy = (float)Math.Sin(latitude);
                float dxz = (float)Math.Cos(latitude);

                // Create a single ring of vertices at this latitude.
                for (int j = 0; j < horizontalSegments; j++)
                {
                    float longitude = j * MathHelper.TwoPi / horizontalSegments;

                    float dx = (float)Math.Cos(longitude) * dxz;
                    float dz = (float)Math.Sin(longitude) * dxz;

                    Vector3 normal = new Vector3(dx, dy, dz);

                    Vector2 uv = new Vector2();

                    uv.X = dx * radius + 0.5f;
                    uv.Y = dz * radius + 0.5f;

                    AddVertex(normal * radius + sink, normal, uv);
                }
            }


            // Create a fan connecting the bottom vertex to the bottom latitude ring.
            for (int i = 0; i < horizontalSegments; ++i)
            {
                AddIndex(0);
                AddIndex(1 + i);
                AddIndex(1 + (i + 1) % horizontalSegments);
            }

            // Fill the sphere body with triangles joining each pair of latitude rings.
            for (int i = 0; i < verticalSegments - 1; ++i)
            {
                for (int j = 0; j < horizontalSegments; j++)
                {
                    int nextI = i + 1;
                    int nextJ = (j + 1) % horizontalSegments;

                    AddIndex(1 + i * horizontalSegments + j);
                    AddIndex(1 + nextI * horizontalSegments + j);
                    AddIndex(1 + i * horizontalSegments + nextJ);

                    AddIndex(1 + i * horizontalSegments + nextJ);
                    AddIndex(1 + nextI * horizontalSegments + j);
                    AddIndex(1 + nextI * horizontalSegments + nextJ);
                }
            }
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}

