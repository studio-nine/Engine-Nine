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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics
{
    internal struct BatchItem<TKey>
    {
        public TKey Key;
        public int StartIndex;
        public int Count;
    }


    /// <summary>
    /// A collection that automatically group values into batches. Values are grouped by TKey.
    /// </summary>
    internal sealed class Batch<TKey, TValue>
    {
        private int previousBatchCount = 8;

        public int Count { get; private set; }
        public TValue[] Values { get; private set; }
        public List<BatchItem<TKey>> Batches { get; private set; }


        public Batch(int capacity)
        {
            Values = new TValue[capacity];
            Batches = new List<BatchItem<TKey>>();
        }


        public void Clear()
        {
            previousBatchCount = Math.Max(Batches.Count, 1);
            Batches.Clear();
            Count = 0;
        }


        public void Add(TKey key, TValue value)
        {
            // Check if there is already a batch with the same key
            for (int i = 0; i < Batches.Count; i++)
            {
                if (Batches[i].Key.Equals(key))
                {
                    int index = Batches[i].StartIndex + Batches[i].Count;

                    Append(i, value, index);

                    Count++;

                    return;
                }
            }

            
            // Create a new batch
            BatchItem<TKey> newBatch;

            newBatch.Key = key;
            newBatch.Count = 0;
            newBatch.StartIndex = 0;

            if (Batches.Count > 0)
            {
                BatchItem<TKey> lastBatch = Batches[Batches.Count - 1];

                if (lastBatch.StartIndex + lastBatch.Count >= Values.Length)
                    throw new OutOfMemoryException("Not enough space for new batches");

                // Leave space for last batch
                newBatch.StartIndex = lastBatch.StartIndex;
                newBatch.StartIndex += Math.Max(lastBatch.Count * 2, Values.Length / previousBatchCount / 2);

                if (newBatch.StartIndex >= Values.Length)
                    newBatch.StartIndex = Values.Length - 1;
            }

            Batches.Add(newBatch);

            Count++;
            Append(Batches.Count - 1, value, newBatch.StartIndex);
        }


        private void Append(int batch, TValue value, int index)
        {
            BatchItem<TKey> item;

            // Find the ending index of this batch
            int ending = Values.Length;

            if (batch + 1 < Batches.Count)
                ending = Batches[batch + 1].StartIndex;


            // Add the new value if this batch is not full
            if (index < ending)
            {
                Values[index] = value;
                item = Batches[batch];
                item.Count++;
                Batches[batch] = item;
                return;
            }


            // Prepend the new value if we reach the end of the buffer
            if (index >= Values.Length)
            {
                throw new OutOfMemoryException("Not enough space for new values");
            }


            // Shrink next batch
            BatchItem<TKey> nextBatch = Batches[batch + 1];

            TValue nextValue = Values[nextBatch.StartIndex];

            Values[index] = value;
            item = Batches[batch];
            item.Count++;
            Batches[batch] = item;

            Append(batch + 1, nextValue, nextBatch.StartIndex + nextBatch.Count);

            nextBatch.StartIndex++;
            Batches[batch + 1] = nextBatch;
        }
    }
}
