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
    
    #region GridPartition
    public abstract class GridPartition
    {
        public int TessellationX { get; protected set; }
        public int TessellationY { get; protected set; }
        public Vector2 Position { get; protected set; }
        public Vector2 Dimension { get; protected set; }


        public GridPartition(float width, float height, float x, float y, int tessellationX, int tessellationY)
        {
            Position = new Vector2(x, y);
            Dimension = new Vector2(width, height);
            TessellationX = tessellationX;
            TessellationY = tessellationY;
        }

        public bool Contains(float x, float y)
        {
            return x >= -Dimension.X / 2 && y >= -Dimension.Y / 2 &&
                   x < Dimension.X / 2 && y < Dimension.Y / 2;
        }

        public Point PositionToGrid(float x, float y, bool clamp)
        {
            Point pt;

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

        public Vector2 GridToPosition(int x, int y)
        {
            Vector2 v;

            v.X = -Dimension.X / 2 + (x + 0.5f) * Dimension.X / TessellationX;
            v.Y = -Dimension.Y / 2 + (y + 0.5f) * Dimension.Y / TessellationY;

            return v;
        }
    }
    #endregion

    #region GridObjectManager
    public class GridObjectManager : GridPartition, ISpacialObjectManager, ICollection<object>
    {
        internal struct Entry
        {
            public object Object;
            public Vector2 Position;
        }

        private Dictionary<int, List<Entry>> dictionary = new Dictionary<int, List<Entry>>();


        public GridObjectManager(float width, float height, float x, float y, int tessellationX, int tessellationY)
            : base(width, height, x, y, tessellationX, tessellationY)
        {

        }
        
        public void Add(object obj, float x, float y)
        {
            if (obj == null)
                throw new ArgumentException();

            Point pt = PositionToGrid(x, y, true);

            int key = pt.Y * TessellationX + pt.X;

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

        public void Clear()
        {
            dictionary.Clear();

            Count = 0;
        }


        public IEnumerable<object> GetNearbyObjects(Vector3 position, float radius)
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