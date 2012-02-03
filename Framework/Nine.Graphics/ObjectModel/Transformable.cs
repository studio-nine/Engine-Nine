#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;

#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Base class for all objects that has a transform and a bounds.
    /// </summary>
    public abstract class Transformable : IContainer, IContainedObject
    {
        #region Properties
        /// <summary>
        /// Gets the parent of this object.
        /// </summary>
        public Transformable Parent
        {
            get { return parent; }

            // Don't allow externals to set parents
            internal set 
            {
                parent = value; 
                MarkAbsoluteTransformsDirty();
                OnTransformChanged();
            }
        }
        private Transformable parent;

        /// <summary>
        /// Gets or sets the name of this transformable.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets any custom data.
        /// </summary>
        public object Tag { get; set; }
        #endregion

        #region Transform
        /// <summary>
        /// Gets or sets the transform of this object.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public Matrix Transform
        {
            get { return transform; }
            set
            {
                Matrix oldValue = transform;
                transform = value;
                MarkAbsoluteTransformsDirty();
            }
        }
        private Matrix transform = Matrix.Identity;

        /// <summary>
        /// Called when local or absolute transform changed.
        /// </summary>
        protected virtual void OnTransformChanged()
        {
            if (TransformChanged != null)
                TransformChanged(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when transform changed.
        /// </summary>
        public event EventHandler<EventArgs> TransformChanged;
        
        /// <summary>
        /// Gets the absolute transform of this drawable.
        /// </summary>
        [ContentSerializerIgnore()]
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
                }
                return absoluteTransform; 
            } 
        }
        private Matrix absoluteTransform = Matrix.Identity;
        private bool isAbsoluteTransformDirty = false; 

        private void MarkAbsoluteTransformsDirty()
        {
            isAbsoluteTransformDirty = true;
            ContainerTraverser.Traverse<Transformable>(this, transformable =>
            {
                transformable.isAbsoluteTransformDirty = true;
                transformable.OnTransformChanged();
                return TraverseOptions.Continue;
            });
        }
        #endregion

        #region IContainer
        IContainer IContainedObject.Parent
        {
            get { return Parent; }
        }

        int IContainer.Count
        {
            get { return ChildCount; }
        }

        /// <summary>
        /// Gets the number of child objects
        /// </summary>
        protected virtual int ChildCount { get { return 0; } }

        /// <summary>
        /// Copies all the child objects to the target array.
        /// </summary>
        public virtual void CopyTo(object[] array, int startIndex) { }
        #endregion

        #region ToString
        /// <summary>
        /// Returns a <see cref="System.String"/> that represents this instance.
        /// </summary>
        public override string ToString()
        {
            return Name != null ? Name.ToString() : base.ToString();
        }
        #endregion
    }
}