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
    /// Manages a collection of objects using grids.
    /// </summary>
    public class GridSceneManager<T> : UniformGrid, ISceneManager<T>
    {
        private GridSceneManagerEntry<T>[] Data;
        private float rayPickPrecision = float.MaxValue;
        private T obj;
        private Ray ray;
        private BoundingBox box;
        private Predicate<Point> add;
        private Predicate<Point> findRay;
        private Predicate<Point> findBoundingBox;

        /// <summary>
        /// Creates a new instance of GridSceneManager.
        /// </summary>
        public GridSceneManager(float x, float y, float width, float height, int countX, int countY)
            : base(x, y, width, height, countX, countY)
        {
            Position = new Vector2(x, y); 
            InitDelegates();
        }

        /// <summary>
        /// Creates a new instance of GridSceneManager.
        /// </summary>
        public GridSceneManager(BoundingRectangle bounds, int countX, int countY)
            : base(bounds, countX, countY)
        {
            InitDelegates();
        }

        /// <summary>
        /// Creates a new instance of GridSceneManager.
        /// </summary>
        public GridSceneManager(BoundingBox bounds, int countX, int countY)
            : base(new BoundingRectangle(bounds), countX, countY)
        {
            InitDelegates();
        }

        private void InitDelegates()
        {
            add = new Predicate<Point>(Add);
            findBoundingBox = new Predicate<Point>(FindBoundingBox);
            findRay = new Predicate<Point>(FindRay);
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

            this.obj = obj;
            this.box = box;
            Traverse(box, add);
            this.obj = default(T);
            Count++;
        }

        private bool Add(Point grid)
        {
            var index = grid.X + SegmentCountX * grid.Y;

            if (Data == null)
                Data = new GridSceneManagerEntry<T>[SegmentCountX * SegmentCountY];
            if (Data[index] == null)
                Data[index] = new GridSceneManagerEntry<T>();

            var entry = Data[index];
            if (entry.Objects == null)
            {
                entry.Objects = new List<T>();
                entry.ObjectBounds = new List<BoundingBox>();

                BoundingRectangle bounds = GetSegmentBounds(grid.X, grid.Y);

                entry.Bounds.Min.X = bounds.Min.X;
                entry.Bounds.Min.Y = bounds.Min.Y;
                entry.Bounds.Max.X = bounds.Max.X;
                entry.Bounds.Max.Y = bounds.Max.Y;
            }

            entry.Objects.Add(obj);
            entry.ObjectBounds.Add(box);

            if (box.Min.Z < entry.Bounds.Min.Z)
                entry.Bounds.Min.Z = box.Min.Z;
            if (box.Max.Z > entry.Bounds.Max.Z)
                entry.Bounds.Max.Z = box.Max.Z;

            BoundingRectangle rectangle = new BoundingRectangle(box);
            float precision = Vector2.Subtract(rectangle.Max, rectangle.Min).Length() * 0.25f;
            if (precision < rayPickPrecision)
                rayPickPrecision = precision;
            return true;
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
                    var index = x + SegmentCountX * y;

                    if (Data != null && Data[index] != null && Data[index].Objects != null)
                    {
                        var entry = Data[index];
                        entry.Objects.Clear();
                        entry.ObjectBounds.Clear();
                        entry.Bounds.Max.Z = entry.Bounds.Min.Z = 0;
                    }
                }
            }
        }

        #region ISpatialQuery Members
        public void FindAll(ref Ray ray, ICollection<T> result)
        {
            this.ray = ray;
            Traverse(ray, rayPickPrecision, findRay);
            FlushFindResult(result);
        }

        private bool FindRay(Point grid)
        {
            var index = grid.X + SegmentCountX * grid.Y;
            if (Data != null && Data[index] != null && Data[index].Objects != null)
            {
                var entry = Data[index];
                for (int i = 0; i < entry.Objects.Count; i++)
                {
                    if (entry.ObjectBounds[i].Intersects(ray).HasValue)
                    {
                        HashSet.Add(entry.Objects[i]);
                    }
                }
            }
            return true;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<T> result)
        {
            this.box = boundingBox;
            Traverse(boundingBox, findBoundingBox);
            FlushFindResult(result);
        }

        private bool FindBoundingBox(Point grid)
        {
            var index = grid.X + SegmentCountX * grid.Y;
            if (Data != null && Data[index] != null && Data[index].Objects != null)
            {
                var entry = Data[index];
                for (int i = 0; i < entry.Objects.Count; i++)
                {
                    if (entry.ObjectBounds[i].Contains(box) != ContainmentType.Disjoint)
                    {
                        HashSet.Add(entry.Objects[i]);
                    }
                }
            }
            return true;
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<T> result)
        {
            BoundingBox box = new BoundingBox();
            BoundingBox.CreateFromSphere(ref boundingSphere, out box);
            FindAll(ref box, result);
        }

        public void FindAll(ref BoundingFrustum boundingFrustum, ICollection<T> result)
        {
            for (int x = 0; x < SegmentCountX; x++)
            {
                for (int y = 0; y < SegmentCountY; y++)
                {
                    var index = x + SegmentCountX * y;
                    if (Data != null && Data[index] != null && Data[index].Objects != null)
                    {
                        var entry = Data[index];
                        for (int i = 0; i < entry.Objects.Count; i++)
                        {
                            if (boundingFrustum.Contains(entry.ObjectBounds[i]) != ContainmentType.Disjoint)
                            {
                                HashSet.Add(entry.Objects[i]);
                            }
                        }
                    }
                }
            }
            FlushFindResult(result);
        }

        static HashSet<T> HashSet = new HashSet<T>();
        static void FlushFindResult(ICollection<T> result)
        {
            foreach (var value in HashSet)
                result.Add(value);
            HashSet.Clear();
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
                    var index = x + SegmentCountX * y;
                    if (Data != null && Data[index] != null && Data[index].Objects != null)
                    {
                        var entry = Data[index];
                        for (int i = 0; i < entry.Objects.Count; i++)
                        {
                            if (entry.Objects[i].Equals(item))
                            {
                                hasValue = true;
                                entry.Objects.RemoveAt(i);
                                entry.ObjectBounds.RemoveAt(i);
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
            HashSet.Clear();
            if (Data != null)
                foreach (GridSceneManagerEntry<T> entry in Data)
                    if (entry.Objects != null)
                        foreach (T e in entry.Objects)
                            HashSet.Add(e);
            return HashSet.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }

    class GridSceneManagerEntry<T>
    {
        public BoundingBox Bounds;
        public List<T> Objects;
        public List<BoundingBox> ObjectBounds;
    }
}