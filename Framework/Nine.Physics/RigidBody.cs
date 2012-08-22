namespace Nine.Physics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using BEPUphysics;
    using BEPUphysics.Entities;
    using Microsoft.Xna.Framework;
    using Nine.Physics.Colliders;

    /// <summary>
    /// Defines a rigid body in the physics simulation.
    /// </summary>
    [ContentProperty("Collider")]
    public class RigidBody : Component, ISpaceObject, Nine.IUpdateable
    {
        /// <summary>
        /// Gets the collider used by this body.
        /// </summary>
        public Collider Collider
        {
            get { return collider; }
            set
            {
                if (collider != value)
                {
                    if (space != null)
                    {
                        if (collider != null)
                            space.Remove(collider.Entity);
                        if (value != null)
                        {
                            if (value.Entity == null)
                                throw new InvalidOperationException();
                            value.Entity.BecomeDynamic(collider.Mass);
                            space.Add(value.Entity);
                        }
                    }
                    collider = value;
                }
            }
        }
        private Collider collider;

        /// <summary>
        /// Gets the underlying Bepu physics entity.
        /// </summary>
        /// <remarks>
        /// This method is Bepu specific.
        /// </remarks>
        public Entity Entity
        {
            get { return collider != null ? collider.Entity : null; }
        }

        /// <summary>
        /// Gets the Space to which the object belongs.
        /// </summary>
        ISpace ISpaceObject.Space 
        {
            get { return space; }
            set { space = value; }
        }
        private ISpace space;

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Initializes a new instance of Body.
        /// </summary>
        public RigidBody()
        {
            
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            var entity = Entity;
            if (entity != null)
                Parent.Transform = entity.WorldTransform;
        }

        void ISpaceObject.OnAdditionToSpace(ISpace newSpace)
        {
            var entity = Entity;
            if (entity != null)
            {
                entity.WorldTransform = Parent.Transform;
                entity.BecomeDynamic(collider.Mass);
                newSpace.Add(Entity);
            }
        }

        void ISpaceObject.OnRemovalFromSpace(ISpace oldSpace)
        {
            var entity = Entity;
            if (entity != null)
                oldSpace.Remove(Entity);
        }
    }
}
