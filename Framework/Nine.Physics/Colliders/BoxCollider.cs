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
            get { return size; }
            set 
            {
                size.X = shape.Width = value.X;
                size.Y = shape.Height = value.Y;
                size.Z = shape.Length = value.Z;
            }
        }
        private Vector3 size = Vector3.One;
        private BoxShape shape;

        public BoxCollider():base(new Box(Vector3.Zero, 1, 1, 1))
        {
            shape = ((ConvexCollidable<BoxShape>)Collidable).Shape;
        }
    }
}
