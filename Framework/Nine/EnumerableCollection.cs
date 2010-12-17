#region Copyright 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
#endregion

namespace Nine
{
    /// <summary>
    /// Event args for changed an item.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ItemChangedEventArgs<T> : EventArgs
    {
        /// <summary>
        /// Gets the index of the item.
        /// </summary>
        public int Index { get; internal set; }

        /// <summary>
        /// Gets the value of the item.
        /// </summary>
        public T Value { get; internal set; }

        /// <summary>
        /// Gets the old value of the changed item.
        /// </summary>
        public T PreviousValue { get; internal set; }
    }


    /// <summary>
    /// A collection that can be manipulated during enumeration.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumerableCollection<T> : IList<T>
    {
        private bool isDirty = true;
        private List<T> elements = null;
        private List<T> copy = null;
        
        /// <summary>
        /// Creates a new instance of EnumerableCollection.
        /// </summary>
        public EnumerableCollection() { }

        /// <summary>
        /// Creates a new instance of EnumerableCollection.
        /// </summary>
        public EnumerableCollection(IEnumerable<T> elements)
        {
            AddRange(elements);
        }

        /// <summary>
        /// Raised when a new element is added to the collection.
        /// </summary>
        public event EventHandler<ItemChangedEventArgs<T>> Added;

        /// <summary>
        /// Raised when an element is removed from the collection.
        /// </summary>
        public event EventHandler<ItemChangedEventArgs<T>> Removed;

        /// <summary>
        /// Raised when an element is changed to a different value.
        /// </summary>
        public event EventHandler<ItemChangedEventArgs<T>> Changed;

        
        /// <summary>
        /// Gets the enumerator associated with is collection.
        /// </summary>
        public IEnumerator<T> GetEnumerator()
        {
            // Copy a new list whiling iterating it
            if (isDirty)
            {
                if (copy == null)
                    copy = new List<T>();
                else
                    copy.Clear();

                if (elements != null)
                {
                    foreach (T e in elements)
                        copy.Add(e);
                }

                isDirty = false;
            }

            // NOTE: If copy contains references to an object and this
            //       collection is never enumerated again, copy array
            //       will keep the reference to the object as long as
            //       this collection is alive, will this be a potential
            //       memery pitfall?
            return copy.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        /// <summary>
        /// Adds a new item to the collection.
        /// </summary>
        public void Add(T value)
        {
            if (elements == null)
                elements = new List<T>();

            isDirty = true;
            elements.Add(value);

            OnAdded(elements.Count - 1, value);
        }

        /// <summary>
        /// Adds a collection of items to this collection.
        /// </summary>
        public void AddRange(IEnumerable<T> elements)
        {
            foreach (T e in elements)
                Add(e);
        }

        /// <summary>
        /// Removes the first occurrance of an item from the collection.
        /// </summary>
        public bool Remove(T value)
        {
            if (elements == null)
                return false;

            isDirty = true;
            int index = elements.IndexOf(value);
            if (index < 0)
                return false;

            elements.RemoveAt(index);

            OnRemoved(index, value);

            return true;
        }

        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            if (elements != null)
            {
                isDirty = true;
                List<T> temp = elements;
                elements = null;

                if (Removed != null && elements != null)
                    for (int i = 0; i < elements.Count; i++)
                        OnRemoved(i, elements[i]);

                temp.Clear();
            }
        }

        /// <summary>
        /// Gets whether the list is readonly.
        /// </summary>
        public bool IsReadOnly
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the count of the list.
        /// </summary>
        public int Count
        {
            get { return elements != null ? elements.Count : 0; }
        }

        /// <summary>
        /// Gets whether the list contains the specifed value.
        /// </summary>
        public bool Contains(T item)
        {
            return elements != null ? elements.Contains(item) : false;
        }

        /// <summary>
        /// Copy the list content to an array at the specified index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            if (elements != null)
                elements.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the index of an item.
        /// </summary>
        public int IndexOf(T item)
        {
            return elements != null ?  elements.IndexOf(item) : -1;
        }

        /// <summary>
        /// Inserts an item at the specifed index.
        /// </summary>
        public void Insert(int index, T item)
        {
            if (elements == null)
                elements = new List<T>();

            isDirty = true;
            elements.Insert(index, item);

            OnAdded(index, item);
        }

        /// <summary>
        /// Removes an item at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            if (elements != null)
            {
                isDirty = true;
                T e = elements[index];
                elements.RemoveAt(index);

                OnRemoved(index, e);
            }
        }

        /// <summary>
        /// Removes all items that matches the specified condition.
        /// </summary>
        public int RemoveAll(Predicate<T> match)
        {
            if (elements == null)
                return 0;

            isDirty = true;

            int count = 0;

            for (int i = 0; i < elements.Count; i++)
            {
                if (match(elements[i]))
                {
                    T e = elements[i];

                    elements.RemoveAt(i);

                    OnRemoved(i, e);

                    i--;
                    count++;
                }
            }

            return count;
        }

        /// <summary>
        /// Gets or sets the value at the given index.
        /// </summary>
        public T this[int index]
        {
            get 
            {
                return elements != null ? elements[index] : default(T); 
            }
            
            set
            {
                if (elements == null)
                    elements = new List<T>();

                T oldValue = elements[index];
                elements[index] = value; 
                isDirty = true;

                OnChanged(index, value, oldValue);
            }
        }

        /// <summary>
        /// Raised when a new element is added to the collection.
        /// </summary>
        protected virtual void OnAdded(int index, T value)
        {
            if (Added != null)
                Added(this, new ItemChangedEventArgs<T> { Index = index, Value = value });
        }

        /// <summary>
        /// Raised when an element is removed from the collection.
        /// </summary>
        protected virtual void OnRemoved(int index, T value)
        {
            if (Removed != null)
                Removed(this, new ItemChangedEventArgs<T> { Index = index, Value = value });
        }

        /// <summary>
        /// Raised when an element is changed to a different value.
        /// </summary>
        protected virtual void OnChanged(int index, T value, T previousValue)
        {
            if (Changed != null && !previousValue.Equals(value))
                Changed(this, new ItemChangedEventArgs<T> { Index = index, Value = value, PreviousValue = previousValue });
        }
    }
}