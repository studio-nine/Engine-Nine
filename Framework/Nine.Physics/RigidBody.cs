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
    public class RigidBody : Component, IContainer, INotifyCollectionChanged<object>
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
                    if (value == null)
                        throw new ArgumentNullException("collider");
                    
                    value.Attach(this);
                    if (value.Entity == null)
                        throw new InvalidOperationException("collider.Entity");

                    collider = value;
                    entity = collider.entity;
                    entity.BecomeDynamic(1);
                }
            }
        }
        private Collider collider;
        private Entity entity;

        /// <summary>
        /// Gets the world transform of this rigid body.
        /// </summary>
        public Matrix Transform
        {
            get 
            {
                if (collider.Offset.HasValue)
                {
                    var transform = Matrix.CreateTranslation(collider.Offset.Value);
                    var orientation = Entity.Orientation;
                    Matrix.Transform(ref transform, ref orientation, out transform);

                    var position = Entity.Position;
                    transform.M41 += position.X;
                    transform.M42 += position.Y;
                    transform.M43 += position.Z;
                    return transform;
                }
                return entity.WorldTransform;
            }
        }

        /// <summary>
        /// Gets or sets the position of this rigid body.
        /// </summary>
        public Vector3 Position
        {
            get { return entity.position; }
            set { entity.Position = value; }
        }

        /// <summary>
        /// Gets or sets the orientation of this rigid body.
        /// </summary>
        public Quaternion Orientation
        {
            get { return entity.orientation; }
            set { entity.Orientation = value; }
        }

        /// <summary>
        /// Gets or sets the mass of this rigid body.
        /// </summary>
        public float Mass
        {
            get { return entity.mass; }
            set { entity.Mass = value; }
        }
        
        /// <summary>
        /// Gets or sets the linear velocity of this rigid body.
        /// </summary>
        public Vector3 Velocity
        {
            get { return entity.linearVelocity; }
            set { entity.LinearVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the angular velocity of this rigid body.
        /// </summary>
        public Vector3 AngularVelocity
        {
            get { return entity.angularVelocity; }
            set { entity.AngularVelocity = value; }
        }

        /// <summary>
        /// Gets or sets the linear damping of this rigid body.
        /// </summary>
        public float Damping
        {
            get { return entity.LinearDamping; }
            set { entity.LinearDamping = value; }
        }

        /// <summary>
        /// Gets or sets the angular Damping of this rigid body.
        /// </summary>
        public float AngularDamping
        {
            get { return entity.AngularDamping; }
            set { entity.AngularDamping = value; }
        }

        /// <summary>
        /// Gets the underlying Bepu physics entity.
        /// </summary>
        /// <remarks>
        /// This method is Bepu specific.
        /// </remarks>
        public Entity Entity
        {
            get { return entity; }
        }
        #endregion

        #region Methods
        internal RigidBody() { }

        /// <summary>
        /// Initializes a new instance of Body.
        /// </summary>
        public RigidBody(Collider collider)
        {
            this.Collider = collider;
        }

        ///<summary>
        /// Applies an impulse to the rigid body.
        ///</summary>
        public void ApplyImpulse(Vector3 impulse)
        {
            entity.ApplyLinearImpulse(ref impulse);
        }

        ///<summary>
        /// Applies an impulse to the rigid body.
        ///</summary>
        public void ApplyImpulse(Vector3 position, Vector3 impulse)
        {
            entity.ApplyImpulse(ref position, ref impulse);
        }

        ///<summary>
        /// Applies an impulse to the rigid body.
        ///</summary>
        public void ApplyAngularImpulse(Vector3 impulse)
        {
            entity.ApplyAngularImpulse(ref impulse);
        }

        /// <summary>
        /// Called when this component is added to a parent group.
        /// </summary>
        protected override void OnAdded(Group parent)
        {
            collider.Transform = parent.Transform;
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        protected override void Update(TimeSpan elapsedTime)
        {
            Parent.Transform = Transform;
        }
        
        event Action<object> INotifyCollectionChanged<object>.Added
        {
            add { added += value; }
            remove { added -= value; }
        }
        private Action<object> added;

        event Action<object> INotifyCollectionChanged<object>.Removed
        {
            add { removed += value; }
            remove { removed -= value; }
        }
        private Action<object> removed;

        IList IContainer.Children
        {
            get
            {
                if (collider == null)
                    return null;
                SpaceObjectList[0] = entity;
                return SpaceObjectList;
            }
        }
        private static ISpaceObject[] SpaceObjectList = new ISpaceObject[1];
        #endregion
    }
}
