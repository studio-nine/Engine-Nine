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
    /// Defines a display object that contains a list of objects.
    /// </summary>
    /// <remarks>
    /// This class serves as a container to composite other objects.
    /// If you wish to create your own display object, derive from <c>IGraphicsObject</c> instead.
    /// </remarks>
    public sealed class DisplayObject : Transformable, IEnumerable<object>, INotifyCollectionChanged<object>, IDisposable
    {
        #region Children
        /// <summary>
        /// Gets the child drawable owned used by this drawable.
        /// </summary>
        [ContentSerializer(Optional=true)]
        public IList<object> Children
        {
            get { return children; }

            // To be used by content reader
            internal set
            {
                children.Clear();
                children.AddRange(value);
            }
        }
        private NotificationCollection<object> children;

        void children_Added(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            ISpatialQueryable boundable = e.Value as ISpatialQueryable;
            if (boundable != null)
                boundable.BoundingBoxChanged += new EventHandler<EventArgs>(boundable_BoundingBoxChanged);

            Transformable transformable = e.Value as Transformable;
            if (transformable != null)
            {
                if (transformable.Parent != null)
                    throw new InvalidOperationException("The object is already added to a display object.");
                transformable.Parent = this;
            }
        }

        void children_Removed(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            ISpatialQueryable boundable = e.Value as ISpatialQueryable;
            if (boundable != null)
                boundable.BoundingBoxChanged -= new EventHandler<EventArgs>(boundable_BoundingBoxChanged);

            Transformable transformable = e.Value as Transformable;
            if (transformable != null)
            {
                if (transformable.Parent == null)
                    throw new InvalidOperationException("The object does not belong to this display object.");
                transformable.Parent = null;
            }
        }
        #endregion

        #region BoundingBox
        public override BoundingBox BoundingBox
        {
            get 
            {
                if (boundingBoxDirty)
                {
                    boundingBox = BoundingBoxExtensions.CreateMerged(children.OfType<IBoundable>().Select(b => b.BoundingBox));
                    boundingBoxDirty = false;
                }
                return boundingBox; 
            }
        }

        BoundingBox boundingBox;
        bool boundingBoxDirty;

        void boundable_BoundingBoxChanged(object sender, EventArgs e)
        {
            boundingBoxDirty = true;
        }
        #endregion

        /// <summary>
        /// Initializes a new instance of <c>DisplayObject</c>.
        /// </summary>
        public DisplayObject()
        {
            children = new NotificationCollection<object>();
            children.Sender = this;
            children.Added += (sender, e) => { if (Added != null) Added(sender, e); };
            children.Removed += (sender, e) => { if (Removed != null) Removed(sender, e); };
        }

        #region IEnumerable
        public IEnumerator<object> GetEnumerator()
        {
            return Children.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region IDisposable
        public void Dispose()
        {
            foreach (var child in Children)
            {
                IDisposable disposable = child as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
            }
        }
        #endregion

        public event EventHandler<NotifyCollectionChangedEventArgs<object>> Added;
        public event EventHandler<NotifyCollectionChangedEventArgs<object>> Removed;
    }
}