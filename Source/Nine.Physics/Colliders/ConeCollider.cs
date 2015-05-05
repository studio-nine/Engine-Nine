namespace Nine.Physics.Colliders
{
    using BEPUphysics.Collidables;
    using BEPUphysics.Collidables.MobileCollidables;
    using BEPUphysics.CollisionShapes.ConvexShapes;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a cone shaped collider.
    /// </summary>
    public class ConeCollider : Collider
    {
        /// <summary>
        /// Gets or sets the height of this collider.
        /// </summary>
        public float Height
        {
            get { return shape.Height; }
            set { shape.Height = value; }
        }

        /// <summary>
        /// Gets or sets the radius of this collider.
        /// </summary>
        public float Radius
        {
            get { return shape.Radius; }
            set { shape.Radius = value; }
        }
        private ConeShape shape;

        public ConeCollider()
        {
            NotifyColliderChanged(
                new Entity<ConvexCollidable<ConeShape>>(
                    new ConvexCollidable<ConeShape>(
                        shape = new ConeShape(1, 1))));
        }
    }
}
