namespace Nine
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using Microsoft.Xna.Framework.Content;
    
    /// <summary>
    /// A collection that can notify changes.
    /// </summary>
    [DebuggerDisplay("Count = {Count}")]
    [DebuggerTypeProxy("System.Collections.Generic.Mscorlib_CollectionDebugView`1, mscorlib")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    class NotificationCollection<T> : IList<T>, IList, INotifyCollectionChanged<T> where T : class
    {
        internal List<T> Elements = new List<T>();

        private List<T> copy = null;
        private bool isDirty = true;

        /// <summary>
        /// Gets or sets the sender that raise the Added and Removed event.
        /// </summary>
        internal object Sender = null;
        internal bool EnableManipulationWhenEnumerating;
        
        /// <summary>
        /// Creates a new instance of EnumerableCollection.
        /// </summary>
        public NotificationCollection()
        {
            Sender = this;
        }

        /// <summary>
        /// Raised when a new element is added to the collection.
        /// </summary>
        public event Action<T> Added;

        /// <summary>
        /// Raised when an element is removed from the collection.
        /// </summary>
        public event Action<T> Removed;
                
        /// <summary>
        /// Gets the enumerator associated with is collection.
        /// </summary>
        public NotificationCollectionEnumerator<T> GetEnumerator()
        {
            if (!EnableManipulationWhenEnumerating)
            {
                return new NotificationCollectionEnumerator<T>() { List = Elements ?? Empty, CurrentIndex = -1 };
            }

            // Copy a new list while iterating it
            if (isDirty)
            {
                if (copy == null)
                    copy = new List<T>();
                else
                    copy.Clear();

                int count = Elements.Count;
                for (int i = 0; i < count; ++i)
                    copy.Add(Elements[i]);

                isDirty = false;
            }

            return new NotificationCollectionEnumerator<T>() { List = copy, CurrentIndex = -1 };
        }

        static List<T> Empty = new List<T>();

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
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
            if (value == null)
                return;

            isDirty = true;
            Elements.Add(value);

            OnAdded(value);
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
            isDirty = true;
            int index = Elements.IndexOf(value);
            if (index < 0)
                return false;

            Elements.RemoveAt(index);

            OnRemoved(value);

            return true;
        }

        /// <summary>
        /// Clears the collection
        /// </summary>
        public void Clear()
        {
            isDirty = true;
            for (int i = 0; i < Elements.Count; ++i)
                OnRemoved(Elements[i]);
            Elements.Clear();
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
            get { return Elements.Count; }
        }

        /// <summary>
        /// Gets whether the list contains the specifed value.
        /// </summary>
        public bool Contains(T item)
        {
            return Elements.Contains(item);
        }

        /// <summary>
        /// Copy the list content to an array at the specified index.
        /// </summary>
        public void CopyTo(T[] array, int arrayIndex)
        {
            Elements.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Gets the index of an item.
        /// </summary>
        public int IndexOf(T item)
        {
            return Elements.IndexOf(item);
        }

        /// <summary>
        /// Inserts an item at the specifed index.
        /// </summary>
        public void Insert(int index, T item)
        {
            isDirty = true;
            Elements.Insert(index, item);

            OnAdded(item);
        }

        /// <summary>
        /// Removes an item at the specified index.
        /// </summary>
        public void RemoveAt(int index)
        {
            isDirty = true;
            T e = Elements[index];
            Elements.RemoveAt(index);

            OnRemoved(e);
        }

        /// <summary>
        /// Removes all items that matches the specified condition.
        /// </summary>
        public int RemoveAll(Predicate<T> match)
        {
            isDirty = true;

            int count = 0;

            for (int i = 0; i < Elements.Count; ++i)
            {
                if (match(Elements[i]))
                {
                    T e = Elements[i];

                    Elements.RemoveAt(i);

                    OnRemoved(e);

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
            get { return Elements[index]; }            
            set
            {
                T oldValue = Elements[index];
                OnRemoved(oldValue);
                Elements[index] = value;
                isDirty = true;
                OnAdded(value);
            }
        }

        /// <summary>
        /// Raised when a new element is added to the collection.
        /// </summary>
        protected virtual void OnAdded(T value)
        {
            var added = Added;
            if (added != null)
                added(value);
        }

        /// <summary>
        /// Raised when an element is removed from the collection.
        /// </summary>
        protected virtual void OnRemoved(T value)
        {
            var removed = Removed;
            if (removed != null)
                removed(value);
        }

        #region IList
        int IList.Add(object value)
        {
            Add((T)value);
            return Count - 1;
        }
        
        bool IList.Contains(object value)
        {
            return Contains((T)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((T)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (T)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        bool IList.IsReadOnly
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            Remove((T)value);
        }

        object IList.this[int index]
        {
            get { return (T)this[index]; }
            set { this[index] = (T)value; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            if (Elements != null)
                ((IList)Elements).CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return null; }
        }
        #endregion
    }

    /// <summary>
    /// An optimized enumerator for notification collection.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public struct NotificationCollectionEnumerator<T> : IEnumerator<T>
    {
        internal List<T> List;
        internal int CurrentIndex;

        public T Current
        {
            get { return List[CurrentIndex]; }
        }

        public void Dispose()
        {

        }

        object IEnumerator.Current
        {
            get { return List[CurrentIndex]; }
        }

        public bool MoveNext()
        {
            return ++CurrentIndex < List.Count;
        }

        public void Reset()
        {
            CurrentIndex = -1;
        }
    }
}