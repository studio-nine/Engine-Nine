namespace Nine
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using System.Xaml;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Nine.Content;

    /// <summary>
    /// Base class for all objects that has a transform and a bounds.
    /// </summary>
    [ContentSerializable]
    public abstract class Transformable : Nine.Object, IComponent
    {
        #region Properties
        /// <summary>
        /// Gets the parent of this object.
        /// </summary>
        [ContentSerializerIgnore]
        public Transformable Parent
        {
            get { return parent; }
        }

        IContainer IComponent.Parent
        {
            get { return parent as IContainer; }
            set { SetParent(value); }
        }

        private void SetParent(IContainer value)
        {
            if (parent != value)
            {
                if (value != null)
                {
                    if (parent != null)
                        throw new InvalidOperationException("This object already belongs to a container");
                    var transformable = value as Transformable;
                    if (transformable == null)
                        throw new InvalidOperationException("This object can only be attached to a transformable");
                    parent = transformable;
                }
                else
                {
                    if (parent == null)
                        throw new InvalidOperationException("This object does not belongs to the specified container");
                    parent = null;
                }
                NotifyTransformChanged();
            }
        }

        private Transformable parent;
        #endregion

        #region Transform
        /// <summary>
        /// Gets or sets the transform of this object.
        /// </summary>
        public Matrix Transform
        {
            get { return transform; }
            set { transform = value; NotifyTransformChanged(); }
        }
        internal Matrix transform = Matrix.Identity;
        
        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected virtual void OnTransformChanged() 
        {

        }
        
        /// <summary>
        /// Gets the absolute transform of this drawable.
        /// </summary>
        public Matrix AbsoluteTransform 
        {
            get 
            {
                if ((absoluteTransformDirtyFlags & AbsoluteTransformDirty) != 0)
                {
                    if (Parent == null)
                        absoluteTransform = transform;
                    else
                        absoluteTransform = transform * Parent.AbsoluteTransform;
                    absoluteTransformDirtyFlags |= ~AbsoluteTransformDirty;
                }
                return absoluteTransform; 
            } 
        }
        internal Matrix absoluteTransform = Matrix.Identity;
        internal uint absoluteTransformDirtyFlags = 0;

        const uint AbsoluteTransformDirty = 1;

        internal void NotifyTransformChanged()
        {
            absoluteTransformDirtyFlags = 0xFFFFFFFF;
            OnTransformChanged();
        }
        #endregion
        /*
        #region Methods
        /// <summary>
        /// Moves the position of this transformable by the vector offset provided along world axes.
        /// </summary>
        public void Move(Vector3 translate)
        {
            this.position = this.position + translate;
            NotifyTransformChanged();
        }

        /// <summary>
        /// Moves the position of this transformable by the vector offset provided along it's own axes (relative to orientation).
        /// </summary>
        public void MoveRelative(Vector3 translate)
        {
            // Transform the axes of the relative vector by camera's local axes
            Vector3 trans = MultiplyQuaternion(orientation, translate);

            this.position = this.position + trans; 
            NotifyTransformChanged();
        }

        /// <summary>
        /// Rotate the transformable around an arbitrary axis.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="angle"></param>
        public void Rotate(Vector3 axis, float angle)
        {
            Quaternion q;
            Quaternion.CreateFromAxisAngle(ref axis, angle, out q);
            Rotate(q);
        }
        #endregion
         */
    }
}