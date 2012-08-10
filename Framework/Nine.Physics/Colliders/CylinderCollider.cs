namespace Nine.Physics.Colliders
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.Collidables;
    using BEPUphysics.CollisionShapes.ConvexShapes;
    using BEPUphysics.Collidables.MobileCollidables;

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

        public CylinderCollider() : base(new Cylinder(Vector3.Zero, 1, 1))
        {
            shape = ((ConvexCollidable<CylinderShape>)Collidable).Shape;
        }
    }
}
