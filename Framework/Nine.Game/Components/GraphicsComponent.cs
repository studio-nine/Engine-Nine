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
    public class GraphicsComponent : GameObject
    {
        /// <summary>
        /// Gets or sets the view template of this graphics component.
        /// </summary>
        public string ViewTemplate
        {
            get { return viewTemplate; }
            set
            {
                if (viewTemplate != value)
                {
                    DestroyGraphicsObject();
                    viewTemplate = value;
                    CreateGraphicsObject();
                }
            }
        }
        private string viewTemplate;

        /// <summary>
        /// Gets the graphics object owned by this graphics component.
        /// </summary>
        [XmlIgnore]
        public ISpatialQueryable GraphicsObject { get; private set; }

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
            Scene = parent.Find<Scene>();
            CreateGraphicsObject();
        }

        /// <summary>
        /// Called when this game object is removed from a parent container.
        /// </summary>
        protected override void OnRemoved(IGameObjectContainer parent)
        {
            if (Scene == null && GraphicsObject != null)
                throw new InvalidOperationException();
            DestroyGraphicsObject();
           Scene = null;
        }

        private void CreateGraphicsObject()
        {
            if (Scene != null && !string.IsNullOrEmpty(ViewTemplate))
            {
                var contentManager = Parent.Find<ContentManager>();
                if (contentManager != null)
                {
                    GraphicsObject = contentManager.Create<ISpatialQueryable>(ViewTemplate);
                    Scene.Add(GraphicsObject);
                }
            }
        }

        private void DestroyGraphicsObject()
        {
            if (Scene != null && GraphicsObject != null)
            {
                Scene.Remove(GraphicsObject);
                var disposable = GraphicsObject as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                GraphicsObject = null;
            }
        }
    }
}
