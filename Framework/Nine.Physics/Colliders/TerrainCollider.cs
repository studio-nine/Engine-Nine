namespace Nine.Physics.Colliders
{
    using System.Linq;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.DataStructures;
    using BEPUphysics.Collidables;
    using Nine.Graphics;
    using BEPUphysics.MathExtensions;

    /// <summary>
    /// Represents a collider based on heightmap.
    /// </summary>
    public class TerrainCollider : Collider
    {
        /// <summary>
        /// Gets or sets the heightmap.
        /// </summary>
        public Heightmap Heightmap
        {
            get { return heightmap; }
            set { if (heightmap != value) { heightmap = value; NotifyColliderChanged(); } }
        }
        private Heightmap heightmap;

        protected override Collidable CreateCollidable()
        {
            int xLength = heightmap.Width + 1;
            int zLength = heightmap.Height + 1;

            var heights = new float[xLength, zLength];
            for (int x = 0; x < xLength; x++)
                for (int z = 0; z < zLength; z++)
                    heights[x, z] = heightmap.GetHeight(x, zLength - 1 - z);

            //Create the terrain.
            return new Terrain(heights, new AffineTransform(new Vector3(heightmap.Step, 1, heightmap.Step)));
        }
    }
}
