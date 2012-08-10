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
            get { return Colliders; }
        }
        private NotificationCollection<Collider> colliders;

        /// <summary>
        /// Initializes a new instance of CompoundCollider.
        /// </summary>
        public CompoundCollider() : base((Collidable)null)
        {
            colliders = new NotificationCollection<Collider>();
            //colliders.Added += x => { NotifyColliderChanged(); };
            //colliders.Removed += x => { NotifyColliderChanged(); };
            throw new NotImplementedException();
        }

        /// <summary>
        /// Initializes a new instance of CompoundCollider.
        /// </summary>
        public CompoundCollider(IEnumerable<Collider> colliders) : this()
        {
            foreach (var collider in colliders)
                this.colliders.Add(collider);
        }

        private static Collidable CreateCollidable(IList<Collider> colliders)
        {
            var count = colliders.Count;
            var children = new List<CompoundChildData>(count);
            for (int i = 0; i < count; i++)
            {
                var collidable = colliders[i].Collidable;
                var child = new CompoundChildData();
                //child.Entry = new CompoundShapeEntry(collidable.Shape,
                //              new RigidTransform(collidable.Position, collidable.Orientation));
                //child.Material = collidable.Material;
                children.Add(child);
            }
            return new CompoundCollidable(children);
        }
    }
}
