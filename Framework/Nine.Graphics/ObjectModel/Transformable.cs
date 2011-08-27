#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Base class for all objects that has a transform and a bounds.
    /// </summary>
    public abstract class Transformable : ISpatialQueryable
    {
        /// <summary>
        /// Gets the parent of this object.
        /// </summary>
        public Transformable Parent { get; internal set; }

        #region BoundingBox
        /// <summary>
        /// Gets the axis aligned bounding box in world space.
        /// </summary>
        public abstract BoundingBox BoundingBox { get; }

        /// <summary>
        /// Called by derived classes to raise BoundingBoxChanged event.
        /// </summary>
        protected virtual void OnBoundingBoxChanged()
        {
            if (BoundingBoxChanged != null)
                BoundingBoxChanged(this, EventArgs.Empty);
        }

        object ISpatialQueryable.SpatialData { get; set; }

        /// <summary>
        /// Occurs when the bounding box changed.
        /// </summary>
        public event EventHandler<EventArgs> BoundingBoxChanged;
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
                OnTransformChanged();
                OnBoundingBoxChanged();
                absoluteTransform = transform;
                MarkChildrenAbsoluteTransformsDirty();
            }
        }
        private Matrix transform = Matrix.Identity;

        /// <summary>
        /// Called when transform changed.
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
                        throw new InvalidOperationException();
                    absoluteTransform = transform * Parent.AbsoluteTransform;
                }
                return absoluteTransform; 
            } 
        }
        private Matrix absoluteTransform = Matrix.Identity;
        private bool isAbsoluteTransformDirty = false; 

        private void MarkChildrenAbsoluteTransformsDirty()
        {
            System.Collections.IEnumerable enumerable = this as System.Collections.IEnumerable;
            if (enumerable != null)
            {
                foreach (var child in enumerable)
                {
                    Transformable transformable = child as Transformable;
                    if (transformable != null)
                    {
                        transformable.isAbsoluteTransformDirty = true;
                        transformable.MarkChildrenAbsoluteTransformsDirty();
                    }
                }
            }
        }
        #endregion
    }
}