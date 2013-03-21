namespace Nine.Studio
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows.Threading;

    /// <summary>
    /// http://karlhulme.wordpress.com/2007/03/04/synchronizedobservablecollection-and-bindablecollection/
    /// </summary>
    class BindableCollection<T> : IList<T>, IList, INotifyCollectionChanged, INotifyPropertyChanged
    {
        private IList<T> list;
        private Dispatcher dispatcher;

        #region Constructor
        public BindableCollection(IList<T> list) : this(list, null) { }

        public BindableCollection(IList<T> list, Dispatcher dispatcher)
        {
            if (list == null ||
                list as INotifyCollectionChanged == null ||
                list as INotifyPropertyChanged == null)
            {
                throw new ArgumentNullException("The list must support IList, INotifyCollectionChanged " +
                    "and INotifyPropertyChanged.");
            }

            this.list = list;
            this.dispatcher = (dispatcher == null) ? Dispatcher.CurrentDispatcher : dispatcher;

            INotifyCollectionChanged collectionChanged = list as INotifyCollectionChanged;
            collectionChanged.CollectionChanged += delegate(Object sender, NotifyCollectionChangedEventArgs e)
            {
                this.dispatcher.Invoke(DispatcherPriority.Normal,
                    new RaiseCollectionChangedEventHandler(RaiseCollectionChangedEvent), e);
            };

            INotifyPropertyChanged propertyChanged = list as INotifyPropertyChanged;
            propertyChanged.PropertyChanged += delegate(Object sender, PropertyChangedEventArgs e)
            {
                this.dispatcher.Invoke(DispatcherPriority.Normal,
                    new RaisePropertyChangedEventHandler(RaisePropertyChangedEvent), e);
            };
        }
        #endregion

        #region INotifyCollectionChanged Members

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void RaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
        {
            if (CollectionChanged != null)
            {
                CollectionChanged(this, e);
            }
        }

        private delegate void RaiseCollectionChangedEventHandler(NotifyCollectionChangedEventArgs e);

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChangedEvent(PropertyChangedEventArgs e)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, e);
            }
        }

        private delegate void RaisePropertyChangedEventHandler(PropertyChangedEventArgs e);

        #endregion

        #region ICollection<T> Members

        public void Add(T item)
        {
            list.Add(item);
        }

        public void Clear()
        {
            list.Clear();
        }

        public bool Contains(T item)
        {
            return list.Contains(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return list.Count; }
        }

        public bool IsReadOnly
        {
            get { return list.IsReadOnly; }
        }

        public bool Remove(T item)
        {
            return list.Remove(item);
        }

        #endregion

        #region IList<T> Members

        public int IndexOf(T item)
        {
            return list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            list.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            list.RemoveAt(index);
        }

        public T this[int index]
        {
            get
            {
                return list[index];
            }
            set
            {
                list[index] = value;
            }
        }
        #endregion

        #region IEnumerable<T> Members

        public IEnumerator<T> GetEnumerator()
        {
            return list.GetEnumerator();
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            ((ICollection)list).CopyTo(array, index);
        }

        int ICollection.Count
        {
            get { return ((ICollection)list).Count; }
        }

        bool ICollection.IsSynchronized
        {
            get { return ((ICollection)list).IsSynchronized; }
        }

        object ICollection.SyncRoot
        {
            get { return ((ICollection)list).SyncRoot; }
        }

        #endregion

        #region IList Members

        int IList.Add(object value)
        {
            return ((IList)list).Add(value);
        }

        void IList.Clear()
        {
            ((IList)list).Clear();
        }

        bool IList.Contains(object value)
        {
            return ((IList)list).Contains(value);
        }

        int IList.IndexOf(object value)
        {
            return ((IList)list).IndexOf(value);
        }

        void IList.Insert(int index, object value)
        {
            ((IList)list).Insert(index, value);
        }

        bool IList.IsFixedSize
        {
            get { return ((IList)list).IsFixedSize; }
        }

        bool IList.IsReadOnly
        {
            get { return ((IList)list).IsReadOnly; }
        }

        void IList.Remove(object value)
        {
            ((IList)list).Remove(value);
        }

        void IList.RemoveAt(int index)
        {
            ((IList)list).RemoveAt(index);
        }

        object IList.this[int index]
        {
            get
            {
                return ((IList)list)[index];
            }
            set
            {
                ((IList)list)[index] = value;
            }
        }

        #endregion

        #region IEnumerable Members

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return ((System.Collections.IEnumerable)list).GetEnumerator();
        }

        #endregion

    }
}
