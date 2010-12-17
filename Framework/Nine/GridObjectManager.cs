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
    /// Basic 2D Space partition using uniform grids.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class UniformGrid
    {
        /// <summary>
        /// Gets or sets the number of columns (x) of the grid.
        /// </summary>
        public int TessellationX { get; protected set; }

        /// <summary>
        /// Gets or sets the number of rows (y) of the grid.
        /// </summary>
        public int TessellationY { get; protected set; }

        /// <summary>
        /// Gets or sets the center position of the grid.
        /// </summary>
        public Vector2 Position { get; protected set; }

        /// <summary>
        /// Gets the width and height of the grid.
        /// </summary>
        public Vector2 Dimension { get; protected set; }

        /// <summary>
        /// Creates a new grid.
        /// </summary>
        public UniformGrid(float width, float height, float x, float y, int tessellationX, int tessellationY)
        {
            Position = new Vector2(x, y);
            Dimension = new Vector2(width, height);
            TessellationX = tessellationX;
            TessellationY = tessellationY;
        }

        /// <summary>
        /// Gets whether the grid contains the specified point.
        /// </summary>
        public bool Contains(float x, float y)
        {
            return x >= -Dimension.X / 2 && y >= -Dimension.Y / 2 &&
                   x < Dimension.X / 2 && y < Dimension.Y / 2;
        }

        /// <summary>
        /// Converts from world space to integral grid space.
        /// </summary>
        /// <param name="clamp">Whether points outside the grid are clamped to borders.</param>
        public Point PositionToGrid(float x, float y, bool clamp)
        {
            Point pt = new Point();

            pt.X = (int)((x - Position.X + Dimension.X / 2) * TessellationX / Dimension.X);
            pt.Y = (int)((y - Position.Y + Dimension.Y / 2) * TessellationY / Dimension.Y);

            if (clamp)
            {
                if (pt.X < 0)
                    pt.X = 0;
                if (pt.X >= TessellationX)
                    pt.X = TessellationX - 1;
                if (pt.Y < 0)
                    pt.Y = 0;
                if (pt.Y >= TessellationY)
                    pt.Y = TessellationY - 1;
            }

            return pt;
        }

        /// <summary>
        /// Gets the center position of the specified integral grid. Input boundary is not checked.
        /// </summary>
        public Vector2 GridToPosition(int x, int y)
        {
            Vector2 v = new Vector2();

            v.X = -Dimension.X / 2 + (x + 0.5f) * Dimension.X / TessellationX;
            v.Y = -Dimension.Y / 2 + (y + 0.5f) * Dimension.Y / TessellationY;

            return v;
        }
    }


    /// <summary>
    /// Manages a collection of objects using grids.
    /// </summary>
    public class GridObjectManager : UniformGrid, ICollection<object>, ISpatialQuery
    {
        internal struct Entry
        {
            public object Object;
            public Vector2 Position;
        }

        private Dictionary<int, List<Entry>> dictionary = new Dictionary<int, List<Entry>>();

        /// <summary>
        /// Creates a new instance of GridObjectManager.
        /// </summary>
        public GridObjectManager(float width, float height, float x, float y, int tessellationX, int tessellationY)
            : base(width, height, x, y, tessellationX, tessellationY)
        {

        }

        /// <summary>
        /// Creates a new instance of GridObjectManager.
        /// </summary>
        public GridObjectManager(BoundingRectangle bounds, int tessellationX, int tessellationY)
            : base(bounds.Max.X - bounds.Min.X, bounds.Max.Y - bounds.Min.Y, bounds.Min.X, bounds.Min.Y, tessellationX, tessellationY)
        {

        }
        
        /// <summary>
        /// Adds an object with the specified position.
        /// </summary>
        public void Add(object obj, float x, float y)
        {
            if (obj == null)
                throw new ArgumentException();

            Point pt = PositionToGrid(x, y, true);

            int key = pt.Y * TessellationX + pt.X;

            List<Entry> list;

            if (dictionary.TryGetValue(key, out list))
            {
                Entry e = new Entry();

                e.Object = obj;
                e.Position.X = x;
                e.Position.Y = y;

                list.Add(e);
            }
            else
            {
                list = new List<Entry>();

                Entry e = new Entry();

                e.Object = obj;
                e.Position.X = x;
                e.Position.Y = y;

                list.Add(e);

                dictionary.Add(key, list);
            }

            Count++;
        }

        public void Add(object obj, BoundingSphere bounds)
        {
            Add(obj, bounds.Center.X, bounds.Center.Y, bounds.Radius);
        }

        public void Add(object obj, float x, float y, float radius)
        {
            throw new NotImplementedException();
        }
        
        /// <summary>
        /// Clear all objects managed by this instance.
        /// </summary>
        public void Clear()
        {
            dictionary.Clear();

            Count = 0;
        }

        #region ISpatialQuery Members
        public T FindFirst<T>(Vector3 position)
        {
            throw new NotImplementedException();
        }

        public T FindFirst<T>(Ray ray)
        {
            throw new NotImplementedException();
        }

        public T FindFirst<T>(Vector3 position, float radius)
        {
            float min = float.MaxValue;
            T nearest = default(T);

            List<Entry> list;
            float rr = radius * radius;

            Point ul = PositionToGrid(position.X - radius, position.Y - radius, true);
            Point br = PositionToGrid(position.X + radius, position.Y + radius, true);

            for (int y = ul.Y; y <= br.Y; y++)
            {
                for (int x = ul.X; x <= br.X; x++)
                {
                    if (dictionary.TryGetValue(y * TessellationX + x, out list))
                    {
                        foreach (Entry e in list)
                        {
                            if (!(e.Object is T))
                                continue;

                            float xx = e.Position.X - position.X;
                            float yy = e.Position.Y - position.Y;
                            float dd = xx * xx + yy * yy;

                            if (dd <= rr && dd < min)
                            {
                                min = dd;
                                nearest = (T)e.Object;
                            }
                        }
                    }
                }
            }

            return nearest;
        }

        public IEnumerable<T> Find<T>(Vector3 position, float radius)
        {
            List<Entry> list;
            float rr = radius * radius;

            Point ul = PositionToGrid(position.X - radius, position.Y - radius, true);
            Point br = PositionToGrid(position.X + radius, position.Y + radius, true);

            for (int y = ul.Y; y <= br.Y; y++)
            {
                for (int x = ul.X; x <= br.X; x++)
                {
                    if (dictionary.TryGetValue(y * TessellationX + x, out list))
                    {
                        foreach (Entry e in list)
                        {
                            if (!(e.Object is T))
                                continue;

                            float xx = e.Position.X - position.X;
                            float yy = e.Position.Y - position.Y;

                            if (xx * xx + yy * yy <= rr)
                            {
                                yield return (T)e.Object;
                            }
                        }
                    }
                }
            }
        }

        public IEnumerable<T> Find<T>(Ray ray)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<T> Find<T>(BoundingFrustum frustum)
        {
            throw new NotImplementedException();
        }
        #endregion

        #region ICollection<object> Members

        /// <summary>
        /// This method will always throw an InvalidOperationException().
        /// Use the other overload instead.
        /// </summary>
        void ICollection<object>.Add(object item)
        {
            throw new InvalidOperationException();
        }

        public bool Contains(object item)
        {
            foreach (object o in this)
            {
                if (o == item)
                    return true;
            }

            return false;
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            foreach (object o in this)
            {
                array[arrayIndex++] = o;
            }
        }

        public int Count { get; private set; }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public bool Remove(object item)
        {
            if (item == null)
                return false;

            List<Entry> list = null;
            int index = -1;

            foreach (List<Entry> entry in dictionary.Values)
            {
                if (entry != null)
                {
                    for (int i = 0; i < entry.Count; i++)
                    {
                        if (entry[i].Object == item)
                        {
                            index = i;
                            list = entry;
                            break;
                        }
                    }
                }

                if (index >= 0)
                    break;
            }

            if (index >= 0)
            {
                list.RemoveAt(index);
                return true;
            }

            return false;
        }

        #endregion

        #region IEnumerable<object> Members

        public IEnumerator<object> GetEnumerator()
        {
            foreach (List<Entry> entry in dictionary.Values)
            {
                if (entry != null)
                {
                    foreach (Entry e in entry)
                    {
                        yield return e.Object;
                    }
                }
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion
    }
}