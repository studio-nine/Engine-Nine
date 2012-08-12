namespace Nine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Nine.Animations;

    /// <summary>
    /// Defines a logic group of transformable objects to create a transform and bounding box hierarchy.
    /// </summary>
    [ContentProperty("Children")]
    public class Group : Transformable, IContainer, INotifyCollectionChanged<object>, Nine.IUpdateable, IDisposable
    {
        #region Children
        /// <summary>
        /// Gets the child drawable owned used by this drawable.
        /// </summary>
        public IList<object> Children
        {
            get { return children; }
        }
        
        IList IContainer.Children
        {
            get { return children; } 
        }
        private NotificationCollection<object> children;

        /// <summary>
        /// Gets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        public object this[int index]
        {
            get { return children[index]; }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified name.
        /// </summary>
        public object this[string name]
        {
            get 
            {
                if (string.IsNullOrEmpty(name))
                    throw new ArgumentException("name");

                var count = children.Count;
                for (int i = 0; i < count; ++i)
                {
                    var child = children[i] as Nine.Object;
                    if (child != null && child.Name == name)
                        return child;
                }
                return null;
            }
        }

        void Child_Added(object value)
        {
            if (value == null)
                throw new ArgumentNullException("item");

            CheckIntegrity(value);

            var component = value as IComponent;
            if (component != null)
            {
                if (component.Parent != null)
                    throw new InvalidOperationException("The object is already added to a display object.");
                component.Parent = this;
            }

            OnAdded(value);

            var added = Added;
            if (added != null)
                added(value);
        }

        [Conditional("DEBUG")]
        private void CheckIntegrity(object value)
        {
            // This method is called after the object has been added to children, so check against 1 instead.
            if (children.Count(c => c == value) != 1)
                throw new InvalidOperationException("The object is already a child of this display object");
        }

        void Child_Removed(object value)
        {
            var component = value as IComponent;
            if (component != null)
            {
                if (component.Parent == null)
                    throw new InvalidOperationException("The object does not belong to this display object.");
                component.Parent = null;
            }

            OnRemoved(value);

            var removed = Removed;
            if (removed != null)
                removed(value);
        }

        /// <summary>
        /// Called when a child object is added directly to this drawing group.
        /// </summary>
        protected virtual void OnAdded(object child)
        {

        }

        /// <summary>
        /// Called when a child object is removed directly from this drawing group.
        /// </summary>
        protected virtual void OnRemoved(object child)
        {

        }

        /// <summary>
        /// Occurs when a child object is added directly to this drawing group.
        /// </summary>
        public event Action<object> Added;

        /// <summary>
        /// Occurs when a child object is removed directly from this drawing group.
        /// </summary>
        public event Action<object> Removed;
        #endregion

        #region ICollection
        /// <summary>
        /// Adds an object to this drawing group, this is equivelant to Children.Add.
        /// </summary>
        public void Add(object item)
        {
            children.Add(item);
        }

        /// <summary>
        /// Removes an object from this drawing group, this is equivelant to Children.Remove.
        /// </summary>
        public bool Remove(object item)
        {
            return children.Remove(item);
        }

        /// <summary>
        /// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
        /// </summary>
        public void Clear()
        {
            children.Clear();
        }

        /// <summary>
        /// Gets whether this drawing group contains the target item.
        /// </summary>
        public bool Contains(object item)
        {
            return children.Contains(item);
        }

        /// <summary>
        /// Gets the number of objects managed by this drawing group.
        /// </summary>
        public int Count
        {
            get { return children.Count; }
        }
        #endregion

        #region BoundingBox
        /// <summary>
        /// Computes the axis aligned bounding box that exactly contains the bounds of all child node.
        /// </summary>
        /// <remarks>
        /// Any children that implements <see cref="ISpatialQueryable"/> will be included in the final bounding box.
        /// </remarks>
        public BoundingBox BoundingBox
        {
            get
            {
                var hasBoundable = false;
                var bounds = new BoundingBox();
                var count = children.Count;
                for (int i = 0; i < count; ++i)
                {
                    var boundable = children[i] as ISpatialQueryable;
                    if (boundable != null)
                    {
                        if (hasBoundable)
                        {
                            var childBounds = boundable.BoundingBox;
                            BoundingBox.CreateMerged(ref bounds, ref childBounds, out bounds);
                        }
                        else
                        {
                            bounds = boundable.BoundingBox;
                            hasBoundable = true;
                        }
                    }
                }
                return bounds;
            }
        }
        #endregion

        #region Transform
        /// <summary>
        /// Updates the transform of each children
        /// </summary>
        protected override void OnTransformChanged()
        {
            var count = children.Count;
            for (int i = 0; i < count; ++i)
            {
                var transformable = children[i] as Transformable;
                if (transformable != null)
                    transformable.NotifyTransformChanged();
            }
        }
        #endregion
                
        #region Animations
        /// <summary>
        /// Gets all the animations in this display object.
        /// </summary>
        public AnimationPlayer Animations
        {
            get { return animations; }
            set 
            {
                animations.Animations.Clear();
                if (value != null && value.Animations != null)
                {
                    UpdateTweenAnimationTargets(value.Animations.Values);
                    animations.Animations.AddRange(value.Animations);
                }
                animations.Play();
            }
        }
        
        private void UpdateTweenAnimationTargets(IEnumerable value)
        {
            UtilityExtensions.ForEachRecursive<ISupportTarget>(value, supportTarget => supportTarget.Target = this);
        }

        private AnimationPlayer animations = new AnimationPlayer();
        #endregion

        #region Find
        /// <summary>
        /// Performs a depth first search and finds the first desecant transformables with the specified name.
        /// </summary>
        public T FindName<T>(string name) where T : class
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            T result = null;
            ContainerTraverser.Traverse<T>(this, delegate(T desendant)
            {
                var transformable = desendant as Object;
                if (transformable != null && transformable.Name == name)
                {
                    result = desendant;
                    return TraverseOptions.Stop;
                }
                return TraverseOptions.Continue;
            });
            return result;
        }

        /// <summary>
        /// Finds the first direct child with the specified type
        /// </summary>
        public T Find<T>() where T : class
        {
            for (int i = 0; i < children.Count; ++i)
            {
                var child = children[i] as T;
                if (child != null)
                    return child;
            }
            return null;
        }

        /// <summary>
        /// Finds the root object in the scene hierarchy.
        /// </summary>
        public object FindRoot()
        {
            return ContainerTraverser.FindRootContainer(this);
        }

        /// <summary>
        /// Traverse up the scene tree and returns the first parent of type T.
        /// Returns the target object if it is already an instance of T.
        /// </summary>
        public T FindParent<T>() where T : class
        {
            return ContainerTraverser.FindParentContainer<T>(this);
        }

        /// <summary>
        /// Finds all the child objects of the target including the input target.
        /// </summary>
        public void Traverse<T>(ICollection<T> result) where T : class
        {
            ContainerTraverser.Traverse(this, result);
        }
        
        /// <summary>
        /// Finds all the child objects of the target including the input target.
        /// </summary>
        public void Traverse<T>(Func<T, TraverseOptions> result) where T : class
        {
            ContainerTraverser.Traverse(this, result);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <c>Group</c>.
        /// </summary>
        public Group()
        {
            children = new NotificationCollection<object>();
            children.Sender = this;
            children.Added += Child_Added;
            children.Removed += Child_Removed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        public Group(IEnumerable children) : this()
        {
            foreach (var child in children)
                this.children.Add(child);
        }
        #endregion

        #region Update
        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime)
        {
            var count = children.Count;
            for (int i = 0; i < count; ++i)
            {
                var updateable = children[i] as Nine.IUpdateable;
                if (updateable != null)
                    updateable.Update(elapsedTime);
            }

            Animations.Update(elapsedTime);
        }
        #endregion

        #region IDisposable
        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                for (var i = 0; i < children.Count; ++i)
                {
                    IDisposable disposable = children[i] as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }

        ~Group()
        {
            Dispose(false);
        }
        #endregion
    }
}