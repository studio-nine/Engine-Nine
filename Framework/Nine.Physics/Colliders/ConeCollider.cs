namespace Nine.Physics.Colliders
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;

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
            get { return height; }
            set { height = value; NotifyColliderChanged(); }
        }
        private float height;

        /// <summary>
        /// Gets or sets the radius of this collider.
        /// </summary>
        public float Radius
        {
            get { return radius; }
            set { radius = value; NotifyColliderChanged(); }
        }
        private float radius;

        protected override Entity CreateCollidable()
        {
            return new Cone(Vector3.Zero, height, radius);
        }
    }
}
