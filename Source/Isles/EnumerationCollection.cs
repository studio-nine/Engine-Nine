#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles
{
    /// <summary>
    /// A collection that can be manipulated during enumeration.
    /// </summary>
    internal class EnumerationCollection<T> : IList<T>
    {
        private bool isDirty = true;
        private List<T> elements = new List<T>();
        private List<T> copy = new List<T>();


        public event EventHandler Added;
        public event EventHandler Removed;
        

        public IEnumerator<T> GetEnumerator()
        {
            // Copy a new list whiling iterating it
            if (isDirty)
            {
                copy.Clear();
                foreach (T e in elements)
                    copy.Add(e);
                isDirty = false;
            }

            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(T e)
        {
            isDirty = true;
            elements.Add(e);

            if (Added != null)
                Added(this, EventArgs.Empty);
        }

        public bool Remove(T e)
        {
            isDirty = true;
            bool result = elements.Remove(e);

            if (Removed != null)
                Removed(this, EventArgs.Empty);

            return result;
        }

        public void Clear()
        {
            isDirty = true;
            elements.Clear();
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public int Count
        {
            get { return elements.Count; }
        }

        public bool Contains(T item)
        {
            return elements.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }

        public int IndexOf(T item)
        {
            return elements.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            isDirty = true;
            elements.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            isDirty = true;
            elements.RemoveAt(index);
        }

        public int RemoveAll(Predicate<T> match)
        {
            isDirty = true;
            return elements.RemoveAll(match);
        }

        public T this[int index]
        {
            get { return elements[index]; }
            set { elements[index] = value; isDirty = true; }
        }
    }
}