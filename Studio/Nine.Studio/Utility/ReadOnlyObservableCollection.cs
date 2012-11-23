namespace Nine.Studio
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Windows.Threading;

    /// <summary>
    /// http://karlhulme.wordpress.com/2007/03/04/synchronizedobservablecollection-and-bindablecollection/
    /// </summary> 
    class ReadOnlyObservableCollection<T> : ReadOnlyCollection<T>, INotifyPropertyChanged, INotifyCollectionChanged
    {
        #region Constructor
        public ReadOnlyObservableCollection(IList<T> list) : base(list)
        {
            ((INotifyPropertyChanged)list).PropertyChanged += delegate(Object sender, PropertyChangedEventArgs e)
            {
                OnPropertyChanged(e);
            };
            ((INotifyCollectionChanged)list).CollectionChanged += delegate(Object sender, NotifyCollectionChangedEventArgs e)
            {
                OnCollectionChanged(e);
            };
        }
        #endregion

        #region Event Handling
        private NotifyCollectionChangedEventHandler collectionChanged;
        private PropertyChangedEventHandler propertyChanged;

        protected event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { collectionChanged += value; }
            remove { collectionChanged -= value; }
        }

        protected event PropertyChangedEventHandler PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        event NotifyCollectionChangedEventHandler INotifyCollectionChanged.CollectionChanged
        {
            add { collectionChanged += value; }
            remove { collectionChanged -= value; }
        }

        event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
        {
            add { propertyChanged += value; }
            remove { propertyChanged -= value; }
        }

        protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            if (propertyChanged != null)
            {
                propertyChanged(this, e);
            }
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (collectionChanged != null)
            {
                collectionChanged(this, e);
            }
        }
        #endregion
    }
}
