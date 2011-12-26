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
    class PriorityQueue
    {
        /// <summary>
        /// Internal queue elements.
        /// The first element is not used for easy index generation.
        /// </summary>
        int[] data;

        /// <summary>
        /// Priority of each item.
        /// </summary>
        float[] priorities;

        /// <summary>
        /// Actual data length
        /// </summary>
        int count;

        /// <summary>
        /// Keep track of the position of individual item in the heap.
        /// E.g. index[3] = 5 means that data[5] = 3;
        /// </summary>
        public int[] Index;

        /// <summary>
        /// Gets the number of elements added to the priority queue.
        /// </summary>
        public int Count
        {
            get { return count; }
        }

        /// <summary>
        /// Retrive the minimun (top) element without removing it
        /// </summary>
        public int Top
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

            data = new int[capacity];
            priorities = new float[capacity];
            Index = new int[capacity];

            Clear();
        }
        
        /// <summary>
        /// Clear the priority queue
        /// </summary>
        public void Clear()
        {
            UtilityExtensions.FastClear(Index);
            count = 0;
        }

        /// <summary>
        /// Adds an element to the queue
        /// </summary>
        public void Push(int element, float priority)
        {
            int i, x;

            // Bubble up the heap
            i = ++count;            
            while (i > 0)
            {
                x = i >> 1;
                if (x > 0 && priority < priorities[x])
                {
                    priorities[i] = priorities[x];
                    data[i] = data[x];
                    Index[data[x]] = i + 1;
                    i = x;
                }
                else
                    break;
            }

            // Assign the new element
            priorities[i] = priority;
            data[i] = element;
            Index[element] = i + 1;
        }

        /// <summary>
        /// Remove and retrieve the minimun (top) element
        /// </summary>
        public int Pop()
        {
            if (count <= 0)
                throw new InvalidOperationException();

            // Make use of the first element here
            int top = data[1];
            Index[top] = 1;
            FixHeap(1, count - 1, data[count], priorities[count]);
            count--;
            return top;
        }

        /// <summary>
        /// Increase the priority of a given node
        /// </summary>
        public void IncreasePriority(int element, float priority)
        {
            int x, i;

            // Check to see if the element is in the heap
            i = Index[element] - 1;
            if (i <= 0)
                return;

            // Bubble up the heap
            while (i > 0)
            {
                x = i >> 1;
                if (x > 0 && priority < priorities[x])
                {
                    priorities[i] = priorities[x];
                    data[i] = data[x];
                    Index[data[x]] = i + 1;
                    i = x;
                }
                else
                    break;
            }

            // Assign the new element
            priorities[i] = priority;
            data[i] = element;
            Index[element] = i + 1;
        }

        /// <summary>
        /// Fix the heap
        /// </summary>
        /// <param name="cost"></param>
        /// <param name="value"></param>
        /// <param name="i">Root index of the subtree</param>
        /// <param name="n">Subtree size</param>
        /// <param name="k">Element to be add as the root</param>
        void FixHeap(int i, int n, int k, float cost)
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
                    min = (priorities[x] < priorities[x + 1]) ? x : x + 1;

                if (priorities[min] < cost)
                {
                    priorities[i] = priorities[min];
                    data[i] = data[min];  /* Sink if k is bigger */
                    Index[data[min]] = i + 1;
                    i = min;
                }
                else
                {
                    break;          /* Otherwise fix is done */
                }
            }

            priorities[i] = cost;
            data[i] = k;
            Index[k] = i + 1;
        }
    }
}
