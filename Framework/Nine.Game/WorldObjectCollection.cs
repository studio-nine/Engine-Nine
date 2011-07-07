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
using System.Diagnostics;
using System.Xml.Serialization;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines a collection that can have any custom data attached.
    /// </summary>
    public class WorldObjectCollection<T> : NotificationCollection<T>
    {
        Dictionary<Type, WorldObjectCollectionEntry<T>> extensionDataCollections;

        internal WorldObjectCollection() { }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Attach<TExpansion>(object state) where TExpansion : new()
        {
            Attach(typeof(TExpansion), state);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Attach(Type expansionType, object state)
        {
            Attach(expansionType, (Func<T, object>)null);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Attach(Type expansionType, Func<T, object> createExpansion, object state)
        {
            WorldObjectCollectionEntry<T> entry;

            if (expansionType == null)
                throw new ArgumentNullException("expansionType");
            
            if (extensionDataCollections == null)
                extensionDataCollections = new Dictionary<Type, WorldObjectCollectionEntry<T>>();

            if (!extensionDataCollections.TryGetValue(expansionType, out entry))
            {
                extensionDataCollections.Add(expansionType, entry = new WorldObjectCollectionEntry<T>()
                {
                    State = state,
                    Type = expansionType,
                    Create = createExpansion,
                    List = new List<object>(Count),
                });
            }

            for (int i = 0; i < Count; i++)
            {
                entry.List.Add(CreateObject(entry, this[i]));
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsAttached<TExpansion>()
        {
            return IsAttached(typeof(TExpansion));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsAttached(Type expansionType)
        {
            return extensionDataCollections != null && extensionDataCollections.ContainsKey(expansionType);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public TState GetAttachedState<TExpansion, TState>()
        {
            return (TState)GetAttachedState(typeof(TExpansion));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public object GetAttachedState(Type expansionType)
        {
            WorldObjectCollectionEntry<T> entry;
            if (extensionDataCollections != null && extensionDataCollections.TryGetValue(expansionType, out entry))
                return entry.State;
            return null;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Detach<TExpansion>()
        {
            Detach(typeof(TExpansion));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Detach(Type expansionType)
        {
            WorldObjectCollectionEntry<T> entry;
            if (extensionDataCollections != null && 
                extensionDataCollections.TryGetValue(expansionType, out entry))
            {
                extensionDataCollections.Remove(expansionType);
            }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Collections.IEnumerable GetExtensions<TExpansion>()
        {
            return GetExtensions(typeof(TExpansion));
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public System.Collections.IEnumerable GetExtensions(Type expansionType)
        {
            WorldObjectCollectionEntry<T> entry;
            if (extensionDataCollections != null &&
                extensionDataCollections.TryGetValue(expansionType, out entry))
            {
                return entry.List;
            }
            return System.Linq.Enumerable.Empty<object>();
        }

        private object CreateObject(WorldObjectCollectionEntry<T> entry, T item)
        {
            if (entry.Type.IsAssignableFrom(item.GetType()))
                return item;
            return entry.Create != null ? entry.Create(item) : Activator.CreateInstance(entry.Type);
        }

        protected override void OnAdded(int index, T value)
        {
            base.OnAdded(index, value);
            if (extensionDataCollections != null)
            {
                foreach (var entry in extensionDataCollections.Values)
                {
                    entry.List.Add(CreateObject(entry, value));
                }
            }
        }

        protected override void OnRemoved(int index, T value)
        {
            base.OnRemoved(index, value);
            if (extensionDataCollections != null)
            {
                foreach (var entry in extensionDataCollections.Values)
                    entry.List.RemoveAt(index);
            }
        }
    }

    class WorldObjectCollectionEntry<T>
    {
        public object State;
        public Type Type;
        public List<object> List;
        public Func<T, object> Create;
    }
}
