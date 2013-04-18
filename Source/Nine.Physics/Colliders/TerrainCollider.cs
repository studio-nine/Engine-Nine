namespace Nine.Physics.Colliders
{
    using BEPUphysics.Collidables;
    using BEPUphysics.CollisionShapes;
    using BEPUphysics.MathExtensions;
    using Microsoft.Xna.Framework;
    using Nine.Graphics;

    /// <summary>
    /// Represents a collider based on heightmap.
    /// </summary>
    public class TerrainCollider : Collider
    {
        /// <summary>
        /// Gets or sets the heightmap.
        /// </summary>
        public IHeightmap Heightmap
        {
            get { return heightmap; }
            set 
            {
                if (heightmap != value)
                {
                    NotifyColliderChanged(terrain = CreateTerrain(heightmap = value));
                    OnTransformChanged();
                }
            }
        }
        private IHeightmap heightmap;
        private Terrain terrain;

        /// <summary>
        /// Initializes a new instance of the <see cref="TerrainCollider"/> class.
        /// </summary>
        public TerrainCollider() { }

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            if (terrain != null && heightmap != null)
            {
                Matrix matrix;
                Matrix.CreateScale(heightmap.Step, 1, heightmap.Step, out matrix);
                Matrix.Multiply(ref matrix, ref transform, out matrix);
                terrain.WorldTransform = new AffineTransform() { Matrix = matrix };
            }
            base.OnTransformChanged();
        }

        private static Terrain CreateTerrain(IHeightmap heightmap)
        {
            return heightmap != null ? new Terrain(CreateTerrainShape(heightmap), AffineTransform.Identity) : null;
        }

        private static TerrainShape CreateTerrainShape(IHeightmap heightmap)
        {
            int xLength = heightmap.Width + 1;
            int zLength = heightmap.Height + 1;

            var heights = new float[xLength, zLength];
            for (int x = 0; x < xLength; ++x)
                for (int z = 0; z < zLength; z++)
                    heights[x, z] = heightmap.GetHeight(x, z);

            return new TerrainShape(heights);
        }
    }
}
