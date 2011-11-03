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
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Markup;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a basic game object that can be added to a parent container.
    /// </summary>
    [RuntimeNameProperty("Name")]
    [DictionaryKeyProperty("Name")]
    public class GameObject : IGameObject
    {
        /// <summary>
        /// Gets or sets the name of this game object.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the tag.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Gets the parent of this game object.
        /// </summary>
        public IGameObjectContainer Parent
        {
            get { return parent; }
        }

        IGameObjectContainer IGameObject.Parent
        {
            get { return parent; }
            set
            {
                if (value != null)
                {
                    if (parent != null)
                    {
                        throw new InvalidOperationException("This game object already belongs to a container");
                    }
                    parent = value;
                    OnAdded(parent);
                }
                else
                {
                    if (parent == null)
                    {
                        throw new InvalidOperationException("This game object does not belongs to the specified container");
                    }
                    OnRemoved(parent);
                    parent = null;
                }
            }
        }
        IGameObjectContainer parent;

        /// <summary>
        /// Called when this game object is added to a parent container.
        /// </summary>
        protected virtual void OnAdded(IGameObjectContainer parent) { }

        /// <summary>
        /// Called when this game object is removed from a parent container.
        /// </summary>
        protected virtual void OnRemoved(IGameObjectContainer parent) { }
    }

    /// <summary>
    /// Defines a basic game object that can contain a list of components.
    /// </summary>
    public class GameObjectContainer : GameObject, IGameObjectContainer, IEnumerable
    {
        #region Components
        /// <summary>
        /// Gets or sets whether this container is the root container.
        /// </summary>
        internal bool IsRoot;

        /// <summary>
        /// Gets a collection of components for this game object container.
        /// </summary>
        [XmlArrayItem("Component")]
        public NotificationCollection<object> Components
        {
            get 
            {
                if (components == null)
                {
                    components = new NotificationCollection<object>();
                    components.Added += new EventHandler<NotifyCollectionChangedEventArgs<object>>(components_Added);
                    components.Removed += new EventHandler<NotifyCollectionChangedEventArgs<object>>(components_Removed);
                }
                return components;
            }
        }
        private NotificationCollection<object> components;

        void components_Added(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            if ((IsRoot || Parent != null) && e.Value is IGameObject)
            {
                ((IGameObject)e.Value).Parent = this;
            }
        }

        void components_Removed(object sender, NotifyCollectionChangedEventArgs<object> e)
        {
            if ((IsRoot || Parent != null) && e.Value is IGameObject)
            {
                ((IGameObject)e.Value).Parent = null;
            }
        }

        /// <summary>
        /// Called when this game object is added to a parent container.
        /// </summary>
        protected override void OnAdded(IGameObjectContainer parent)
        {
            if (IsRoot)
            {
                throw new InvalidOperationException("Cannot add root object to a parent container");
            }

            if (components != null)
            {
                foreach (var gameObject in components.OfType<IGameObject>())
                {
                    gameObject.Parent = this;
                }
            }
        }

        /// <summary>
        /// Called when this game object is removed from a parent container.
        /// </summary>
        protected override void OnRemoved(IGameObjectContainer parent)
        {
            if (IsRoot)
            {
                throw new InvalidOperationException("Cannot remove root object from a parent container");
            }

            if (components != null)
            {
                foreach (var gameObject in components.OfType<IGameObject>())
                {
                    gameObject.Parent = null;
                }
            }
        }
        #endregion

        #region IGameObjectContainer
        /// <summary>
        /// Find the first feature of type T owned by this game object container.
        /// </summary>
        public virtual T Find<T>()
        {
            if (this is T)
                return (T)(object)this;
            if (components != null)
            {
                if (components is T)
                    return (T)(object)components;
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    if (component is T)
                        return (T)component;
                }
            }
            if (Parent != null)
            {
                var result = Parent.Find<T>();
                if (result is T)
                    return (T)result;
            }
            return default(T);
        }

        /// <summary>
        /// Find all the feature of type T owned by this game object container.
        /// </summary>
        public virtual IEnumerable<T> FindAll<T>()
        {
            if (this is T)
                yield return (T)(object)this;
            if (components != null)
            {
                if (components is T)
                    yield return (T)(object)components;
                for (int i = 0; i < components.Count; i++)
                {
                    var component = components[i];
                    if (component is T)
                        yield return (T)component;
                }
            }
            if (Parent != null)
            {
                foreach (var result in Parent.FindAll<T>())
                    yield return result;
            }
        }
        #endregion

        #region IEnumerable
        IEnumerator IEnumerable.GetEnumerator()
        {
            return components != null ? components.GetEnumerator() : Enumerable.Empty<object>().GetEnumerator();
        }
        #endregion
    }
}