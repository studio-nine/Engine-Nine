namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;

    /// <summary>
    /// Defines a collection of directional lights that are sorted by importance.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DirectionalLightCollection : IList<DirectionalLight>, IList
    {
        static IComparer<DirectionalLight> comparer = new DirectionalLightComparer();

        int count;
        int version;
        DirectionalLight defaultLight;
        DirectionalLight[] elements;

        internal DirectionalLightCollection(DirectionalLight defaultLight) 
        {
            this.elements = new DirectionalLight[4];
            this.defaultLight = defaultLight;
        }

        private void SortAndIncrementVersion()
        {
            Array.Sort(elements, 0, count, comparer);
            version++;
        }

        private void EnsureCapacity()
        {
            if (count >= elements.Length)
                Array.Resize(ref elements, count * 2);
        }

        /// <summary>
        /// Gets the version of this collection. This value increments each time
        /// any new item is added, replaced or removed from this collection.
        /// </summary>
        public int Version
        {
            get { return version; }
        }

        public int IndexOf(DirectionalLight item)
        {
            return ((IList)elements).IndexOf(item);
        }

        public void Insert(int index, DirectionalLight item)
        {
            if (index < 0 || index > count)
                throw new ArgumentOutOfRangeException();

            EnsureCapacity();
            elements[++count] = elements[index];
            elements[index] = item;
            SortAndIncrementVersion();
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= count)
                throw new ArgumentOutOfRangeException();

            elements[index] = elements[--count];
            elements[count] = null;
            SortAndIncrementVersion();
        }

        public DirectionalLight this[int index]
        {
            get { return index >= count ? defaultLight : elements[index]; }
            set { elements[index] = value; SortAndIncrementVersion(); }
        }

        public void Add(DirectionalLight item)
        {
            Insert(count, item);
        }

        public void Clear()
        {
            Array.Clear(elements, 0, count);
            count = 0;
            version++;
        }

        public bool Contains(DirectionalLight item)
        {
            return ((IList)elements).Contains(item);
        }

        public void CopyTo(DirectionalLight[] array, int arrayIndex)
        {
            elements.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(DirectionalLight item)
        {
            for (int i = 0; i < count; ++i)
                if (item == elements[i])
                {
                    RemoveAt(i);
                    return true;
                }
            return false;
        }

        public IEnumerator<DirectionalLight> GetEnumerator()
        {
            return elements.Take(count).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        int IList.Add(object value)
        {
            Add((DirectionalLight)value);
            return count;
        }

        bool IList.Contains(object value)
        {
            return Contains((DirectionalLight)value);
        }

        int IList.IndexOf(object value)
        {
            return IndexOf((DirectionalLight)value);
        }

        void IList.Insert(int index, object value)
        {
            Insert(index, (DirectionalLight)value);
        }

        bool IList.IsFixedSize
        {
            get { return false; }
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get { return this[index]; }
            set { this[index] = (DirectionalLight)value; }
        }

        void ICollection.CopyTo(Array array, int index)
        {
            elements.CopyTo(array, index);
        }

        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        object ICollection.SyncRoot
        {
            get { return GetType(); }
        }
    }

    sealed class DirectionalLightComparer : IComparer<DirectionalLight>
    {
        public int Compare(DirectionalLight x, DirectionalLight y)
        {
            return x.order.CompareTo(y.order);
        }
    }
}