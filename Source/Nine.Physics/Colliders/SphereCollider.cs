namespace Nine.Physics.Colliders
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.Collidables;
    using BEPUphysics.Collidables.MobileCollidables;
    using BEPUphysics.CollisionShapes.ConvexShapes;

    /// <summary>
    /// Represents a sphere shaped collider.
    /// </summary>
    public class SphereCollider : Collider
    {
        /// <summary>
        /// Gets or sets the radius of this collider.
        /// </summary>
        public float Radius
        {
            get { return shape.Radius; }
            set { shape.Radius = value; }
        }
        private SphereShape shape;

        public SphereCollider()
        {
            NotifyColliderChanged(
                new Entity<ConvexCollidable<SphereShape>>(
                    new ConvexCollidable<SphereShape>(
                        shape = new SphereShape(1))));
        }
    }
}
