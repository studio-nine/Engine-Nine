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
    /// Represents a cylinder shaped collider.
    /// </summary>
    public class CylinderCollider : Collider
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
        private CylinderShape shape;

        public CylinderCollider()
        {
            NotifyColliderChanged(
                new Entity<ConvexCollidable<CylinderShape>>(
                    new ConvexCollidable<CylinderShape>(
                        shape = new CylinderShape(1, 1))));
        }
    }
}
