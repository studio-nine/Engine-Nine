#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Nine.Navigation;
#endregion

namespace Nine.Components
{/*
    /// <summary>
    /// Defines a navigation component that can be added to a game object container.
    /// </summary>
    public class NavigationComponent : GameObject, IUpdateable
    {
        /// <summary>
        /// Gets or sets the navigator that drives the parent object.
        /// </summary>
        public Navigator Navigator { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationComponent"/> class.
        /// </summary>
        public NavigationComponent() { }

        /// <summary>
        /// Called when this game object is added to a parent container.
        /// </summary>
        protected override void OnAdded(IGameObjectContainer parent)
        {
            if (Scene != null || GraphicsObject != null)
                throw new InvalidOperationException();

            var worldObject = Parent as WorldObject;
            if (worldObject != null)
                worldObject.TransformChanged += new EventHandler<EventArgs>(parent_TransformChanged);

            var freeObject = Parent as FreeObject;
            if (freeObject != null)
                freeObject.TransformChanged += new EventHandler<EventArgs>(parent_TransformChanged);
        }

        /// <summary>
        /// Called when this game object is removed from a parent container.
        /// </summary>
        protected override void OnRemoved(IGameObjectContainer parent)
        {
            if (Scene == null && GraphicsObject != null)
                throw new InvalidOperationException();

            var worldObject = Parent as WorldObject;
            if (worldObject != null)
                worldObject.TransformChanged -= new EventHandler<EventArgs>(parent_TransformChanged);

            var freeObject = Parent as FreeObject;
            if (freeObject != null)
                freeObject.TransformChanged -= new EventHandler<EventArgs>(parent_TransformChanged);

            DestroyGraphicsObject();
        }

        void IUpdateable.Update(TimeSpan elapsedTime)
        {
            if (Parent != null && Scene != null && GraphicsObject != null)
            {
                Update(elapsedTime);
            }
        }

        void parent_TransformChanged(object sender, EventArgs e)
        {
            transformNeedsUpdate = true;
        }

        bool transformNeedsUpdate = true;

        protected virtual void Update(TimeSpan elapsedTime)
        {
            if (transformNeedsUpdate)
            {
                var worldObject = Parent as WorldObject;
                if (worldObject != null)
                    GraphicsObject.Transform = worldObject.Transform;

                var freeObject = Parent as FreeObject;
                if (freeObject != null)
                    GraphicsObject.Transform = freeObject.Transform;

                transformNeedsUpdate = false;
            }
        }
    }
  */
}
