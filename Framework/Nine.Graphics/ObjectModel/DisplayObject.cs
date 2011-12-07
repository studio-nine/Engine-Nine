#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Animations;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines a display object that contains a list of objects.
    /// </summary>
    /// <remarks>
    /// This class serves as a container to composite other objects.
    /// If you wish to create your own display object, derive from <c>Transformable</c> instead.
    /// </remarks>
    public class DisplayObject : Transformable, IUpdateable, INotifyCollectionChanged<object>, IDisposable
    {
        #region Children
        /// <summary>
        /// Gets the child drawable owned used by this drawable.
        /// </summary>
        public NotificationCollection<object> Children { get; private set; }

        /// <summary>
        /// Gets the <see cref="System.Object"/> at the specified index.
        /// </summary>
        public object this[int index]
        {
            get { return Children[index]; }
        }

        /// <summary>
        /// Gets the <see cref="System.Object"/> with the specified name.
        /// </summary>
        public object this[string name]
        {
            get { return Find<object>(name); }
        }

        /// <summary>
        /// Gets the number of child objects
        /// </summary>
        protected override int ChildCount
        {
            get { return Children.Count; }
        }

        /// <summary>
        /// Copies all the child objects to the target array.
        /// </summary>
        public override void CopyTo(object[] array, int startIndex)
        {
            Children.CopyTo(array, startIndex);
        }

        void children_Added(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            if (e.Value == null)
                throw new ArgumentNullException("item");

            // This method is called after the object has been added to children, so check against 1 instead.
            if (Children.Count(c => c == e.Value) > 1)
                throw new InvalidOperationException("The object is already a child of this display object");

            Transformable transformable = e.Value as Transformable;
            if (transformable != null)
            {
                if (transformable.Parent != null)
                    throw new InvalidOperationException("The object is already added to a display object.");
                transformable.Parent = this;
            }

            if (Added != null)
                Added(sender, e);
        }

        void children_Removed(object sender, NotifyCollectionChangedEventArgs<object> e)
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

            if (Removed != null)
                Removed(sender, e);
        }

        /// <summary>
        /// Occurs when a child object is added.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs<object>> Added;

        /// <summary>
        /// Occurs when a child object is removed.
        /// </summary>
        public event EventHandler<NotifyCollectionChangedEventArgs<object>> Removed;
        #endregion

        #region Transform Binding
        /// <summary>
        /// Gets all the transform bindings owned by this <c>DisplayObject</c>.
        /// </summary>
        public NotificationCollection<TransformBinding> TransformBindings { get; private set; }

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

            if (!Children.Contains(e.Value.Source) || !Children.Contains(e.Value.Target))
                throw new InvalidOperationException("The source and target object for the binding must be a child of this display object.");

            if (TransformBindings.Count(b => b.Source == e.Value.Source) > 1)
                throw new InvalidOperationException("Cannot bind the source object multiple times.");
            
            if (!string.IsNullOrEmpty(e.Value.TargetBone))
            {
                var model = e.Value.Target as DrawableModel;
                if (model == null)
                    throw new InvalidOperationException("The target object must be a Model when a bone name is specified.");
                
                e.Value.TargetBoneIndex = model.Skeleton.GetBone(e.Value.TargetBone);
                if (e.Value.TargetBoneIndex < 0)
                    throw new InvalidOperationException(string.Format("Target bone {0} not found", e.Value.TargetBone));
            }

            // TODO: Dependency sorting
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
                    animations = value;
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
        public T Find<T>(string name)
        {
            if (Name == name && this is T)
                return (T)(object)this;
            var result = Children.OfType<Transformable>().FirstOrDefault(t => t.Name == name);
            if (result is T)
                return (T)(object)result;
            return default(T);
        }

        public IEnumerable<T> FindAll<T>(string name)
        {
            if (Name == name && this is T)
                yield return (T)(object)this;
            foreach (var result in Children.OfType<Transformable>().Where(t => t.Name == name))
            {
                if (result is T)
                    yield return (T)(object)result;
            }
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <c>DisplayObject</c>.
        /// </summary>
        public DisplayObject()
        {
            Children = new NotificationCollection<object>();
            Children.Sender = this;
            Children.Added += children_Added;
            Children.Removed += children_Removed;

            TransformBindings = new NotificationCollection<TransformBinding>();
            TransformBindings.Sender = this;
            TransformBindings.Added += transformBindings_Added;
        }
        #endregion

        #region Update
        public virtual void Update(TimeSpan elapsedTime)
        {
            Animations.Update(elapsedTime);
            UpdateTransformBindings();
        }

        private void UpdateTransformBindings()
        {
            foreach (var binding in TransformBindings)
            {
                if (binding.TargetBoneIndex < 0)
                {
                    if (binding.Transform != null)
                        binding.Source.Transform = binding.Transform.Value * binding.Target.Transform;
                    else
                        binding.Source.Transform = binding.Target.Transform;
                }
                else
                {
                    var model = binding.Target as DrawableModel;
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
        #endregion

        #region IDisposable
        public void Dispose()
        {
            for (var i = 0; i < Children.Count; i++)
            {
                IDisposable disposable = Children[i] as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }
        #endregion
    }
}