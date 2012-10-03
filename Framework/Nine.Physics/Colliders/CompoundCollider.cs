namespace Nine.Physics.Colliders
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using BEPUphysics.Collidables;
    using BEPUphysics.Collidables.MobileCollidables;
    using BEPUphysics.CollisionShapes;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.MathExtensions;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Represents a collider that is an aggregate of several colliders.
    /// </summary>
    [ContentProperty("Colliders")]
    public class CompoundCollider : Collider
    {
        /// <summary>
        /// Gets a collection of inner colliders.
        /// </summary>
        public IList<Collider> Colliders
        {
            get { return colliders; }
        }
        private NotificationCollection<Collider> colliders;

        /// <summary>
        /// Initializes a new instance of CompoundCollider.
        /// </summary>
        public CompoundCollider()
        {
            colliders = new NotificationCollection<Collider>();
            colliders.Added += x => { Rebuild(); };
            colliders.Removed += x => { Rebuild(); };
        }

        /// <summary>
        /// Initializes a new instance of CompoundCollider.
        /// </summary>
        public CompoundCollider(IEnumerable<Collider> colliders) : this()
        {
            foreach (var collider in colliders)
                this.colliders.Add(collider);
        }
        
        private void Rebuild()
        {
            var count = colliders.Count;
            var children = new List<CompoundChildData>(count);
            for (int i = 0; i < count; ++i)
            {
                var entity = colliders[i].entity;
                if (entity == null)
                    throw new InvalidOperationException();

                children.Add(new CompoundChildData
                {
                    Material = entity.Material,
                    Entry = new CompoundShapeEntry(entity.CollisionInformation.Shape,
                        new RigidTransform(entity.Position, entity.Orientation)),
                });
            }
            NotifyColliderChanged(new Entity(new CompoundCollidable(children)));
        }
    }
}
