#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using System.Reflection;
using System.Windows.Markup;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a world that contains objects to be updated and rendered.
    /// </summary>
    [ContentProperty("WorldObjects")]    
    public sealed class World : IServiceProvider, IUpdateable, IDrawable
    {
        #region Constructor
        /// <summary>
        /// Initializes a new instance of <c>World</c>.
        /// </summary>
        public World()
        {
            Services = new GameServiceContainer();
            Prototypes = new Dictionary<string, object>();

            worldObjects = new NotificationCollection<WorldObject>() { Sender = this, EnableManipulationWhenEnumerating = true };
            worldObjects.Added += new EventHandler<NotifyCollectionChangedEventArgs<WorldObject>>(OnAdded);
            worldObjects.Removed += new EventHandler<NotifyCollectionChangedEventArgs<WorldObject>>(OnRemoved);
        }
        #endregion

        #region Version
        /// <summary>
        /// Gets or sets the version of this world.
        /// </summary>
        [XmlAttribute()]
        internal string Version
        {
            get
            {
                AssemblyName name = new AssemblyName(Assembly.GetExecutingAssembly().FullName);
                return name.Version.ToString();
            }
            set 
            {
                if (Version != value)
                    throw new InvalidOperationException("Version mismatch.");
            }
        }
        #endregion

        #region Prototypes
        /// <summary>
        /// Gets a dictionary of prototypes that can be created though the IObjectFactory service.
        /// </summary>
        public IDictionary<string, object> Prototypes { get; private set; }
        #endregion

        #region WorldObjects
        /// <summary>
        /// Gets a collection of world objects managed by this world.
        /// </summary>
        public NotificationCollection<WorldObject> WorldObjects
        {
            get { return worldObjects; }
        }
        private NotificationCollection<WorldObject> worldObjects;

        private void OnAdded(object sender, NotifyCollectionChangedEventArgs<WorldObject> e)
        {
            e.Value.World = this;
        }

        private void OnRemoved(object sender, NotifyCollectionChangedEventArgs<WorldObject> e)
        {
            e.Value.World = null;
        }
        #endregion

        #region Content
        /// <summary>
        /// Creates the content manager used by this world.
        /// </summary>
        public void CreateContent(ContentManager content)
        {
            if (content == null)
                throw new ArgumentNullException("content");

            Services.AddService(typeof(ContentManager), content);
            Services.AddService(typeof(IObjectFactory), new ObjectFactory(Prototypes, content));
        }

        /// <summary>
        /// Destroys the content manager.
        /// </summary>
        public void DestroyContent()
        {
            Services.RemoveService(typeof(ContentManager));
            Services.RemoveService(typeof(IObjectFactory));
        }
        #endregion

        #region Services
        /// <summary>
        /// Gets the services used by this world.
        /// </summary>
        [XmlIgnore]
        public GameServiceContainer Services { get; private set; }

        /// <summary>
        /// Gets the service.
        /// </summary>
        public object GetService(Type type)
        {
            if (type.IsAssignableFrom(GetType()))
                return this;

            var service = Services.GetService(type);
            if (service != null)
                return service;

            return null;
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
            foreach (var worldObject in worldObjects)
            {
                var updatable = worldObject as IUpdateable;
                if (updatable != null)
                    updatable.Update(elapsedTime);
            }

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
            foreach (var worldObject in worldObjects)
            {
                var drawable = worldObject as IDrawable;
                if (drawable != null)
                    drawable.Draw(elapsedTime);
            }

            if (Drawing != null)
            {
                timeEventArgs.ElapsedTime = elapsedTime;
                Drawing(this, timeEventArgs);
            }
        }
        #endregion

        #region Serialization
        /// <summary>
        /// Loads the world from file.
        /// </summary>
        public static World FromFile(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Open))
            {
                return FromStream(stream);
            }
        }

        /// <summary>
        /// Loads the world from a stream.
        /// </summary>
        public static World FromStream(Stream stream)
        {
            try
            {
                return (World)Serialization.CreateSerializer(typeof(World)).Deserialize(stream);
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }
        }

        /// <summary>
        /// Saves the world to a file.
        /// </summary>
        public void Save(string filename)
        {
            using (var stream = File.Open(filename, FileMode.Create))
            {
                Save(stream);
            }
        }

        /// <summary>
        /// Saves the world to a stream.
        /// </summary>
        public void Save(Stream stream)
        {
            Serialization.CreateSerializer(typeof(World)).Serialize(stream, this);
        }
        #endregion
    }
}
