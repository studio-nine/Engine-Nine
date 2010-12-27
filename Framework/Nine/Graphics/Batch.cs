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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics
{
    internal class BatchItem<TKey, TValue>
    {
        public TKey Key;
        public TValue[] Values;
        public int Count;
    }


    /// <summary>
    /// A collection that automatically group values into batches. Values are grouped by TKey.
    /// </summary>
    internal class Batch<TKey, TValue>
    {
        int capacity;
        int batchCount;

        Dictionary<TKey, int> index = new Dictionary<TKey, int>();

        List<BatchItem<TKey, TValue>> batches = new List<BatchItem<TKey, TValue>>();
        

        public Batch(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException();

            this.capacity = capacity;
        }

        public int Count { get { return batchCount; } }

        public IEnumerable<BatchItem<TKey, TValue>> Batches
        {
            get
            {
                for (int i = 0; i < batchCount; i++)
                    yield return batches[i];
            }
        }

        public void Clear()
        {
            index.Clear();

            batchCount = 0;

            for (int i = 0; i < batches.Count; i++)
            {
                // Do we need to clear Values array as well?
                batches[i].Count = 0;
            }
        }

        public void Add(TKey key, TValue value)
        {
            int i;

            if (index.TryGetValue(key, out i))
            {
                TValue[] array = batches[i].Values;

                if (array.Length <= batches[i].Count)
                {
                    Array.Resize(ref array, array.Length * 2);
                }
                
                batches[i].Values = array;
                batches[i].Values[batches[i].Count] = value;
                batches[i].Count++;
                
                return;
            }

            if (batches.Count <= batchCount)
                batches.Add(new BatchItem<TKey, TValue>());

            if (batches[batchCount].Values == null)
                batches[batchCount].Values = new TValue[capacity];

            batches[batchCount].Count = 1;
            batches[batchCount].Key = key;
            batches[batchCount].Values[0] = value;

            index.Add(key, batchCount++);
        }
    }
}
