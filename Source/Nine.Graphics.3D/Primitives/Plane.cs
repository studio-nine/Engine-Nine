namespace Nine.Graphics.Primitives
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Geometric primitive class for drawing planes.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    public class Plane : Primitive<VertexPositionNormalTexture>
    {
        /// <summary>
        /// Gets or sets the tessellation on x axis of this primitive.
        /// </summary>
        public int TessellationX
        {
            get { return tessellationX; }
            set
            {
                if (tessellationX != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("tessellationX");
                    tessellationX = value;
                    Invalidate();
                }
            }
        }
        private int tessellationX = 1;

        /// <summary>
        /// Gets or sets the tessellation on z axis of this primitive.
        /// </summary>
        public int TessellationZ
        {
            get { return tessellationZ; }
            set
            {
                if (tessellationZ != value)
                {
                    if (value < 1)
                        throw new ArgumentOutOfRangeException("tessellationY");
                    tessellationZ = value;
                    Invalidate();
                }
            }
        }
        private int tessellationZ = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="Plane"/> class.
        /// </summary>
        public Plane(GraphicsDevice graphicsDevice) : base(graphicsDevice) 
        {

        }

        protected override bool CanShareBufferWith(Primitive<VertexPositionNormalTexture> primitive)
        {
            return primitive is Plane && ((Plane)primitive).tessellationX == tessellationX && ((Plane)primitive).tessellationZ == tessellationZ;
        }
        
        protected override void OnBuild()
        {
            tessellationX++;
            tessellationZ++;

            for (int z = 0; z < tessellationZ; z++)
            {
                for (int x = 0; x < tessellationX; ++x)
                {
                    Vector3 position = new Vector3();

                    position.X = 1f * x / (tessellationX - 1) - 0.5f;
                    position.Y = 0;
                    position.Z = 1f * z / (tessellationZ - 1) - 0.5f;

                    Vector2 uv = new Vector2();

                    uv.X = 1.0f * x / (tessellationX - 1);
                    uv.Y = 1.0f * z / (tessellationZ - 1);

                    AddVertex(position, Vector3.Up, uv);
                }
            }

            for (int z = 0; z < tessellationZ - 1; z++)
            {
                for (int x = 0; x < tessellationX - 1; ++x)
                {
                    AddIndex((ushort)(z * tessellationX + x));
                    AddIndex((ushort)(z * tessellationX + x + 1));
                    AddIndex((ushort)((z + 1) * tessellationX + x + 1));

                    AddIndex((ushort)(z * tessellationX + x));
                    AddIndex((ushort)((z + 1) * tessellationX + x + 1));
                    AddIndex((ushort)((z + 1) * tessellationX + x));
                }
            }

            tessellationX--;
            tessellationZ--;
        }

        private void AddVertex(Vector3 position, Vector3 normal, Vector2 uv)
        {
            AddVertex(position, new VertexPositionNormalTexture() { Position = position, Normal = normal, TextureCoordinate = uv });
        }
    }
}
