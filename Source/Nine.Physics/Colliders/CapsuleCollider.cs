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
    /// Represents a capsule shaped collider.
    /// </summary>
    public class CapsuleCollider : Collider
    {
        /// <summary>
        /// Gets or sets the height of this collider.
        /// </summary>
        public float Height
        {
            get { return shape.Length; }
            set { shape.Length = value; }
        }

        /// <summary>
        /// Gets or sets the radius of this collider.
        /// </summary>
        public float Radius
        {
            get { return shape.Radius; }
            set { shape.Radius = value; }
        }
        private CapsuleShape shape;

        public CapsuleCollider()
        {
            NotifyColliderChanged(
                new Entity<ConvexCollidable<CapsuleShape>>(
                    new ConvexCollidable<CapsuleShape>(
                        shape = new CapsuleShape(1, 1))));
        }
    }
}
