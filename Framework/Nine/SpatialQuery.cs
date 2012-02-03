#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

#endregion

namespace Nine
{
    /// <summary>
    /// Represents an adapter class that filters and converts the result of
    /// an existing <c>SpatialQuery</c>.
    /// </summary>
    public class SpatialQuery<TInput, TOutput> : ISpatialQuery<TOutput>
    {
        /// <summary>
        /// Gets or sets the inner query.
        /// </summary>
        public IList<ISpatialQuery<TInput>> InnerQueries { get; set; }

        /// <summary>
        /// Gets or sets a predicate that filters the result of the inner query.
        /// Objects passed the predicated will be included in the query.
        /// </summary>
        public Predicate<TInput> Filter { get; set; }

        /// <summary>
        /// Gets or sets a predicate that converts the result of the inner query.
        /// </summary>
        public Converter<TInput, TOutput> Converter { get; set; }

        private CollectionAdapter adapter;

        public SpatialQuery(params ISpatialQuery<TInput>[] queries) 
        {
            if (queries == null)
                throw new ArgumentNullException("query");
            
            this.Filter = d => d is TOutput;
            this.Converter = d => (TOutput)(object)d;
            this.InnerQueries = new List<ISpatialQuery<TInput>>(queries);
            this.adapter = new CollectionAdapter() { Parent = this };
        }

        private bool Convert(TInput input, out TOutput output)
        {
            if (Filter != null && !Filter(input))
            {
                output = default(TOutput);
                return false;
            }

            if (Converter != null)
            {
                output = Converter(input);
                return true;
            }

            if (input is TOutput)
            {
                output = (TOutput)(object)input;
                return true;
            }

            output = default(TOutput);
            return false;
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<TOutput> result)
        {
            adapter.Result = result;
            if (InnerQueries != null)
                for (int i = 0; i < InnerQueries.Count; i++)
                    InnerQueries[i].FindAll(ref boundingSphere, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref Ray ray, ICollection<TOutput> result)
        {
            adapter.Result = result;
            if (InnerQueries != null)
                for (int i = 0; i < InnerQueries.Count; i++)
                    InnerQueries[i].FindAll(ref ray, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<TOutput> result)
        {
            adapter.Result = result;
            if (InnerQueries != null)
                for (int i = 0; i < InnerQueries.Count; i++)
                    InnerQueries[i].FindAll(ref boundingBox, adapter);
            adapter.Result = null;
        }

        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<TOutput> result)
        {
            adapter.Result = result;
            if (InnerQueries != null)
                for (int i = 0; i < InnerQueries.Count; i++)
                    InnerQueries[i].FindAll(ref boundingFrustum, adapter);
            adapter.Result = null;
        }

        class CollectionAdapter : SpatialQueryCollectionAdapter<TInput>
        {
            public SpatialQuery<TInput, TOutput> Parent;
            public ICollection<TOutput> Result;

            public override void Add(TInput item)
            {
                TOutput output;
                if (Parent.Convert(item, out output))
                    Result.Add(output);
            }
        }
    }

    /// <summary>
    /// Represents a basic query from fixed list.
    /// </summary>
    public class SpatialQuery<T> : ISpatialQuery<T>
    {
        public IList<T> Objects { get; private set; }

        public SpatialQuery() { }

        public SpatialQuery(IList<T> objects)
        {
            if (objects == null)
                throw new ArgumentNullException("objects");
            this.Objects = objects;
        }

        private void Find(ICollection<T> result)
        {
            var count = Objects.Count;
            for (int i = 0; i < count; i++)
                result.Add(Objects[i]);
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<T> result)
        {
            Find(result);
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<T> result)
        {
            Find(result);
        }

        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<T> result)
        {
            Find(result);
        }

        public void FindAll(ref Ray ray, ICollection<T> result)
        {
            Find(result);
        }
    }

    abstract class SpatialQueryCollectionAdapter<T> : ICollection<T>
    {
        public abstract void Add(T item);
        public void Clear() { throw new InvalidOperationException(); }
        public bool Contains(T item) { throw new InvalidOperationException(); }
        public void CopyTo(T[] array, int arrayIndex) { throw new InvalidOperationException(); }
        public int Count { get { throw new InvalidOperationException(); } }
        public bool IsReadOnly { get { throw new InvalidOperationException(); } }
        public bool Remove(T item) { throw new InvalidOperationException(); }
        public IEnumerator<T> GetEnumerator() { throw new InvalidOperationException(); }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() { throw new InvalidOperationException(); }
    }
}