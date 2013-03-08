namespace Nine
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// Manages a collection of objects using grids.
    /// </summary>
    [Obsolete("Use QuadTreeSceneManager instead")]
    public class GridSceneManager : UniformGrid, ISceneManager<ISpatialQueryable>
    {
        private GridSceneManagerEntry<ISpatialQueryable>[] Data;
        private float rayPickPrecision = float.MaxValue;
        private ISpatialQueryable obj;
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

        private void InitDelegates()
        {
            add = new Predicate<Point>(Add);
            findBoundingBox = new Predicate<Point>(FindBoundingBox);
            findRay = new Predicate<Point>(FindRay);
        }

        public void Add(ISpatialQueryable obj, Vector3 position, float radius)
        {
            Add(obj, new BoundingSphere(position, radius));
        }

        public void Add(ISpatialQueryable obj, BoundingSphere bounds)
        {
            Add(obj, BoundingBox.CreateFromSphere(bounds));
        }

        public void Add(ISpatialQueryable obj, BoundingBox box)
        {
            if (obj == null)
                throw new ArgumentNullException("obj");

            this.obj = obj;
            this.box = box;
            Traverse(box, add);
            this.obj = default(ISpatialQueryable);
            Count++;
        }

        private bool Add(Point grid)
        {
            var index = grid.X + SegmentCountX * grid.Y;

            if (Data == null)
                Data = new GridSceneManagerEntry<ISpatialQueryable>[SegmentCountX * SegmentCountY];
            if (Data[index] == null)
                Data[index] = new GridSceneManagerEntry<ISpatialQueryable>();

            var entry = Data[index];
            if (entry.Objects == null)
            {
                entry.Objects = new List<ISpatialQueryable>();
                entry.ObjectBounds = new List<BoundingBox>();

                BoundingRectangle bounds = GetSegmentBounds(grid.X, grid.Y);

                entry.Bounds.Min.X = bounds.X;
                entry.Bounds.Min.Z = bounds.Y;
                entry.Bounds.Max.X = bounds.X + bounds.Width;
                entry.Bounds.Max.Z = bounds.Y + bounds.Height;
            }

            entry.Objects.Add(obj);
            entry.ObjectBounds.Add(box);

            if (box.Min.Y < entry.Bounds.Min.Y)
                entry.Bounds.Min.Y = box.Min.Y;
            if (box.Max.Y > entry.Bounds.Max.Y)
                entry.Bounds.Max.Y = box.Max.Y;

            BoundingRectangle rectangle = new BoundingRectangle();
            rectangle.X = box.Min.X;
            rectangle.Y = box.Min.Z;
            rectangle.Width = box.Max.X - box.Min.X;
            rectangle.Height = box.Max.Z - box.Min.Z;

            float precision = (float)Math.Sqrt(rectangle.Width * rectangle.Width + rectangle.Height * rectangle.Height) * 0.25f;
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

            for (int x = 0; x < SegmentCountX; ++x)
            {
                for (int y = 0; y < SegmentCountY; ++y)
                {
                    var index = x + SegmentCountX * y;

                    if (Data != null && Data[index] != null && Data[index].Objects != null)
                    {
                        var entry = Data[index];
                        entry.Objects.Clear();
                        entry.ObjectBounds.Clear();
                        entry.Bounds.Max.Y = entry.Bounds.Min.Y = 0;
                    }
                }
            }
        }

        #region ISpatialQuery Members
        public void FindAll(ref Ray ray, ICollection<ISpatialQueryable> result)
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
                for (int i = 0; i < entry.Objects.Count; ++i)
                {
                    if (entry.ObjectBounds[i].Intersects(ray).HasValue)
                    {
                        HashSet.Add(entry.Objects[i]);
                    }
                }
            }
            return true;
        }

        public void FindAll(ref BoundingBox boundingBox, ICollection<ISpatialQueryable> result)
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
                for (int i = 0; i < entry.Objects.Count; ++i)
                {
                    if (entry.ObjectBounds[i].Contains(box) != ContainmentType.Disjoint)
                    {
                        HashSet.Add(entry.Objects[i]);
                    }
                }
            }
            return true;
        }

        public void FindAll(ref BoundingSphere boundingSphere, ICollection<ISpatialQueryable> result)
        {
            BoundingBox box = new BoundingBox();
            BoundingBox.CreateFromSphere(ref boundingSphere, out box);
            FindAll(ref box, result);
        }

        public void FindAll(BoundingFrustum boundingFrustum, ICollection<ISpatialQueryable> result)
        {
            for (int x = 0; x < SegmentCountX; ++x)
            {
                for (int y = 0; y < SegmentCountY; ++y)
                {
                    var index = x + SegmentCountX * y;
                    if (Data != null && Data[index] != null && Data[index].Objects != null)
                    {
                        var entry = Data[index];
                        for (int i = 0; i < entry.Objects.Count; ++i)
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

        static HashSet<ISpatialQueryable> HashSet = new HashSet<ISpatialQueryable>();
        static void FlushFindResult(ICollection<ISpatialQueryable> result)
        {
            foreach (var value in HashSet)
                result.Add(value);
            HashSet.Clear();
        }
        #endregion

        #region ICollection<ISpatialQueryable> Members

        /// <summary>
        /// This method will always throw an InvalidOperationException().
        /// Use the other overload instead.
        /// </summary>
        void ICollection<ISpatialQueryable>.Add(ISpatialQueryable item)
        {
            throw new InvalidOperationException();
        }

        public bool Contains(ISpatialQueryable item)
        {
            foreach (ISpatialQueryable o in this)
            {
                if (o.Equals(item))
                    return true;
            }

            return false;
        }

        public void CopyTo(ISpatialQueryable[] array, int arrayIndex)
        {
            foreach (ISpatialQueryable o in this)
            {
                array[arrayIndex++] = o;
            }
        }

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ISpatialQueryable item)
        {
            if (item == null)
                return false;

            bool hasValue = false;

            for (int x = 0; x < SegmentCountX; ++x)
            {
                for (int y = 0; y < SegmentCountY; ++y)
                {
                    var index = x + SegmentCountX * y;
                    if (Data != null && Data[index] != null && Data[index].Objects != null)
                    {
                        var entry = Data[index];
                        for (int i = 0; i < entry.Objects.Count; ++i)
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

        #region IEnumerable<ISpatialQueryable> Members
        public IEnumerator<ISpatialQueryable> GetEnumerator()
        {
            HashSet.Clear();
            if (Data != null)
                foreach (GridSceneManagerEntry<ISpatialQueryable> entry in Data)
                    if (entry.Objects != null)
                        foreach (ISpatialQueryable e in entry.Objects)
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

    class GridSceneManagerEntry<ISpatialQueryable>
    {
        public BoundingBox Bounds;
        public List<ISpatialQueryable> Objects;
        public List<BoundingBox> ObjectBounds;
    }
}