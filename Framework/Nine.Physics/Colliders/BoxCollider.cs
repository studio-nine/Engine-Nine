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
    /// Represents a box shaped collider.
    /// </summary>
    public class BoxCollider : Collider
    {
        /// <summary>
        /// Gets or sets the size of this box collider.
        /// </summary>
        public Vector3 Size
        {
            get { return new Vector3(shape.Width, shape.Height, shape.Length); }
            set { shape.Width = value.X; shape.Height = value.Y; shape.Length = value.Z; }
        }
        private BoxShape shape;

        public BoxCollider()
        {
            NotifyColliderChanged(
                new Entity<ConvexCollidable<BoxShape>>(
                    new ConvexCollidable<BoxShape>(
                        shape = new BoxShape(1, 1, 1))));
        }
    }
}
