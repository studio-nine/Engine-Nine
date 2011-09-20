#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a basic game object that can be added to a parent container.
    /// </summary>
    public class GameObject : IGameObject
    {
        /// <summary>
        /// Gets or sets the parent of this game object.
        /// </summary>
        public IGameObjectContainer Parent
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
    public class GameObjectContainer : GameObject, IGameObjectContainer
    {
        [XmlArrayItem("Component")]
        public NotificationCollection<object> Components
        {
            get 
            {
                if (components == null)
                {
                    components = new NotificationCollection<object>();
                    components.Added += (sender, e) => { if (e.Value is IGameObject) ((IGameObject)e.Value).Parent = this; };
                    components.Removed += (sender, e) => { if (e.Value is IGameObject) ((IGameObject)e.Value).Parent = null; };
                }
                return components;
            }
        }
        private NotificationCollection<object> components;

        public T Find<T>()
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

        public IEnumerable<T> FindAll<T>()
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
    }
}