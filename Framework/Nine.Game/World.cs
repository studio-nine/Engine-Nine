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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Nine.Graphics.ObjectModel;

#endregion

namespace Nine
{
    /// <summary>
    /// Defines a world that contains objects to be updated and rendered.
    /// </summary>
    public class World : GameObjectContainer, IGameObjectContainer, IObjectFactory, IUpdateable, IDrawable
    {
        /// <summary>
        /// Gets a collection of templates owned by this world.
        /// </summary>
        [XmlArrayItem("Template")]
        public ICollection<Template> Templates { get; set; }

        /// <summary>
        /// Gets a collection of world objects managed by this world.
        /// </summary>
        [XmlArrayItem("WorldObject")]
        public NotificationCollection<object> WorldObjects
        {
            get { return worldObjects; } 
        }

        /// <summary>
        /// Gets the graphics scene used by this world.
        /// </summary>
        public Scene Scene { get; private set; }

        private TimeEventArgs timeEventArgs = new TimeEventArgs();
        private NotificationCollection<object> worldObjects;
        
        /// <summary>
        /// Occurs when the world is updating itself.
        /// </summary>
        public event EventHandler<TimeEventArgs> Updating;

        /// <summary>
        /// Occurs when the world is drawing itself.
        /// </summary>
        public event EventHandler<TimeEventArgs> Drawing;

        /// <summary>
        /// Initializes a new instance of <c>World</c>.
        /// </summary>
        public World()
        {
            worldObjects = new NotificationCollection<object>() { Sender = this, EnableManipulationWhenEnumerating = true };
            worldObjects.Added += new EventHandler<NotifyCollectionChangedEventArgs<object>>(OnAdded);
            worldObjects.Removed += new EventHandler<NotifyCollectionChangedEventArgs<object>>(OnRemoved);
        }
        
        protected override sealed void OnAdded(IGameObjectContainer parent)
        {
            base.OnAdded(parent);
        }

        protected override sealed void OnRemoved(IGameObjectContainer parent)
        {
            base.OnRemoved(parent);
        }

        protected virtual void OnAdded(object child) { }
        protected virtual void OnRemoved(object child) { }

        private void OnAdded(object sender, NotifyCollectionChangedEventArgs<object> e) { OnAdded(e.Value); }
        private void OnRemoved(object sender, NotifyCollectionChangedEventArgs<object> e) { OnRemoved(e.Value); }

        /// <summary>
        /// Updates all the objects managed by this world.
        /// </summary>
        public void Update(TimeSpan elapsedTime)
        {
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
            if (Drawing != null)
            {
                timeEventArgs.ElapsedTime = elapsedTime;
                Drawing(this, timeEventArgs);
            }
        }

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
            /*
            object result = null;
            foreach (var template in Templates)
            {
                if (template != null && ((result = template.Create(typeName)) is T))
                    return result as T;
            }*/
            return default(T);
        }

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
            try
            {
                Serialization.CreateSerializer(typeof(World)).Serialize(stream, this);
            }
            catch (InvalidOperationException e)
            {
                throw e.InnerException;
            }
        }
        #endregion
    }
}
