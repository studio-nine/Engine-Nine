#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ObjectModel;
using Nine.Components;

#endregion

namespace Nine
{
    /// <summary>
    /// Defines a world that contains objects to be updated and rendered.
    /// </summary>
    [ContentProperty("WorldObjects")]
    public class World : GameObjectContainer, IGameObjectContainer, IObjectFactory, IUpdateable, IDrawable, IEnumerable, IServiceProvider
    {
        #region Templates
        /// <summary>
        /// Gets a collection of templates owned by this world.
        /// </summary>
        [ContentSerializerIgnore]
        public ICollection<Template> Templates { get; set; }

        /// <summary>
        /// Creates an object from a type name.
        /// </summary>
        public object Create(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            object result = null;
            foreach (var template in Templates)
            {
                if (template != null && ((result = template.CreateInstance()) != null))
                    return result;
            }
            return null;
        }

        /// <summary>
        /// Creates an object from a type name.
        /// </summary>
        public T Create<T>(string typeName)
        {
            if (string.IsNullOrEmpty(typeName))
                throw new ArgumentNullException("typeName");

            object result = null;
            foreach (var template in Templates)
            {
                if (template != null && ((result = template.CreateInstance<T>()) is T))
                    return (T)result;
            }
            return default(T);
        }
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of <c>World</c>.
        /// </summary>
        public World()
        {
            IsRoot = true;
            worldObjects = new NotificationCollection<object>() { Sender = this, EnableManipulationWhenEnumerating = true };
            worldObjects.Added += new EventHandler<NotifyCollectionChangedEventArgs<object>>(OnAdded);
            worldObjects.Removed += new EventHandler<NotifyCollectionChangedEventArgs<object>>(OnRemoved);
            Templates = new List<Template>();
            Services = new GameServiceContainer();
            Commands = new GameCommandExecutor();
        }
        #endregion

        #region WorldObjects
        /// <summary>
        /// Gets a collection of world objects managed by this world.
        /// </summary>
        [ContentSerializer]
        public NotificationCollection<object> WorldObjects
        {
            get { return worldObjects; }
        }
        private NotificationCollection<object> worldObjects;

        /// <summary>
        /// Called when this world is added to a parent container.
        /// </summary>
        protected override sealed void OnAdded(IGameObjectContainer parent)
        {
            base.OnAdded(parent);
        }

        /// <summary>
        /// Called when this world is removed from a parent container.
        /// </summary>
        protected override sealed void OnRemoved(IGameObjectContainer parent)
        {
            base.OnRemoved(parent);
        }

        /// <summary>
        /// Called when a child object is added to this world.
        /// </summary>
        protected virtual void OnAdded(object child) { }

        /// <summary>
        /// Called when a child object is removed from this world.
        /// </summary>
        protected virtual void OnRemoved(object child) { }

        private void OnAdded(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            if (e.Value is IGameObject)
            {
                ((IGameObject)e.Value).Parent = this;
            }
            OnAdded(e.Value); 
        }

        private void OnRemoved(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            if (e.Value is IGameObject)
            {
                ((IGameObject)e.Value).Parent = null;
            }
            OnRemoved(e.Value); 
        }
        #endregion

        #region Scene
        /// <summary>
        /// Gets or sets the graphics scene used by this world.
        /// </summary>
        public Scene Scene { get; private set; }

        /// <summary>
        /// Creates the graphics scene to render this world.
        /// </summary>
        public void CreateGraphics(GraphicsDevice graphics)
        {
            CreateGraphics(graphics, null, null);
        }

        /// <summary>
        /// Creates the graphics scene to render this world.
        /// </summary>
        public void CreateGraphics(GraphicsDevice graphics, GraphicsSettings settings, ISceneManager<ISpatialQueryable> sceneManager)
        {
            UpdateScene(new Scene(graphics, settings, sceneManager));
        }

        /// <summary>
        /// Destroies the graphics of this world.
        /// </summary>
        public void DestroyGraphics()
        {
            UpdateScene(null);
        }

