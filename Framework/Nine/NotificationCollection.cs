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
    class NotificationCollection<T> : IList<T>, IList, INotifyCollectionChanged<T>
    {
        private List<T> elements = null;
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
                return new NotificationCollectionEnumerator<T>() { List = elements ?? Empty, CurrentIndex = -1 };
            }

            // Copy a new list while iterating it
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
            if (elements == null)
                elements = new List<T>();

            isDirty = true;
            elements.Add(value);

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
            if (elements == null)
                return false;

            isDirty = true;
            int index = elements.IndexOf(value);
            if (index < 0)
                return false;

            elements.RemoveAt(index);

            OnRemoved(value);

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

                if (elements != null)
                    for (int i = 0; i < elements.Count; ++i)
                        OnRemoved(elements[i]);

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

            OnAdded(item);
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

                OnRemoved(e);
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

            for (int i = 0; i < elements.Count; ++i)
            {
                if (match(elements[i]))
                {
                    T e = elements[i];

                    elements.RemoveAt(i);

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
            get 
            {
                return elements != null ? elements[index] : default(T); 
            }
            
            set
            {
                if (elements == null)
                    elements = new List<T>();

                T oldValue = elements[index];
                OnRemoved(oldValue);
                elements[index] = value;
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
            if (elements != null)
                ((IList)elements).CopyTo(array, index);
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

    class NotificationCollectionReader<T> : ContentTypeReader<NotificationCollection<T>>
    {
        ContentTypeReader elementReader;

        public override bool CanDeserializeIntoExistingObject
        {
            get { return true; }
        }

        protected override void Initialize(ContentTypeReaderManager manager)
        {
            elementReader = manager.GetTypeReader(typeof(T));
        }

        protected override NotificationCollection<T> Read(ContentReader input, NotificationCollection<T> existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new NotificationCollection<T>();

            var count = input.ReadInt32();
            for (int i = 0; i < count; ++i)
                existingInstance.Add(input.ReadObject<T>(elementReader));
            return existingInstance;
        }
    }
}