namespace Nine.Physics.Colliders
{
    using System;
    using System.Collections;
    using BEPUphysics;
    using BEPUphysics.Collidables;
    using BEPUphysics.Entities;
    using BEPUphysics.Materials;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Defines a basic physics collider.
    /// </summary>
    public abstract class Collider : Transformable, IContainer, INotifyCollectionChanged<object>
    {
        /// <summary>
        /// Gets the dynamic and static friction of this collider.
        /// </summary>
        public Range<float> Friction
        {
            get { return friction; }
            set
            {
                friction = value;
                if (materialOwner != null)
                {
                    materialOwner.Material.StaticFriction = value.Min;
                    materialOwner.Material.KineticFriction = value.Max;
                }
            }
        }
        private Range<float> friction = new Range<float>(0.6f, 0.8f);

        /// <summary>
        /// Gets the restitution of this collider.
        /// </summary>
        public float Restitution
        {
            get { return restitution; }
            set 
            {
                restitution = value;
                if (materialOwner != null)
                    materialOwner.Material.Bounciness = value; 
            }
        }
        private float restitution = 0.2f;

        /// <summary>
        /// Gets the rigid body that contains this collider.
        /// </summary>
        public RigidBody Body
        {
            get { return body; }
        }
        private RigidBody body;

        /// <summary>
        /// Gets the Bepu physics collidable associated with this collider.
        /// </summary>
        /// <remarks>
        /// This method is Bepu specific.
        /// </remarks>
        public Collidable Collidable { get; private set; }

        /// <summary>
        /// Gets the Bepu physics entity associated with this collider.
        /// </summary>
        public Entity Entity
        {
            get { return entity; }
        }
        internal Entity entity;

        internal Vector3? Offset;

        private IMaterialOwner materialOwner;
        private ISpaceObject spaceObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="Collider"/> class.
        /// </summary>
        protected Collider() { }

        /// <summary>
        /// Called when this collider is attached to a rigid body.
        /// </summary>
        internal void Attach(RigidBody body)
        {
            this.body = body;            
            OnAttached(body);
        }

        /// <summary>
        /// Called when this collider is attached to a rigid body.
        /// </summary>
        protected virtual void OnAttached(RigidBody body)
        {

        }
        
        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected override void OnTransformChanged()
        {
            if (entity != null)
            {
                if (Offset.HasValue)
                {
                    var transform = Matrix.CreateTranslation(-Offset.Value);
                    Matrix.Multiply(ref transform, ref this.transform, out transform);
                    entity.WorldTransform = transform;
                }
                else
                {
                    entity.WorldTransform = transform;
                }
            }
            base.OnTransformChanged();
        }

        /// <summary>
        /// Notifies that this collider has changed.
        /// </summary>
        protected void NotifyColliderChanged(ISpaceObject newSpaceObject)
        {
            Collidable = newSpaceObject as Collidable;
            materialOwner = newSpaceObject as IMaterialOwner;
            if (materialOwner != null)
            {
                materialOwner.Material.Bounciness = restitution;
                materialOwner.Material.StaticFriction = friction.Min;
                materialOwner.Material.KineticFriction = friction.Max;
            }

            var entity = newSpaceObject as Entity;

            if (body != null && entity == null)
            {
                throw new InvalidOperationException();
            }

            if (entity != null)
            {
                this.entity = entity;
                this.Collidable = entity.CollisionInformation;
            }

            if (spaceObject != null && removed != null)
                removed(spaceObject);
            spaceObject = newSpaceObject;
            if (spaceObject != null && added != null)
                added(spaceObject);
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

        /// <summary>
        /// An collider will always have 1 direct child.
        /// </summary>
        IList IContainer.Children
        {
            get 
            {
                if (spaceObject == null)
                    return null;
                SpaceObjectList[0] = spaceObject;
                return SpaceObjectList; 
            }
        }
        private static ISpaceObject[] SpaceObjectList = new ISpaceObject[1];
    }
}
