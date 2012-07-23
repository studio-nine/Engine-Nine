namespace Nine.Navigation.Steering
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SteeringBehaviorCollection : ICollection<ISteeringBehavior>
    {
        Collection<KeyValuePair<ISteeringBehavior, float>> array = new Collection<KeyValuePair<ISteeringBehavior, float>>();

        public void Add(ISteeringBehavior behavior, float weight)
        {
            array.Add(new KeyValuePair<ISteeringBehavior, float>(behavior, weight));
        }

        public float GetWeight(ISteeringBehavior behavior)
        {
            for (int i = 0; i < Count; i++)
            {
                if (array[i].Key == behavior)
                {
                    return array[i].Value;
                }
            }

            return 0;
        }

        public ISteeringBehavior this[int index]
        {
            get { return array[index].Key; }
        }

        public T FindFirst<T>()
        {
            for (int i = 0; i < Count; i++)
                if (array[i].Key is T)
                    return (T)array[i].Key;

            return default(T);
        }

        public IEnumerable<T> FindAll<T>()
        {
            for (int i = 0; i < Count; i++)
                if (array[i].Key is T)
                    yield return (T)array[i].Key;
        }

        internal float GetWeightByIndex(int index)
        {
            return array[index].Value;
        }

        public void Add(ISteeringBehavior behavior)
        {
            array.Add(new KeyValuePair<ISteeringBehavior, float>(behavior, 1.0f));
        }

        public void Clear()
        {
            array.Clear();
        }

        public bool Contains(ISteeringBehavior item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (array[i].Key == item)
                {
                    return true;
                }
            }

            return false;
        }

        public void CopyTo(ISteeringBehavior[] array, int arrayIndex)
        {
            for (int i = 0; i < Count; i++)
            {
                array[i + arrayIndex] = this.array[i].Key;
            }
        }

        public int Count
        {
            get { return array.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ISteeringBehavior item)
        {
            for (int i = 0; i < Count; i++)
            {
                if (array[i].Key == item)
                {
                    array.RemoveAt(i);
                    return true;
                }
            }

            return false;
        }
        
        public IEnumerator<ISteeringBehavior> GetEnumerator()
        {
            for (int i = 0; i < Count; i++)
                yield return array[i].Key;
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}