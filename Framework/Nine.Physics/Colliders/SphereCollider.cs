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
        /// Gets or sets the radius of this sphere collider.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set
            {
                radius = value; 
                if (shape != null) 
                    shape.Radius = value; 
            }
        }
        private float radius;
        private SphereShape shape;

        protected override Collidable CreateCollidable()
        {
            return new ConvexCollidable<SphereShape>(shape = new SphereShape(radius));
        }
    }
}
