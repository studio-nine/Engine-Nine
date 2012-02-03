#region Copyright 2010 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

#endregion

namespace Nine
{
    /// <summary>
    /// Represents an adapter class that filters and converts the result of
    /// an existing <c>SceneManager</c>.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    public class SceneManager<TInput, TOutput> : ISceneManager<TOutput> where TOutput : TInput
    {
        ICollection<TInput> collection;
        SpatialQuery<TInput, TOutput> query;

        public event EventHandler<NotifyCollectionChangedEventArgs<TOutput>> Added;
        public event EventHandler<NotifyCollectionChangedEventArgs<TOutput>> Removed;

        public SceneManager(ISceneManager<TInput> sceneManager)
        {
            if (sceneManager == null)
                throw new ArgumentNullException("sceneManager");

            collection = sceneManager;
            query = new SpatialQuery<TInput, TOutput>(sceneManager);
        }

        #region ICollection<T>
        public void Add(TOutput item)
        {
            collection.Add(item);
            Count++;

            if (Added != null)
                Added(this, new NotifyCollectionChangedEventArgs<TOutput>(0, item));
        }

        public void Clear()
        {
            throw new NotSupportedException();
        }

        public bool Contains(TOutput item)
        {
            if (!(item is TInput))
                return false;

            return collection.Contains((TInput)(object)item);
        }

        public void CopyTo(TOutput[] array, int arrayIndex)
        {
            throw new NotSupportedException();
        }

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(TOutput item)
        {
            bool result = false;
            if (result = collection.Remove(item))
            {
                Count--;
                if (Removed != null)
                    Removed(this, new NotifyCollectionChangedEventArgs<TOutput>(0, item));
            }
            return result;
        }

        public IEnumerator<TOutput> GetEnumerator()
        {
            return collection.OfType<TOutput>().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region ISpatialQuery<T>
        public void FindAll(ref BoundingSphere boundingSphere, ICollection<TOutput> result)
        {
            query.FindAll(ref boundingSphere, result);
        }

        public void FindAll(ref Ray ray, ICollection<TOutput> result)
        {
            query.FindAll(ref ray, result);
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<TOutput> result)
        {
            query.FindAll(ref boundingBox, result);
        }

        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<TOutput> result)
        {
            query.FindAll(ref boundingFrustum, result);
        }
        #endregion
    }
}