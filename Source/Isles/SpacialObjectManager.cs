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
    public interface ISpacialObjectManager
    {
        IEnumerable<object> GetObjectsFromArea(Vector3 position, float radius);
    }


    public class GridObjectManager : ISpacialObjectManager
    {
        internal struct Entry
        {
            public object Object;
            public Vector3 Position;
        }

        private Vector3 position;
        private Vector3 dimension = new Vector3(128, 128, 0);
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

        public Vector3 Position
        {
            get { return position; }
            set
            {
                if (dictionary.Count > 0)
                    throw new InvalidOperationException("Cannot adjust position when dictionary is not empty");

                position = value;
            }
        }

        public Vector3 Dimension
        {
            get { return dimension; }
            set
            {
                if (dictionary.Count > 0)
                    throw new InvalidOperationException("Cannot adjust dimension when dictionary is not empty");

                dimension = value;
            }
        } 


        public void Add(object obj, Vector3 position)
        {
            if (obj == null)
                throw new ArgumentException();

            Point pt = PointFromPosition(position.X, position.Y);

            int key = pt.Y * tessellationX + pt.X;

            List<Entry> list;

            if (dictionary.TryGetValue(key, out list))
            {
                Entry e;

                e.Object = obj;
                e.Position = position;

                list.Add(e);
            }
            else
            {
                list = new List<Entry>();

                Entry e;

                e.Object = obj;
                e.Position = position;

                list.Add(e);

                dictionary.Add(key, list);
            }
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
        }


        public IEnumerable<object> GetObjectsFromArea(Vector3 position, float radius)
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
    }
}