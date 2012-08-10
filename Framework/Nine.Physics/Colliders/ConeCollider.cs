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

        public ConeCollider() : base(new Capsule(Vector3.Zero, 1, 1))
        {
            shape = ((ConvexCollidable<ConeShape>)Collidable).Shape;
        }
    }
}
