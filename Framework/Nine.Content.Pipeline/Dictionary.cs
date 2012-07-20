namespace Nine.Content.Pipeline
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Markup;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Dictionary : IDictionary
    {
        protected Dictionary() { }
        protected virtual IDictionary InnerDictionary { get { UpdateDictionary(); return dictionary; } }
        private IDictionary dictionary;

        private void UpdateDictionary()
        {
            if (dictionary == null)
            {
                var property = GetType().GetCustomAttributes(typeof(ContentPropertyAttribute), true).OfType<ContentPropertyAttribute>().FirstOrDefault();
                if (property != null)
                {
                    dictionary = (IDictionary)(new PropertyExpression<object>(this, property.Name).Value);
                }
            }
        }

        #region IDictionary
        public void Add(object key, object value)
        {
            InnerDictionary.Add((string)key, value);
        }

        public void Clear()
        {
            InnerDictionary.Clear();
        }

        public bool Contains(object key)
        {
            return InnerDictionary.Contains(key);
        }

        public IDictionaryEnumerator GetEnumerator()
        {
            return InnerDictionary.GetEnumerator();
        }

        public bool IsFixedSize
        {
            get { return InnerDictionary.IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return InnerDictionary.IsReadOnly; }
        }

        public ICollection Keys
        {
            get { return InnerDictionary.Keys; }
        }

        public void Remove(object key)
        {
            InnerDictionary.Remove((string)key);
        }

        public ICollection Values
        {
            get { return InnerDictionary.Values; }
        }

        public object this[object key]
        {
            get { return InnerDictionary[(string)key]; }
            set { InnerDictionary[(string)key] = value; }
        }

        public void CopyTo(Array array, int index)
        {
            InnerDictionary.CopyTo(array, index);
        }

        public int Count
        {
            get { return InnerDictionary.Count; }
        }

        public bool IsSynchronized
        {
            get { return InnerDictionary.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return InnerDictionary.SyncRoot; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InnerDictionary.GetEnumerator();
        }
        #endregion
    }
}
