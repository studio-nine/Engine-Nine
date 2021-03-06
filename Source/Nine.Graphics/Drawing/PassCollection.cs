namespace Nine.Graphics.Drawing
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    /// <summary>
    /// Represents a collection of <see cref="Pass"/> that are sorted by dependency.
    /// </summary>
    class PassCollection : Collection<Pass>, IComparer<Pass>, IDependencyProvider<Pass>
    {
        private int[] topologyReorder;
        private FastList<int> order;
                
        internal bool TopologyChanged;
        internal bool PassOrderChanged;

        protected override void ClearItems()
        {
            for (int i = 0; i < Count; ++i)
                this[i].Container = null;
            base.ClearItems();

            TopologyChanged = true;            
        }

        protected override void InsertItem(int index, Pass item)
        {
            base.InsertItem(index, item);
            item.Container = this;

            TopologyChanged = true;
            PassOrderChanged = true;
        }

        protected override void RemoveItem(int index)
        {
            this[index].Container = null;
            base.RemoveItem(index);

            TopologyChanged = true;
        }

        protected override void SetItem(int index, Pass item)
        {
            this[index].Container = null;
            base.SetItem(index, item);
            
            item.Container = this;
            
            TopologyChanged = true;
            PassOrderChanged = true;
        }

        internal FastList<int> GetEnabledAndSortedOrder()
        {
            EnsureTopology();

            if (order == null || order.Count < Count)
                order = new FastList<int>(Count);
            else
                order.Clear();

            if (topologyReorder != null)
            {
                for (int i = 0; i < topologyReorder.Length; ++i)
                {
                    var index = topologyReorder[i];
                    var pass = this[index];
                    if (pass != null && pass.Enabled)
                        order.Add(index);
                }
            }
            else
            {
                for (int i = 0; i < Count; ++i)
                {
                    var pass = this[i];
                    if (pass != null && pass.Enabled)
                        order.Add(i);
                }
            }
            return order;
        }

        private void EnsureTopology()
        {
            if (TopologyChanged)
            {
                if (PassOrderChanged)
                {
                    // Sort by pass order using a stable sort algorithm.

                    InsertionSort(Items, this);
                    PassOrderChanged = false;
                }

                if (topologyReorder == null || topologyReorder.Length < Count)
                    topologyReorder = new int[Count];

                // Sort by dependencies
                for (int i = 0; i < Count; ++i)
                    this[i].Id = i;
                DependencyGraph.Sort(this, topologyReorder, this);

                TopologyChanged = false;
            }
        }
        
        private static void InsertionSort<T>(IList<T> list, IComparer<T> comparer)
        {
            for (int j = 1; j < list.Count; j++)
            {
                T key = list[j];

                int i = j - 1;
                for (; i >= 0 && comparer.Compare(list[i], key) > 0; i--)
                    list[i + 1] = list[i];

                list[i + 1] = key;
            }
        }

        int IDependencyProvider<Pass>.GetDependencies(IList<Pass> elements, int index, int[] dependencies)
        {
            var pass = elements[index];
            if (pass.DependentPasses == null)
                return 0;
            
            var num = 0;
            for (int i = 0; i < pass.DependentPasses.Count; ++i)
            {
                var e = pass.DependentPasses.Elements[i];
                if (e.Container == this)
                    dependencies[num++] = e.Id;
            }
            return num;
        }

        int IComparer<Pass>.Compare(Pass x, Pass y)
        {
            return x.order.CompareTo(y.order);
        }
    }
}