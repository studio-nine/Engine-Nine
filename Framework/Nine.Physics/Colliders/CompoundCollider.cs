namespace Nine.Physics.Colliders
{
    using BEPUphysics.Collidables.MobileCollidables;
    using BEPUphysics.CollisionShapes;
    using BEPUphysics.Entities;
    using BEPUphysics.Entities.Prefabs;
    using BEPUphysics.MathExtensions;
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;

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
            get { return Colliders; }
        }
        private NotificationCollection<Collider> colliders;

        /// <summary>
        /// Initializes a new instance of CompoundCollider.
        /// </summary>
        public CompoundCollider()
        {
            colliders = new NotificationCollection<Collider>();
            colliders.Added += x => { NotifyColliderChanged(); };
            colliders.Removed += x => { NotifyColliderChanged(); };
        }

        /// <summary>
        /// Initializes a new instance of CompoundCollider.
        /// </summary>
        public CompoundCollider(IEnumerable<Collider> colliders) : this()
        {
            foreach (var collider in colliders)
                this.colliders.Add(collider);
        }

        protected override Entity CreateCollidable()
        {
            var count = colliders.Count;
            var children = new List<CompoundChildData>(count);
            for (int i = 0; i < count; i++)
            {
                var entity = colliders[i].Collidable;
                var child = new CompoundChildData();
                child.Entry = new CompoundShapeEntry(entity.CollisionInformation.Shape,
                              new RigidTransform(entity.Position, entity.Orientation));
                child.Material = entity.Material;
                children.Add(child);
            }
            return new CompoundBody(children);
        }
    }
}
