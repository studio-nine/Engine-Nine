#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines a graphics component that can be added to a game object container.
    /// </summary>
    [Serializable]
    public class GraphicsComponent : Component, ITransformable, IServiceProvider, ICloneable
    {
        /// <summary>
        /// Gets or sets the view template of this graphics component.
        /// </summary>
        [XmlAttribute]
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
        /// Gets or sets the transform.
        /// </summary>
        public Matrix Transform
        {
            get { return DisplayObject != null ? DisplayObject.Transform : Matrix.Identity; }
            set { if (DisplayObject != null) DisplayObject.Transform = value; }
        }

        /// <summary>
        /// Gets the graphics object owned by this graphics component.
        /// </summary>
        [XmlIgnore]
        public DisplayObject DisplayObject { get; private set; }

        /// <summary>
        /// Gets the scene that contains the graphics object owned by this graphics component.
        /// </summary>
        [XmlIgnore]
        public Scene Scene { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsComponent"/> class.
        /// </summary>
        public GraphicsComponent() { }

        /// <summary>
        /// Called when this game object is added to a parent container.
        /// </summary>
        protected override void OnAdded(WorldObject parent)
        {
            if (Scene != null || DisplayObject != null)
                throw new InvalidOperationException();

            CreateGraphicsObject();
        }

        /// <summary>
        /// Called when this game object is removed from a parent container.
        /// </summary>
        protected override void OnRemoved(WorldObject parent)
        {
            if (Scene == null && DisplayObject != null)
                throw new InvalidOperationException();

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
                    var objectFactory = Parent.Find<IObjectFactory>();
                    if (objectFactory == null)
                        throw new InvalidOperationException(string.Format(Strings.ServiceNotFound, typeof(IObjectFactory)));
                    DisplayObject = objectFactory.Create<DisplayObject>(Template);
                    DisplayObject.Transform = Parent.Transform;
                    Scene.Add(DisplayObject);
                }
            }
        }

        /// <summary>
        /// Destroys the graphics object.
        /// </summary>
        internal void DestroyGraphicsObject()
        {
            if (Scene != null && DisplayObject != null)
            {
                Scene.Remove(DisplayObject);
                var disposable = DisplayObject as IDisposable;
                if (disposable != null)
                    disposable.Dispose();
                DisplayObject = null;
                Scene = null;
            }
        }

        object IServiceProvider.GetService(Type serviceType)
        {
            if (DisplayObject != null && serviceType.IsAssignableFrom(typeof(DisplayObject)))
                return DisplayObject;
            return null;
        }

        public GraphicsComponent Clone()
        {
            return new GraphicsComponent()
            {
                Name = Name,
                Tag = Tag,
                Template = Template,
            };
        }

        object ICloneable.Clone()
        {
            return Clone();
        }
    }
}
