namespace Nine.Physics.Colliders
{
    using System;
    using System.Collections;
    using BEPUphysics;
    using BEPUphysics.Collidables;
    using BEPUphysics.Entities;
    using BEPUphysics.Materials;

    /// <summary>
    /// Defines a basic physics collider.
    /// </summary>
    public abstract class Collider : Transformable, IContainer
    {
        /// <summary>
        /// Gets or sets whether this collider is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the mass of this collider.
        /// </summary>
        public float Mass { get; set; }

        /// <summary>
        /// Gets the dynamic and static friction of this collider.
        /// </summary>
        public Range<float> Friction
        {
            get 
            {
                var material = materialOwner.Material;
                return new Range<float>(material.staticFriction, material.kineticFriction);
            }
            set
            {
                var material = materialOwner.Material;
                material.StaticFriction = value.Min;
                material.kineticFriction = value.Max;
            }
        }

        /// <summary>
        /// Gets the restitution of this collider.
        /// </summary>
        public float Restitution
        {
            get { return materialOwner.Material.bounciness; }
            set { materialOwner.Material.Bounciness = value; }
        }

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
        public Entity Entity { get; private set; }

        private IMaterialOwner materialOwner;
        private ISpaceObject spaceObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="Collider"/> class.
        /// </summary>
        protected Collider(Collidable collidable)
        {
            if (collidable == null)
                throw new ArgumentNullException("collidable");

            this.Collidable = collidable;
            this.materialOwner = (IMaterialOwner)collidable;
            this.spaceObject = collidable as ISpaceObject;
            this.Mass = 1;
            this.Friction = new Range<float>(0.6f, 0.8f);
            this.Restitution = 0.2f;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Collider"/> class.
        /// </summary>
        protected Collider(Entity entity)
        {
            if (entity == null || entity.IsDynamic)
                throw new ArgumentException("entity");

            this.Entity = entity;
            this.materialOwner = entity;
            this.spaceObject = entity;
            this.Collidable = Entity.CollisionInformation;
            this.Mass = 1;
            this.Friction = new Range<float>(0.6f, 0.8f);
            this.Restitution = 0.2f;
        }

        /// <summary>
        /// Notifies that this collider has changed.
        /// </summary>
        protected void NotifyColliderChanged()
        {

        }

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
            if (Entity != null)
                Entity.WorldTransform = Transform;
            base.OnTransformChanged();
        }

        /// <summary>
        /// An collider will always have 1 direct child.
        /// </summary>
        IList IContainer.Children
        {
            get { SpaceObjectList[0] = spaceObject; return SpaceObjectList; }
        }
        private static ISpaceObject[] SpaceObjectList = new ISpaceObject[1];
    }
}
