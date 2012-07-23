namespace Nine.Graphics.ObjectModel
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
    [RuntimeNameProperty("Name")]
    [DictionaryKeyProperty("Name")]
    public abstract class Transformable : Nine.Object, IContainedObject
    {
        #region Properties
        /// <summary>
        /// Gets the parent of this object.
        /// </summary>
        public Transformable Parent
        {
            get { return parent; }

            // To be used by DrawingGroup.
            internal set 
            {
                if (parent != value)
                {
                    parent = value; 
                    NotifyTransformChanged();
                }
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
                if (isAbsoluteTransformDirty)
                {
                    if (Parent == null)
                        absoluteTransform = transform;
                    else
                        absoluteTransform = transform * Parent.AbsoluteTransform;
                    isAbsoluteTransformDirty = false;
                }
                return absoluteTransform; 
            } 
        }
        internal Matrix absoluteTransform = Matrix.Identity;
        internal bool isAbsoluteTransformDirty = false;

        // To be used by DrawingGroup only.
        internal void NotifyTransformChanged()
        {
            isAbsoluteTransformDirty = true;
            OnTransformChanged();
        }
        #endregion

        #region IContainedObject
        object IContainedObject.Parent
        {
            get { return Parent; }
        }
        #endregion

    }
}