        private void UpdateScene(Scene newScene)
        {
            if (Scene != null)
            {
                this.ForEachRecursive<GraphicsComponent>(graphicsComponent =>  graphicsComponent.DestroyGraphicsObject());
                Scene.Dispose();
                Services.RemoveService(typeof(Scene));
                Services.RemoveService(typeof(GraphicsDevice));
                Services.RemoveService(typeof(IGraphicsDeviceService));
            }

            Scene = newScene;

            if (Scene != null)
            {
                Services.AddService(typeof(Scene), Scene);
                Services.AddService(typeof(GraphicsDevice), newScene.GraphicsDevice);
                Services.AddService(typeof(IGraphicsDeviceService), new GraphicsDeviceServiceProvider(newScene.GraphicsDevice));
                this.ForEachRecursive<GraphicsComponent>(graphicsComponent => graphicsComponent.CreateGraphicsObject());
            }
        }
        #endregion

        #region Content
        /// <summary>
        /// Gets the content manager used by this world.
        /// </summary>
        public ContentManager Content { get; private set; }

        /// <summary>
        /// Creates the content manager used by this world.
        /// </summary>
        public void CreateContent(ContentManager content)
        {
            if (content == null)
                throw new ArgumentNullException("content");
            Content = content;
            Services.AddService(typeof(ContentManager), Content);
        }

        /// <summary>
        /// Destroys the content manager.
        /// </summary>
        public void DestroyContent()
        {
            Content = null;
            Services.RemoveService(typeof(ContentManager));
        }
        #endregion

        #region Commands
        /// <summary>
        /// Gets the command queue currently been executed by this world.
        /// </summary>
        public GameCommandExecutor Commands { get; set; }
        #endregion

        #region Services
        /// <summary>
        /// Gets the services used by this world.
        /// </summary>
        public GameServiceContainer Services { get; private set; }

        object IServiceProvider.GetService(Type serviceType)
        {
            return Services.GetService(serviceType);
        }
        #endregion

        #region Find
        /// <summary>
        /// Find the first feature of type T owned by this game object container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override T Find<T>()
        {
            var result = Services.GetService<T>();
            if ((object)result != null)
                return result;
            result = base.Find<T>();
            if ((object)result == null)
            {
                for (int i = 0; i < worldObjects.Count; i++)
                {
                    var obj = worldObjects[i];
                    if (obj is T)
                        return (T)obj;
                }
            }
            return default(T);
        }

        /// <summary>
        /// Find all the feature of type T owned by this game object container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public override IEnumerable<T> FindAll<T>()
        {
            var result = Services.GetService<T>();
            if ((object)result != null)
                yield return result;
            foreach (var baseResult in base.FindAll<T>())
                yield return baseResult;
            for (int i = 0; i < worldObjects.Count; i++)
            {
                var obj = worldObjects[i];
                if (obj is T)
                    yield return (T)obj;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Components.Concat(worldObjects).GetEnumerator();
        }
        #endregion

        #region Update & Draw
        private TimeEventArgs timeEventArgs = new TimeEventArgs();

        /// <summary>
        /// Occurs when the world is updating itself.
        /// </summary>
        public event EventHandler<TimeEventArgs> Updating;

        /// <summary>
        /// Occurs when the world is drawing itself.
        /// </summary>
        public event EventHandler<TimeEventArgs> Drawing;

        /// <summary>
        /// Updates all the objects managed by this world.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
            Commands.Update(this, elapsedTime);

            if (Updating != null)
            {
                timeEventArgs.ElapsedTime = elapsedTime;
                Updating(this, timeEventArgs);
            }
        }

        /// <summary>
        /// Draws all the objects managed by this world.
        /// </summary>
        public void Draw(TimeSpan elapsedTime)
        {
            if (Scene != null)
                Scene.Draw(elapsedTime);

            if (Drawing != null)
            {
                timeEventArgs.ElapsedTime = elapsedTime;
                Drawing(this, timeEventArgs);
            }
        }
        #endregion
    }
}
