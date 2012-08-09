namespace Nine.Physics.Colliders
{
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;

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
            set { size = value; NotifyColliderChanged(); }
        }
        private Vector3 size;

        protected override Entity CreateCollidable()
        {
            return new Box(Vector3.Zero, size.X, size.Y, size.Z);
        }
    }
}
