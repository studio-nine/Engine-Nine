namespace Nine.Physics
{
    using System;
    using System.Xml.Serialization;
    using BEPUphysics;
    using BEPUphysics.Entities;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Defines a navigation component that can be added to a game object container.
    /// </summary>
    [Serializable]
    public class PhysicsComponent : Component, Nine.IUpdateable, IServiceProvider, ICloneable
    {
        /// <summary>
        /// Gets the physics entity owned by this physics component.
        /// </summary>
        [XmlIgnore]
        public Entity Entity { get; internal set; }

        /// <summary>
        /// Gets or sets the bias transform between the physics model and the graphics model.
        /// </summary>
        public Matrix? TransformBias { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsComponent"/> class.
        /// </summary>
        public PhysicsComponent() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="PhysicsComponent"/> class.
        /// </summary>
        public PhysicsComponent(Entity entity)
        {
            if (entity == null)
                throw new ArgumentNullException("entity");
            
            this.Entity = entity;
        }

        protected override void OnAdded(WorldObject parent)
        {
            CreatePhysicsEntity();
        }

        internal void CreatePhysicsEntity()
        {
            if (Entity == null || Entity.Space != null)
                return;

            var space = Parent.Find<ISpace>();
            if (space != null)
            {
                Vector3 scale;
                Vector3 position;
                Quaternion rotation;
                Matrix transform = Parent.Transform;

                if (TransformBias.HasValue)
                {
                    var invertTransformBias = Matrix.Invert(TransformBias.Value);
                    Matrix.Multiply(ref transform, ref invertTransformBias, out transform);
                }

                if (!transform.Decompose(out scale, out rotation, out position))
                    throw new InvalidOperationException("Invalid transform");

                // NOTE: Scale is ignored
                Entity.Position = position;
                Entity.Orientation = rotation;
                space.Add(Entity);
            }
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime)
        {
            if (Entity != null)
            {
                var transformable = Parent as ITransformable;
                if (transformable != null)
                {
                    if (TransformBias.HasValue)
                    {
                        var transform = TransformBias.Value;
                        var orientation = Entity.Orientation;
                        Matrix.Transform(ref transform, ref orientation, out transform);

                        var position = Entity.Position;
                        transform.M41 += position.X;
                        transform.M42 += position.Y;
                        transform.M43 += position.Z;
                        transformable.Transform = transform;
                    }
                    else
                    {
                        transformable.Transform = Entity.WorldTransform;
                    }
                }
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (Entity != null && serviceType.IsAssignableFrom(typeof(Entity)))
                return Entity;
            return null;
        }

        public PhysicsComponent Clone()
        {
            return new PhysicsComponent() { Name = Name, Tag = Tag, };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
