namespace Nine
{
    using System;
    using System.Linq;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Just a simple wrap around dictionary for Windows Phone & Xbox
    /// </summary>
    class HashSet<T> : ICollection<T>
    {
        Dictionary<T, int> dictionary = new Dictionary<T, int>();

        public int Count
        {
            get { return dictionary.Count; }
        }

        public void Add(T item)
        {
            if (dictionary.ContainsKey(item))
                return;
            dictionary.Add(item, 0);
        }

        public void Clear()
        {
            dictionary.Clear();
        }

        public Dictionary<T, int>.KeyCollection.Enumerator GetEnumerator()
        {
            return dictionary.Keys.GetEnumerator();
        }

        public int RemoveWhere(Predicate<T> predicate)
        {
            if (ToBeRemoved == null || ToBeRemoved.Length < dictionary.Keys.Count)
                ToBeRemoved = new T[dictionary.Keys.Count];
            
            int removed = 0;
            dictionary.Keys.CopyTo(ToBeRemoved, 0);
            for (int i = 0; i < ToBeRemoved.Length; i++)
            {
                if (predicate(ToBeRemoved[i]) && dictionary.Remove(ToBeRemoved[i]))
                    removed++;
                ToBeRemoved[i] = default(T);
            }
            return removed;
        }

        static T[] ToBeRemoved;

        bool ICollection<T>.Contains(T item)
        {
            throw new NotImplementedException();
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<T>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<T>.Remove(T item)
        {
            throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}