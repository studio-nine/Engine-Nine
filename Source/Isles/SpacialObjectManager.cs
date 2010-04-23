#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles
{
    #region ISpacialObjectManager
    public interface ISpacialObjectManager
    {
        IEnumerable<object> GetNearbyObjects(Vector3 position, float radius);
    }
    #endregion
    
    #region GridObjectManager
    public class GridObjectManager : ISpacialObjectManager, ICollection<object>
    {
        internal struct Entry
        {
            public object Object;
            public Vector2 Position;
        }

        private Vector2 position;
        private Vector2 dimension = new Vector2(128, 128);
        private int tessellationX = 16;
        private int tessellationY = 16;
        private Dictionary<int, List<Entry>> dictionary = new Dictionary<int, List<Entry>>();

        public int TessellationX
        {
            get { return tessellationX; }
            set
            {
                if (dictionary.Count > 0)
                    throw new InvalidOperationException("Cannot adjust grid count when dictionary is not empty");
                
                tessellationX = value;
            }
        }

        public int TessellationY
        {
            get { return tessellationY; }
            set
            {
                if (dictionary.Count > 0)
                    throw new InvalidOperationException("Cannot adjust grid count when dictionary is not empty");

                tessellationY = value;
            }
        }

        public Vector2 Position
        {
            get { return position; }
            set
            {
                if (dictionary.Count > 0)
                    throw new InvalidOperationException("Cannot adjust position when dictionary is not empty");

                position = value;
            }
        }

        public Vector2 Dimension
        {
            get { return dimension; }
            set
            {
                if (dictionary.Count > 0)
                    throw new InvalidOperationException("Cannot adjust dimension when dictionary is not empty");

                dimension = value;
            }
        }

        public GridObjectManager() { }

        public GridObjectManager(float width, float height, float x, float y, int tessellationX, int tessellationY)
        {
            Position = new Vector2(x, y);

            Dimension = new Vector2(width, height);

            TessellationX = tessellationX;
            TessellationY = tessellationY;
        }


        public void Add(object obj, float x, float y)
        {
            if (obj == null)
                throw new ArgumentException();

            Point pt = PointFromPosition(x, y);

            int key = pt.Y * tessellationX + pt.X;

            List<Entry> list;

            if (dictionary.TryGetValue(key, out list))
            {
                Entry e;

                e.Object = obj;
                e.Position.X = x;
                e.Position.Y = y;

                list.Add(e);
            }
            else
            {
                list = new List<Entry>();

                Entry e;

                e.Object = obj;
                e.Position.X = x;
                e.Position.Y = y;

                list.Add(e);

                dictionary.Add(key, list);
            }

            Count++;
        }


        private Point PointFromPosition(float x, float y)
        {
            Point pt;

            pt.X = (int)((x - position.X + dimension.X / 2) * tessellationX / dimension.X);
            pt.Y = (int)((y - position.Y + dimension.Y / 2) * tessellationY / dimension.Y);

            if (pt.X < 0)
                pt.X = 0;
            if (pt.X >= tessellationX)
                pt.X = tessellationX - 1;
            if (pt.Y < 0)
                pt.Y = 0;
            if (pt.Y >= tessellationY)
                pt.Y = tessellationY - 1;

            return pt;
        }


        public void Clear()
        {
            dictionary.Clear();

            Count = 0;
        }


        public IEnumerable<object> GetNearbyObjects(Vector3 position, float radius)
        {
            List<Entry> list;
            float rr = radius * radius;

            Point ul = PointFromPosition(position.X - radius, position.Y - radius);
            Point br = PointFromPosition(position.X + radius, position.Y + radius);

            for (int y = ul.Y; y <= br.Y; y++)
            {
                for (int x = ul.X; x <= br.X; x++)
                {
                    if (dictionary.TryGetValue(y * tessellationX + x, out list))
                    {
                        foreach (Entry e in list)
                        {
                            float xx = e.Position.X - position.X;
                            float yy = e.Position.Y - position.Y;

                            if (xx * xx + yy * yy <= rr)
                            {
                                yield return e.Object;
                            }
                        }
                    }
                }
            }
        }

        #region ICollection<object> Members

        public void Add(object item)
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
            get { throw new NotImplementedException(); }
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
    #endregion
}