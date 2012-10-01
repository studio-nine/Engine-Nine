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
        #region Properties
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
        /// Gets the world transform of this rigid body.
        /// </summary>
        public Matrix Transform
        {
            get { return collider.entity.WorldTransform; }
            set { collider.entity.WorldTransform = value; }
        }

        /// <summary>
        /// Gets or sets the position of this rigid body.
        /// </summary>
        public Vector3 Position
        {
            get { return collider.entity.position; }
            set { collider.entity.Position = value; }
        }

        /// <summary>
        /// Gets or sets the orientation of this rigid body.
        /// </summary>
        public Quaternion Orientation
        {
            get { return collider.entity.orientation; }
            set { collider.entity.Orientation = value; }
        }

        /// <summary>
        /// Gets or sets the mass of this rigid body.
        /// </summary>
        public float Mass
        {
            get { return collider.entity.mass; }
            set { collider.entity.Mass = value; }
        }
        
        /// <summary>
        /// Gets or sets the linear velocity of this rigid body.
        /// </summary>
        public Vector3 Velocity
        {
            get { return collider.entity.linearVelocity; }
            set { collider.entity.LinearVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the angular velocity of this rigid body.
        /// </summary>
        public Vector3 AngularVelocity
        {
            get { return collider.entity.angularVelocity; }
            set { collider.entity.AngularVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the linear damping of this rigid body.
        /// </summary>
        public float Damping
        {
            get { return collider.entity.LinearDamping; }
            set { collider.entity.LinearDamping = value; }
        }

        /// <summary>
        /// Gets or sets the angular Damping of this rigid body.
        /// </summary>
        public float AngularDamping
        {
            get { return collider.entity.AngularDamping; }
            set { collider.entity.AngularDamping = value; }
        }

        /// <summary>
        /// Gets the underlying Bepu physics entity.
        /// </summary>
        /// <remarks>
        /// This method is Bepu specific.
        /// </remarks>
        public Entity Entity
        {
            get { return collider.Entity; }
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
        #endregion

        #region Methods
        /// <summary>
        /// Initializes a new instance of Body.
        /// </summary>
        public RigidBody()
        {
            
        }

        ///<summary>
        /// Applies an impulse to the rigid body.
        ///</summary>
        public void ApplyImpulse(Vector3 impulse)
        {
            collider.entity.ApplyLinearImpulse(ref impulse);
        }

        ///<summary>
        /// Applies an impulse to the rigid body.
        ///</summary>
        public void ApplyImpulse(Vector3 position, Vector3 impulse)
        {
            collider.entity.ApplyImpulse(ref position, ref impulse);
        }

        ///<summary>
        /// Applies an impulse to the rigid body.
        ///</summary>
        public void ApplyAngularImpulse(Vector3 impulse)
        {
            collider.entity.ApplyAngularImpulse(ref impulse);
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            Parent.Transform = collider.entity.WorldTransform;
        }

        void ISpaceObject.OnAdditionToSpace(ISpace newSpace)
        {
            collider.entity.WorldTransform = Parent.Transform;
            collider.entity.BecomeDynamic(collider.Mass);
            newSpace.Add(collider.entity);
        }

        void ISpaceObject.OnRemovalFromSpace(ISpace oldSpace)
        {
            oldSpace.Remove(collider.entity);
        }
        #endregion
    }
}
