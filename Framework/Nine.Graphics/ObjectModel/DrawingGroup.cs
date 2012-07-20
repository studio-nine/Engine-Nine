namespace Nine.Graphics.ObjectModel
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
    /// Defines a logic group of drawable objects.
    /// </summary>
    /// <remarks>
    /// DrawingGroup can be used to create transform hierarchy and bounding box hierarchy.
    /// </remarks>
    [ContentProperty("Children")]
    public class DrawingGroup : Transformable, IContainer, ICollection<object>, INotifyCollectionChanged<object>, Nine.IUpdateable, IBoundable, IDisposable
    {
        #region Properties
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="DrawingGroup"/> is visible.
        /// </summary>
        public bool Visible
        {
            get { return visible; }
            set
            {
                if (visible != value)
                {
                    // TODO: Implement this
                    visible = value;
                }
            }
        }
        private bool visible = true;
        #endregion

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
                for (int i = 0; i < count; i++)
                {
                    var transformable = children[i] as Transformable;
                    if (transformable != null && transformable.Name == name)
                        return transformable;
                }
                return null;
            }
        }

        void Child_Added(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            if (e.Value == null)
                throw new ArgumentNullException("item");

            CheckIntegrity(e.Value);

            Transformable transformable = e.Value as Transformable;
            if (transformable != null)
            {
                if (transformable.Parent != null)
                    throw new InvalidOperationException("The object is already added to a display object.");
                transformable.Parent = this;
            }
                        
            // Model animations are added automatically to the parent display object
            //if (e.Value is DrawableModel)
            //{
            //    animations.Animations.AddRange(((DrawableModel)e.Value).Animations.Animations);
            //}

            OnAdded(e.Value);

            if (Added != null)
                Added(this, e);
        }

        [Conditional("DEBUG")]
        private void CheckIntegrity(object value)
        {
            // This method is called after the object has been added to children, so check against 1 instead.
            if (children.Count(c => c == value) != 1)
                throw new InvalidOperationException("The object is already a child of this display object");
        }

        void Child_Removed(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            Transformable transformable = e.Value as Transformable;
            if (transformable != null)
            {
                if (transformable.Parent == null)
                    throw new InvalidOperationException("The object does not belong to this display object.");
                transformable.Parent = null;

                // Remove all the transform bindings associated with this child
                for (int i = 0; i < TransformBindings.Count; i++)
                {
                    var binding = TransformBindings[i];
                    if (binding.Source == transformable || binding.Target == transformable)
                    {
                        TransformBindings.RemoveAt(i);
                        i--;
                    }
                }
            }

            OnRemoved(e.Value);

            if (Removed != null)
                Removed(this, e);
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
        public event EventHandler<NotifyCollectionChangedEventArgs<object>> Added;

        /// <summary>
        /// Occurs when a child object is removed directly from this drawing group.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs<object>> Removed;
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

        void ICollection<object>.CopyTo(object[] array, int arrayIndex)
        {
            children.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the number of objects managed by this drawing group.
        /// </summary>
        public int Count
        {
            get { return children.Count; }
        }

        bool ICollection<object>.IsReadOnly
        {
            get { return false; }
        }

        IEnumerator<object> IEnumerable<object>.GetEnumerator()
        {
            return children.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return children.GetEnumerator();
        }
        #endregion

        #region BoundingBox
        /// <summary>
        /// Computes the axis aligned bounding box that exactly contains the bounds of all child node.
        /// </summary>
        /// <remarks>
        /// Any children that implements <see cref="IBoundable"/> will be included in the final bounding box.
        /// </remarks>
        public BoundingBox BoundingBox
        {
            get
            {
                var hasBoundable = false;
                var bounds = new BoundingBox();
                var count = children.Count;
                for (int i = 0; i < count; i++)
                {
                    var boundable = children[i] as IBoundable;
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
            for (int i = 0; i < count; i++)
            {
                var transformable = children[i] as Transformable;
                if (transformable != null)
                    transformable.NotifyTransformChanged();
            }
        }
        #endregion

        #region Transform Binding
        /// <summary>
        /// Gets all the transform bindings owned by this <c>DisplayObject</c>.
        /// </summary>
        public NotificationCollection<TransformBinding> TransformBindings { get; private set; }

        internal List<TransformBinding> sortedTransformBindings;
        private bool transformBindingNeedsSort = true;

        void transformBindings_Added(object sender, NotifyCollectionChangedEventArgs<TransformBinding> e)
        {
            if (e.Value == null)
                throw new ArgumentNullException("item");

            if (!string.IsNullOrEmpty(e.Value.SourceName))
            {
                e.Value.Source = Find<Transformable>(e.Value.SourceName);
                if (e.Value.Source == null)
                    throw new ContentLoadException("Cannot find a child object with name: " + e.Value.SourceName);
                e.Value.SourceName = null;
            }

            if (!string.IsNullOrEmpty(e.Value.TargetName))
            {
                e.Value.Target = Find<Transformable>(e.Value.TargetName);
                if (e.Value.Target == null)
                    throw new ContentLoadException("Cannot find a child object with name: " + e.Value.TargetName);
                e.Value.TargetName = null;
            }

            if (!children.Contains(e.Value.Source) || !children.Contains(e.Value.Target))
                throw new InvalidOperationException("The source and target object for the binding must be a child of this display object.");

            if (TransformBindings.Count(b => b.Source == e.Value.Source) > 1)
                throw new InvalidOperationException("Cannot bind the source object multiple times.");
            
            if (!string.IsNullOrEmpty(e.Value.TargetBone))
            {
                if (e.Value.ShareSkeleton)
                    throw new InvalidOperationException("Cannot specify bone name when skeleton is shared");

                var model = e.Value.Target as Model;
                if (model == null)
                    throw new InvalidOperationException("The target object must be a DrawableModel when a bone name is specified.");
                
                e.Value.TargetBoneIndex = model.Skeleton.GetBone(e.Value.TargetBone);
                if (e.Value.TargetBoneIndex < 0)
                    throw new InvalidOperationException(string.Format("Target bone {0} not found", e.Value.TargetBone));
            }
            else if (e.Value.ShareSkeleton)
            {
                var targetModel = e.Value.Target as Model;
                if (targetModel == null)
                    throw new InvalidOperationException("The target object must be a DrawableModel.");

                var sourceModel = e.Value.Source as Model;
                if (sourceModel == null)
                    throw new InvalidOperationException("The target object must be a DrawableModel.");

                sourceModel.SharedSkeleton = targetModel.Skeleton;
            }

            transformBindingNeedsSort = true;
        }

        void transformBindings_Removed(object sender, NotifyCollectionChangedEventArgs<TransformBinding> e)
        {
            transformBindingNeedsSort = true;
        }

        /// <summary>
        /// Binds the transformation of the source object to the target object.
        /// </summary>
        public void Bind(Transformable source, Transformable target)
        {
            Bind(source, target, null, null);
        }

        /// <summary>
        /// Binds the transformation of the source object to the target object.
        /// </summary>
        public void Bind(Transformable source, Transformable target, string boneName)
        {
            Bind(source, target, boneName, null);
        }

        /// <summary>
        /// Binds the transformation of the source object to the target object.
        /// </summary>
        public void Bind(Transformable source, Transformable target, string boneName, Matrix? biasTransform)
        {
            TransformBindings.Add(new TransformBinding(source, target) { TargetBone = boneName, Transform = biasTransform });
        }

        /// <summary>
        /// Unbinds the transformation of the source object to the target object.
        /// </summary>
        public void Unbind(Transformable source, Transformable target)
        {
            TransformBindings.RemoveAll(b => b.Source == source && b.Target == target);
        }
        #endregion

        #region Animations
        /// <summary>
        /// Gets all the animations in this display object.
        /// </summary>
        [ContentSerializer]
        public AnimationPlayer Animations
        {
            get { return animations; }
            
            // For serialization
            internal set 
            {
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

        #region Level Of Detail
        /// <summary>
        /// Gets a collection of objects containing all the detail levels.
        /// </summary>
        public IList<object> DetailLevels
        {
            get { return detailLevels ?? (detailLevels = new List<object>()); }
        }
        private IList<object> detailLevels;
        #endregion

        #region Find
        /// <summary>
        /// Performs a depth first search and finds the first desecant transformables with the specified name.
        /// </summary>
        public T Find<T>(string name) where T : class
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentException("name");

            T result = null;
            ContainerTraverser.Traverse<T>(this, delegate(T desendant)
            {
                var transformable = desendant as Transformable;
                if (transformable != null && transformable.Name == name)
                {
                    result = desendant;
                    return TraverseOptions.Stop;
                }
                return TraverseOptions.Continue;
            });
            return result;            
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <c>DisplayObject</c>.
        /// </summary>
        public DrawingGroup()
        {
            children = new NotificationCollection<object>();
            children.Sender = this;
            children.Added += Child_Added;
            children.Removed += Child_Removed;

            TransformBindings = new NotificationCollection<TransformBinding>();
            TransformBindings.Sender = this;
            TransformBindings.Added += transformBindings_Added;
            TransformBindings.Removed += transformBindings_Removed;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingGroup"/> class.
        /// </summary>
        public DrawingGroup(IEnumerable children) : this()
        {
            foreach (var child in children)
                this.children.Add(child);
        }
        #endregion

        #region Update
        public virtual void Update(TimeSpan elapsedTime)
        {
            var count = children.Count;
            for (int i = 0; i < count; i++)
            {
                var updateable = children[i] as Nine.IUpdateable;
                if (updateable != null)
                    updateable.Update(elapsedTime);
            }

            Animations.Update(elapsedTime);
            UpdateTransformBindings();
        }

        private void UpdateTransformBindings()
        {
            if (transformBindingNeedsSort)
            {
                SortTransformBinding();
                transformBindingNeedsSort = false;
            }

            if (sortedTransformBindings == null)
                return;

            for (int i = 0; i < sortedTransformBindings.Count; i++)
            {
                var binding = sortedTransformBindings[i];

                if (binding.TargetBoneIndex < 0)
                {
                    if (binding.Transform != null)
                        binding.Source.Transform = binding.Transform.Value * binding.Target.Transform;
                    else
                        binding.Source.Transform = binding.Target.Transform;
                }
                else
                {
                    var model = binding.Target as Model;
                    var boneTransform = model.Skeleton.GetAbsoluteBoneTransform(binding.TargetBoneIndex);
                    if (!binding.UseBoneScale)
                    {
                        Vector3 translation, scale;
                        Quaternion rotation;
                        if (!boneTransform.Decompose(out scale, out rotation, out translation))
                            throw new InvalidOperationException();
                        Matrix.CreateFromQuaternion(ref rotation, out boneTransform);
                        boneTransform.M41 = translation.X;
                        boneTransform.M42 = translation.Y;
                        boneTransform.M43 = translation.Z;
                    }
                    if (binding.Transform != null)
                        binding.Source.Transform = binding.Transform.Value * boneTransform * model.Transform;
                    else
                        binding.Source.Transform = boneTransform * model.Transform;
                }
            }
        }

        private void SortTransformBinding()
        {
            if (TransformBindings.Count <= 0)
                return;

            if (DependencySortQueue == null)
                DependencySortQueue = new Queue<TransformBinding>(TransformBindings.Count);
            else
                DependencySortQueue.Clear();

            if (sortedTransformBindings == null)
                sortedTransformBindings = new List<TransformBinding>(TransformBindings.Count);
            else
                sortedTransformBindings.Clear();

            // Add nodes with no dependencies
            int count = TransformBindings.Count;
            for (int i = 0; i < count; i++)
            {
                if (!HasDependency(TransformBindings[i].Target))
                    DependencySortQueue.Enqueue(TransformBindings[i]);
            }

            if (DependencySortQueue.Count <= 0)
                throw new InvalidOperationException("Circular dependency found for transform bindings.");

            while (DependencySortQueue.Count > 0)
            {
                var currentNode = DependencySortQueue.Dequeue();
                sortedTransformBindings.Add(currentNode);

                // Add incoming edges
                for (int i = 0; i < TransformBindings.Count; i++)
                    if (TransformBindings[i].Target == currentNode.Source)
                        DependencySortQueue.Enqueue(TransformBindings[i]);
            }
        }

        private bool HasDependency(Transformable target)
        {
            for (int i = 0; i < TransformBindings.Count; i++)
                if (TransformBindings[i].Source == target)
                    return true;
            return false;
        }

        static Queue<TransformBinding> DependencySortQueue;
        #endregion

        #region IDisposable
        public void Dispose()
        {
            for (var i = 0; i < children.Count; i++)
            {
                IDisposable disposable = children[i] as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }
        #endregion
    }
}