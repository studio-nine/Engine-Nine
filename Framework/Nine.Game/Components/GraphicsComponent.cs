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
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Components
{
    /// <summary>
    /// Defines a graphics component that can be added to a game object container.
    /// </summary>
    public class GraphicsComponent : GameObject, IUpdateable
    {
        /// <summary>
        /// Gets or sets the view template of this graphics component.
        /// </summary>
        public string Template
        {
            get { return template; }
            set
            {
                if (template != value)
                {
                    DestroyGraphicsObject();
                    template = value;
                    CreateGraphicsObject();
                }
            }
        }
        private string template;

        /// <summary>
        /// Gets the graphics object owned by this graphics component.
        /// </summary>
        public Transformable GraphicsObject { get; private set; }

        /// <summary>
        /// Gets the scene that contains the graphics object owned by this graphics component.
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsComponent"/> class.
        /// </summary>
        public GraphicsComponent() { }

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

            CreateGraphicsObject();
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

        /// <summary>
        /// Creates the graphics object.
        /// </summary>
        internal void CreateGraphicsObject()
        {
            if (Parent != null)
            {
                Scene = Parent.Find<Scene>();
                if (Scene != null && !string.IsNullOrEmpty(Template))
                {
                    var contentManager = Parent.Find<ContentManager>();
                    if (contentManager == null)
                        throw new InvalidOperationException("Cannot find ContentManager when creating graphics object");
                    GraphicsObject = contentManager.Create<Transformable>(Template);
                    Scene.Add(GraphicsObject);
                }
            }
        }

        /// <summary>
        /// Destroys the graphics object.
        /// </summary>
        internal void DestroyGraphicsObject()
        {
            if (Scene != null && GraphicsObject != null)
            {
                Scene.Remove(GraphicsObject);
                var disposable = GraphicsObject as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                GraphicsObject = null;
                Scene = null;
            }
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
}
