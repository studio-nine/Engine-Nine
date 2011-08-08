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
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Navigation
{        
    /// <summary>
    /// Use min heap to implement a priority queue.
    /// Used to implement Dijkstra's algorithm.
    /// </summary>
    /// <remarks>
    /// The size of the indexed priority queue is fixed.
    /// </remarks>
    internal sealed class PriorityQueue<T> where T : struct
    {
        [StructLayout(LayoutKind.Sequential)]
        internal struct Entry
        {
            public int Key;
            public T Value;

            public override string ToString()
            {
                return "[" + Key + "] " + Value.ToString();
            }
        }

        /// <summary>
        /// Internal queue elements.
        /// The first element is not used for easy index generation.
        /// </summary>
        Entry[] data;

        /// <summary>
        /// Keep track of the position of individual item in the heap.
        /// E.g. index[3] = 5 means that data[5] = 3;
        /// </summary>
        int[] index;

        /// <summary>
        /// Cost of each item
        /// </summary>
        float[] costs;

        /// <summary>
        /// Actual data length
        /// </summary>
        int count;

        /// <summary>
        /// Gets element index array
        /// </summary>
        public int[] Index
        {
            get { return index; }
        }

        /// <summary>
        /// Gets priority queue element count
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Gets priority queue capacity
        /// </summary>
        public int Capacity
        {
            get { return data.Length; }
        }

        /// <summary>
        /// Gets whether the queue is empty
        /// </summary>
        public bool IsEmpty
        {
            get { return count == 0; }
        }

        /// <summary>
        /// Retrive the minimun (top) element without removing it
        /// </summary>
        public Entry Top
        {
            get { return data[1]; }
        }

        /// <summary>
        /// Creates a priority queue to hold n elements
        /// </summary>
        /// <param name="capacity"></param>
        public PriorityQueue(int capacity)
        {
            if (capacity <= 0)
                throw new ArgumentOutOfRangeException();

            data = new Entry[capacity];
            costs = new float[capacity];
            index = new int[capacity];

            Clear();
        }

        /// <summary>
        /// Clear the priority queue
        /// </summary>
        public void Clear()
        {
            Array.Clear(costs, 0, costs.Length);
            Array.Clear(index, 0, index.Length);

            count = 0;
        }

        /// <summary>
        /// Adds an element to the queue
        /// </summary>
        public void Add(int element, float cost, T value)
        {
            int i, x;

            // Bubble up the heap
            i = ++count;            
            while (i > 0)
            {
                x = i >> 1;
                if (x > 0 && cost < costs[x])
                {
                    costs[i] = costs[x];
                    data[i] = data[x];
                    index[data[x].Key] = i + 1;
                    i = x;
                }
                else
                    break;
            }

            // Assign the new element
            costs[i] = cost;
            data[i].Key = element;
            data[i].Value = value;
            index[element] = i + 1;
        }

        /// <summary>
        /// Remove and retrieve the minimun (top) element
        /// </summary>
        public Entry Pop()
        {
            if (count <= 0)
                throw new InvalidOperationException();

            // Make use of the first element here
            Entry top = data[1];
            index[top.Key] = 1;
            FixHeap(1, count - 1, data[count].Key, costs[count], data[count].Value);
            count--;
            return top;
        }

        /// <summary>
        /// Increase the priority of a given node
        /// </summary>
        public void IncreasePriority(int element, float cost, T value)
        {
            int x, i;

            // Check to see if the element is in the heap
            i = index[element] - 1;
            if (i <= 0)
                return;

            // Bubble up the heap
            while (i > 0)
            {
                x = i >> 1;
                if (x > 0 && cost < costs[x])
                {
                    costs[i] = costs[x];
                    data[i] = data[x];
                    index[data[x].Key] = i + 1;
                    i = x;
                }
                else
                    break;
            }

            // Assign the new element
            costs[i] = cost;
            data[i].Key = element;
            data[i].Value = value;
            index[element] = i + 1;
        }

        /// <summary>
        /// Fix the heap
        /// </summary>
        /// <param name="cost"></param>
        /// <param name="value"></param>
        /// <param name="i">Root index of the subtree</param>
        /// <param name="n">Subtree size</param>
        /// <param name="k">Element to be add as the root</param>
        void FixHeap(int i, int n, int k, float cost, T value)
        {
            int x, min;
            while (i <= n)
            {
                x = i << 1;         /* Left subtree */
                if (x > n)
                    break;
                else if (x == n)   /* No right subtree */
                    min = x;
                else
                    min = (costs[x] < costs[x + 1]) ? x : x + 1;

                if (costs[min] < cost)
                {
                    costs[i] = costs[min];
                    data[i] = data[min];  /* Sink if k is bigger */
                    index[data[min].Key] = i + 1;
                    i = min;
                }
                else
                {
                    break;          /* Otherwise fix is done */
                }
            }

            costs[i] = cost;
            data[i].Key = k;
            data[i].Value = value;
            index[k] = i + 1;
        }
    }
}
