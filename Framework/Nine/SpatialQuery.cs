#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Defines Spatial relations between objects.
    /// </summary>
    public interface ISpatialQuery<T> : IEnumerable<T>
    {
        /// <summary>
        /// Finds the nearest object intersects with the specified ray.
        /// </summary>
        T Find(Ray ray);
        
        /// <summary>
        /// Finds all the objects resides within the specified bounding sphere.
        /// </summary>
        IEnumerable<T> FindAll(Vector3 position, float radius);

        /// <summary>
        /// Finds all the objects that intersects with the specified ray.
        /// </summary>
        IEnumerable<T> FindAll(Ray ray);

        /// <summary>
        /// Finds all the objects that intersects with the specified bounding box.
        /// </summary>
        IEnumerable<T> FindAll(BoundingBox boundingBox);

        /// <summary>
        /// Finds all the objects resides within the specified bounding frustum.
        /// </summary>
        IEnumerable<T> FindAll(BoundingFrustum frustum);
    }

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

        public SpatialQuery(params ISpatialQuery<TInput>[] queries) 
        {
            if (queries == null)
                throw new ArgumentNullException("query");

            this.InnerQueries = new List<ISpatialQuery<TInput>>(queries);
        }

        private TOutput Convert(TInput input)
        {
            if (Filter != null && !Filter(input))
                return default(TOutput);

            if (Converter != null)
                return Converter(input);

            if (input is TOutput)
                return (TOutput)(object)input;

            return default(TOutput);
        }

        private IEnumerable<TOutput> Convert(IEnumerable<TInput> inputs)
        {
            foreach (TInput input in inputs)
            {
                if (Filter != null && !Filter(input))
                    continue;

                if (Converter != null)
                    yield return Converter(input);

                if (input is TOutput)
                    yield return (TOutput)(object)input;
            }
        }
        
        public TOutput Find(Ray ray)
        {
            return Convert(InnerQueries[0].Find(ray));
        }

        public IEnumerable<TOutput> FindAll(Vector3 position, float radius)
        {
            foreach (ISpatialQuery<TInput> query in InnerQueries)
                foreach (TOutput output in Convert(query.FindAll(position, radius)))
                    yield return output;
        }

        public IEnumerable<TOutput> FindAll(Ray ray)
        {
            foreach (ISpatialQuery<TInput> query in InnerQueries)
                foreach (TOutput output in Convert(query.FindAll(ray)))
                    yield return output;
        }

        public IEnumerable<TOutput> FindAll(BoundingBox boundingBox)
        {
            foreach (ISpatialQuery<TInput> query in InnerQueries)
                foreach (TOutput output in Convert(query.FindAll(boundingBox)))
                    yield return output;
        }

        public IEnumerable<TOutput> FindAll(BoundingFrustum frustum)
        {
            foreach (ISpatialQuery<TInput> query in InnerQueries)
                foreach (TOutput output in Convert(query.FindAll(frustum)))
                    yield return output;
        }

        public IEnumerator<TOutput> GetEnumerator()
        {
            foreach (ISpatialQuery<TInput> query in InnerQueries)
                foreach (TOutput output in Convert(query))
                    yield return output;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    /// <summary>
    /// Represents a basic query from fixed list.
    /// </summary>
    public class SpatialQuery<T> : ISpatialQuery<T>
    {
        private IEnumerable<T> innerObjects;
        public IEnumerable<T> Objects
        {
            get { return innerObjects ?? Enumerable.Empty<T>(); }
            set { innerObjects = value; }
        }

        public SpatialQuery() { }

        public SpatialQuery(IEnumerable<T> objects)
        {
            if (objects == null)
                throw new ArgumentNullException("objects");
            this.Objects = objects;
        }
        
        public IEnumerable<T> FindAll(Vector3 position, float radius)
        {
            return Objects;
        }

        public IEnumerable<T> FindAll(BoundingBox box)
        {
            return Objects;
        }

        public IEnumerable<T> FindAll(BoundingSphere sphere)
        {
            return Objects;
        }

        public IEnumerable<T> FindAll(BoundingFrustum frustum)
        {
            return Objects;
        }

        public IEnumerable<T> FindAll(Ray ray)
        {
            return Objects;
        }

        public T Find(Ray ray)
        {
            return Objects.FirstOrDefault();
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Objects.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}