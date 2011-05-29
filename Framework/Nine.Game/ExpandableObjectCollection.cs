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
#endregion

namespace Nine
{
    public class ExtensibleObjectCollection<T> : ICollection<T>
    {
        List<T> innerCollection;
        Dictionary<Type, ExtensibleObjectCollectionEntry<T>> extensionDataCollections;

        public ExtensibleObjectCollection()
        {
            innerCollection = new List<T>();
        }

        public ExtensibleObjectCollection(IEnumerable<T> collection)
        {
            innerCollection = new List<T>(collection);
        }

        public ExtensibleObjectCollection(int capacity)
        {
            innerCollection = new List<T>(capacity);
        }

        public void Attach<TExpansion>() where TExpansion : new()
        {
            Attach(typeof(TExpansion));
        }

        public void Attach(Type expansionType)
        {
            Attach(expansionType, (Func<T, object>)null);
        }

        public void Attach(Type expansionType, Func<T, object> createExpansion)
        {
            ExtensibleObjectCollectionEntry<T> entry;

            if (expansionType == null)
                throw new ArgumentNullException("expansionType");
            
            if (extensionDataCollections == null)
                extensionDataCollections = new Dictionary<Type, ExtensibleObjectCollectionEntry<T>>();

            if (!extensionDataCollections.TryGetValue(expansionType, out entry))
            {
                extensionDataCollections.Add(expansionType, entry = new ExtensibleObjectCollectionEntry<T>()
                {
                    Type = expansionType,
                    Create = createExpansion,
                    List = new List<object>(innerCollection.Count),
                });
            }

            for (int i = 0; i < innerCollection.Count; i++)
            {
                entry.List.Add(CreateObject(entry, innerCollection[i]));
            }
        }

        public bool IsAttached<TExpansion>()
        {
            return IsAttached(typeof(TExpansion));
        }

        public bool IsAttached(Type expansionType)
        {
            return extensionDataCollections != null && extensionDataCollections.ContainsKey(expansionType);
        }
        public void Detach<TExpansion>()
        {
            Detach(typeof(TExpansion));
        }
        
        public void Detach(Type expansionType)
        {
            ExtensibleObjectCollectionEntry<T> entry;
            if (extensionDataCollections != null && 
                extensionDataCollections.TryGetValue(expansionType, out entry))
            {
                extensionDataCollections.Remove(expansionType);
            }
        }

        public System.Collections.IEnumerable GetExtensionData<TExpansion>()
        {
            return GetExtensionData(typeof(TExpansion));
        }

        public System.Collections.IEnumerable GetExtensionData(Type expansionType)
        {
            ExtensibleObjectCollectionEntry<T> entry;
            if (extensionDataCollections != null &&
                extensionDataCollections.TryGetValue(expansionType, out entry))
            {
                return entry.List;
            }
            return System.Linq.Enumerable.Empty<object>();
        }

        private object CreateObject(ExtensibleObjectCollectionEntry<T> entry, T item)
        {
            return entry.Create != null ? entry.Create(item) : Activator.CreateInstance(entry.Type);
        }

        #region ICollection
        public void Add(T item)
        {
            innerCollection.Add(item);
            if (extensionDataCollections != null)
            {
                foreach (var entry in extensionDataCollections.Values)
                {
                    entry.List.Add(CreateObject(entry, item));
                }
            }
        }

        public void AddRange(IEnumerable<T> collection)
        {
            foreach (var item in collection)
                Add(item);
        }

        public bool Remove(T item)
        {
            int index = innerCollection.IndexOf(item);
            if (index >= 0)
            {
                innerCollection.RemoveAt(index);
                if (extensionDataCollections != null)
                {
                    foreach (var entry in extensionDataCollections.Values)
                        entry.List.RemoveAt(index);
                }
                return true;
            }
            return false;
        }

        public void Clear()
        {
            innerCollection.Clear();
            if (extensionDataCollections != null)
            {
                foreach (var entry in extensionDataCollections.Values)
                    entry.List.Clear();
            }
        }

        public bool Contains(T item)
        {
            return innerCollection.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            innerCollection.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return innerCollection.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return innerCollection.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return innerCollection.GetEnumerator();
        }
        #endregion
    }

    class ExtensibleObjectCollectionEntry<T>
    {
        public Type Type;
        public List<object> List;
        public Func<T, object> Create;
    }
}
