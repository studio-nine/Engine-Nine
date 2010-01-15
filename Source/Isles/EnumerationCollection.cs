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
    public class EnumerationCollection<TValue, TList>
        : IEnumerable<TValue>, ICollection<TValue> where TList : ICollection<TValue>, new()
    {
        private bool isDirty = true;
        private TList elements = new TList();
        private List<TValue> copy = new List<TValue>();


        public TList Elements
        {
            get { return elements; }
        }

        public IEnumerator<TValue> GetEnumerator()
        {
            // Copy a new list whiling iterating it
            if (isDirty)
            {
                copy.Clear();
                foreach (TValue e in elements)
                    copy.Add(e);
                isDirty = false;
            }

            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Add(TValue e)
        {
            isDirty = true;
            elements.Add(e);
        }

        public bool Remove(TValue e)
        {
            isDirty = true;
            return elements.Remove(e);
        }

        public void Clear()
        {
            isDirty = true;
            elements.Clear();
        }

        public bool IsReadOnly
        {
            get { return true; }
        }

        public int Count
        {
            get { return elements.Count; }
        }

        public bool Contains(TValue item)
        {
            return elements.Contains(item);
        }

        public void CopyTo(TValue[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }
    }
}