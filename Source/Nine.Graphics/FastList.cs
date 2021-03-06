namespace Nine.Graphics
{
    using System;
    using System.Collections;
    using System.Collections.Generic;

    class FastList<T> : IList<T>
    {
        public int Count;
        public int Capacity;
        public T[] Elements;

        public FastList() : this(4) { }

        public FastList(int capacity)
        {
            this.Capacity = capacity;
            this.Elements = new T[capacity];
        }

        public void Add(T item)
        {
            if (Count == Capacity)
                Array.Resize(ref Elements, Capacity = Capacity * 2);
            Elements[Count++] = item;
        }

        public void Clear()
        {
            Count = 0;
        }

        public void Resize(int length)
        {
            if (length > Count)
                Array.Resize(ref Elements, Capacity = length);
            Count = length;
        }

        public T this[int index]
        {
            get { return Elements[index]; }
            set { Elements[index] = value; }
        }

        public bool Remove(T item)
        {
            throw new NotImplementedException();
        }

        public bool Contains(T item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<T>.Count
        {
            get { return Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public int IndexOf(T item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, T item)
        {
            throw new NotImplementedException();
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }
    }
}