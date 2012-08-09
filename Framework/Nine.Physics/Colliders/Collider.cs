namespace Nine.Physics.Colliders
{
    using BEPUphysics.Collidables;
    using BEPUphysics.Entities;
    using System;
    using System.Collections;

    /// <summary>
    /// Defines a basic physics collider.
    /// </summary>
    public abstract class Collider : Transformable, IContainer
    {
        /// <summary>
        /// Gets or sets the mass of this collider.
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// Gets the dynamic and static friction of this collider.
        /// </summary>
        public Range<float> Friction { get; set; }

        /// <summary>
        /// Gets the restitution of this collider.
        /// </summary>
        public float Restitution { get; set; }

        /// <summary>
        /// Gets the Bepu physics entity associated with this collider.
        /// </summary>
        /// <remarks>
        /// This method is Bepu specific.
        /// </remarks>
        public Collidable Collidable
        {
            get
            {
                if (colliderChanged)
                {
                    collidable = CreateCollidable();
                    if (collidable == null)
                        throw new InvalidOperationException("Failed creating collidable");
                    colliderChanged = false;
                }
                return collidable;
            }
        }
        private Collidable collidable;
        private bool colliderChanged = true;

        /// <summary>
        /// Initializes a new instance of Collider.
        /// </summary>
        protected Collider()
        {
            Friction = new Range<float>(0.6f, 0.8f);
            Restitution = 0;
        }

        /// <summary>
        /// When implemented, creates a new Bepu physics collidable object.
        /// </summary>
        /// <remarks>
        /// This method is Bepu specific.
        /// </remarks>
        protected abstract Collidable CreateCollidable();

        /// <summary>
        /// Notifies that this collider has changed and a new inner
        /// object has to be recreated.
        /// </summary>
        protected void NotifyColliderChanged()
        {
            colliderChanged = true;
        }

        /// <summary>
        /// An collider will always have 1 direct child.
        /// </summary>
        IList IContainer.Children
        {
            get { EntityList[0] = Collidable; return EntityList; }
        }
        private static Collidable[] EntityList = new Collidable[1];
    }
}
