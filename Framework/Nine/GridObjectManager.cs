#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
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
    /// Manages a collection of objects using grids.
    /// </summary>
    public class GridObjectManager<T> : UniformGrid, ICollection<T>, ISpatialQuery<T>
    {
        private float rayPickPrecision = float.MaxValue;

        GridObjectManagerEntry<T>[,] Data;

        /// <summary>
        /// Creates a new instance of GridObjectManager.
        /// </summary>
        public GridObjectManager(float x, float y, float width, float height, int countX, int countY)
            : base(x, y, width, height, countX, countY)
        {
            Position = new Vector2(x, y);
        }

        /// <summary>
        /// Creates a new instance of GridObjectManager.
        /// </summary>
        public GridObjectManager(BoundingRectangle bounds, int countX, int countY)
            : base(bounds, countX, countY)
        {

        }

        /// <summary>
        /// Creates a new instance of GridObjectManager.
        /// </summary>
        public GridObjectManager(BoundingBox bounds, int countX, int countY)
            : base(new BoundingRectangle(bounds), countX, countY)
        {

        }

        public void Add(T obj, Vector3 position, float radius)
        {
            Add(obj, new BoundingSphere(position, radius));
        }

        public void Add(T obj, BoundingSphere bounds)
        {
            Add(obj, BoundingBox.CreateFromSphere(bounds));
        }

        public void Add(T obj, BoundingBox box)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            foreach (Point grid in Traverse(box))
            {
                if (Data == null)
                    Data = new GridObjectManagerEntry<T>[SegmentCountX, SegmentCountY];

                if (Data[grid.X, grid.Y].Objects == null)
                {
                    Data[grid.X, grid.Y].Objects = new List<T>();
                    Data[grid.X, grid.Y].ObjectBounds = new List<BoundingBox>();

                    BoundingRectangle bounds = GetSegmentBounds(grid.X, grid.Y);

                    Data[grid.X, grid.Y].Bounds.Min.X = bounds.Min.X;
                    Data[grid.X, grid.Y].Bounds.Min.Y = bounds.Min.Y;
                    Data[grid.X, grid.Y].Bounds.Max.X = bounds.Max.X;
                    Data[grid.X, grid.Y].Bounds.Max.Y = bounds.Max.Y;
                }

                Data[grid.X, grid.Y].Objects.Add(obj);
                Data[grid.X, grid.Y].ObjectBounds.Add(box);

                if (box.Min.Z < Data[grid.X, grid.Y].Bounds.Min.Z)
                    Data[grid.X, grid.Y].Bounds.Min.Z = box.Min.Z;
                if (box.Max.Z > Data[grid.X, grid.Y].Bounds.Max.Z)
                    Data[grid.X, grid.Y].Bounds.Max.Z = box.Max.Z;

                BoundingRectangle rectangle = new BoundingRectangle(box);
                float precision = Vector2.Subtract(rectangle.Max, rectangle.Min).Length() * 0.25f;
                if (precision < rayPickPrecision)
                    rayPickPrecision = precision;
            }

            Count++;
        }

        /// <summary>
        /// Clear all objects managed by this instance.
        /// </summary>
        public void Clear()
        {
            Count = 0;

            for (int x = 0; x < SegmentCountX; x++)
            {
                for (int y = 0; y < SegmentCountY; y++)
                {
                    if (Data != null && Data[x, y].Objects != null)
                    {
                        Data[x, y].Objects.Clear();
                        Data[x, y].ObjectBounds.Clear();
                        Data[x, y].Bounds.Max.Z = Data[x, y].Bounds.Min.Z = 0;
                    }
                }
            }
        }

        #region ISpatialQuery Members
        public IEnumerable<T> FindAll(Vector3 position, float radius)
        {
            return FindAll(new BoundingSphere(position, radius));
        }

        public IEnumerable<T> FindAll(BoundingBox box)
        {
            return Unique(InternalFind(box));
        }

        private IEnumerable<T> InternalFind(BoundingBox box)
        {
            foreach (Point grid in Traverse(box))
            {
                if (Data != null && Data[grid.X, grid.Y].Objects != null)
                {
                    for (int i = 0; i < Data[grid.X, grid.Y].Objects.Count; i++)
                    {
                        if (Data[grid.X, grid.Y].ObjectBounds[i].Contains(box) != ContainmentType.Disjoint)
                        {
                            yield return Data[grid.X, grid.Y].Objects[i];
                        }
                    }
                }
            }
        }
        public IEnumerable<T> FindAll(BoundingSphere sphere)
        {
            return Unique(InternalFind(sphere));
        }

        private IEnumerable<T> InternalFind(BoundingSphere sphere)
        {
            foreach (Point grid in Traverse(BoundingBox.CreateFromSphere(sphere)))
            {
                if (Data != null && Data[grid.X, grid.Y].Objects != null)
                {
                    for (int i = 0; i < Data[grid.X, grid.Y].Objects.Count; i++)
                    {
                        if (Data[grid.X, grid.Y].ObjectBounds[i].Contains(sphere) != ContainmentType.Disjoint)
                            yield return Data[grid.X, grid.Y].Objects[i];
                    }
                }
            }
        }

        public IEnumerable<T> FindAll(Ray ray)
        {
            foreach (Point grid in Traverse(ray, rayPickPrecision))
            {
                if (Data != null && Data[grid.X, grid.Y].Objects != null)
                {
                    for (int i = 0; i < Data[grid.X, grid.Y].Objects.Count; i++)
                    {
                        if (Data[grid.X, grid.Y].Objects[i] is IPickable)
                        {
                            if (((IPickable)(Data[grid.X, grid.Y].Objects[i])).Intersects(ray).HasValue)
                                yield return Data[grid.X, grid.Y].Objects[i];                                
                        }
                        else if (Data[grid.X, grid.Y].ObjectBounds[i].Intersects(ray).HasValue)
                        {
                            yield return Data[grid.X, grid.Y].Objects[i];
                        }
                    }
                }
            }
        }

        public T Find(Ray ray)
        {
            float? currentDistance;
            float minDistance = float.MaxValue;
            T result = default(T);

            foreach (Point grid in Traverse(ray, rayPickPrecision))
            {
                if (Data != null && Data[grid.X, grid.Y].Objects != null)
                {
                    for (int i = 0; i < Data[grid.X, grid.Y].Objects.Count; i++)
                    {
                        if (Data[grid.X, grid.Y].Objects[i] is IPickable)
                        {
                            currentDistance = ((IPickable)(Data[grid.X, grid.Y].Objects[i])).Intersects(ray);
                            if (currentDistance.HasValue && currentDistance < minDistance)
                            {
                                minDistance = currentDistance.Value;
                                result = Data[grid.X, grid.Y].Objects[i];
                            }
                        }
                        else
                        {
                            currentDistance = Data[grid.X, grid.Y].ObjectBounds[i].Intersects(ray);
                            if (currentDistance.HasValue && currentDistance < minDistance)
                            {
                                minDistance = currentDistance.Value;
                                result = Data[grid.X, grid.Y].Objects[i];
                            }
                        }
                    }
                }
            }

            return result;
        }

        public IEnumerable<T> FindAll(BoundingFrustum frustum)
        {
            return Unique(InternalFind(frustum));
        }

        private IEnumerable<T> InternalFind(BoundingFrustum frustum)
        {
            for (int x = 0; x < SegmentCountX; x++)
            {
                for (int y = 0; y < SegmentCountY; y++)
                {
                    if (Data != null && Data[x, y].Objects != null)
                    {
                        for (int i = 0; i < Data[x, y].Objects.Count; i++)
                        {
                            if (frustum.Contains(Data[x, y].ObjectBounds[i]) != ContainmentType.Disjoint)
                            {
                                yield return Data[x, y].Objects[i];
                            }
                        }
                    }
                }
            }
        }

        static Dictionary<T, object> enumerationSet = new Dictionary<T, object>();
        static IEnumerable<T> Unique(IEnumerable<T> enumeration)
        {
            enumerationSet.Clear();

            foreach (T item in enumeration)
            {
                if (!enumerationSet.ContainsKey(item))
                    enumerationSet.Add(item, null);
            }

            return enumerationSet.Keys;
        }
        #endregion

        #region ICollection<T> Members

        /// <summary>
        /// This method will always throw an InvalidOperationException().
        /// Use the other overload instead.
        /// </summary>
        void ICollection<T>.Add(T item)
        {
            throw new InvalidOperationException();
        }

        public bool Contains(T item)
        {
            foreach (T o in this)
            {
                if (o.Equals(item))
                    return true;
            }

            return false;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            foreach (T o in this)
            {
                array[arrayIndex++] = o;
            }
        }

        public int Count { get; private set; }

        bool ICollection<T>.IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(T item)
        {
            if (item == null)
                return false;

            bool hasValue = false;

            for (int x = 0; x < SegmentCountX; x++)
            {
                for (int y = 0; y < SegmentCountY; y++)
                {
                    if (Data != null && Data[x, y].Objects != null)
                    {
                        for (int i = 0; i < Data[x, y].Objects.Count; i++)
                        {
                            if (Data[x, y].Objects[i].Equals(item))
                            {
                                hasValue = true;
                                Data[x, y].Objects.RemoveAt(i);
                                Data[x, y].ObjectBounds.RemoveAt(i);
                                i--;
                            }
                        }
                    }
                }
            }

            if (hasValue)
                Count--;

            return hasValue;
        }

        #endregion

        #region IEnumerable<T> Members
        public IEnumerator<T> GetEnumerator()
        {
            return Unique(InternalGetEnumerator()).GetEnumerator();
        }

        private IEnumerable<T> InternalGetEnumerator()
        {
            if (Data != null)
                foreach (GridObjectManagerEntry<T> entry in Data)
                    if (entry.Objects != null)
                        foreach (T e in entry.Objects)
                            yield return e;
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    internal struct GridObjectManagerEntry<T>
    {
        public BoundingBox Bounds;
        public List<T> Objects;
        public List<BoundingBox> ObjectBounds;
    }
}