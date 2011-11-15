#region Copyright 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Nine
{
    /// <summary>
    /// Just a simple wrap around dictionary for Windows Phone & Xbox
    /// </summary>
    public class HashSet<T>
    {
        Dictionary<T, int> dictionary = new Dictionary<T, int>();

        public int Count
        {
            get { return dictionary.Count; }
        }

        public void Add(T item)
        {
            if (dictionary.ContainsKey(item))
                return;
            dictionary.Add(item, 0);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public Dictionary<T, int>.KeyCollection.Enumerator GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }
    }
}