namespace Nine.Graphics.Primitives
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Geometric primitive class for drawing cubes.
    /// </summary>
    [ContentSerializable]
    public class Box : Primitive<VertexPositionNormalTexture>
    {
        /// <summary>
        /// Constructs a new cube primitive, using default settings.
        /// </summary>
        public Box(GraphicsDevice graphicsDevice) : base(graphicsDevice)
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Box;
        }

        protected override void  OnBuild()
        {
            // A cube has six faces, each one pointing in a different direction.
            Vector3[] normals =
            {
                new Vector3(0, 0, 1),
                new Vector3(0, 0, -1),
                new Vector3(1, 0, 0),
                new Vector3(-1, 0, 0),
                new Vector3(0, 1, 0),
                new Vector3(0, -1, 0),
            };

            // Create each face in turn.
            foreach (Vector3 normal in normals)
            {
                // Get two vectors perpendicular to the face normal and to each other.
                Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
                Vector3 side2 = Vector3.Cross(normal, side1);

                // Six indices (two triangles) per face.
                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 1);
                AddIndex(CurrentVertex + 2);

                AddIndex(CurrentVertex + 0);
                AddIndex(CurrentVertex + 2);
                AddIndex(CurrentVertex + 3);

                // Four vertices per face.
                AddVertex((normal - side1 - side2) / 2, normal, Vector2.Zero);
                AddVertex((normal - side1 + side2) / 2, normal, Vector2.UnitX);
                AddVertex((normal + side1 + side2) / 2, normal, Vector2.One);
                AddVertex((normal + side1 - side2) / 2, normal, Vector2.UnitY);
            }
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() 
            {
                Position = position, 
                Normal = normal,
                TextureCoordinate = uv,
            });
        }
    }

    [NotContentSerializable]
    class BoxInvert : Box
    {
        public BoxInvert(GraphicsDevice graphics) : base(graphics)
        {
            InvertWindingOrder = true;
        }
    }
}

