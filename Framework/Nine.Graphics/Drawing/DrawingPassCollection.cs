#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using System.Collections.Generic;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// Represents a collection of <see cref="DrawingPass"/> that are sorted by dependency.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrawingPassCollection : Collection<DrawingPass>, IComparer<DrawingPass>, IDependencyProvider<DrawingPass>
    {
        private int[] topologyReorder;
        
        internal bool TopologyChanged;
        internal bool PassOrderChanged;

        protected override void ClearItems()
        {
            for (int i = 0; i < Count; i++)
                this[i].Container = null;
            base.ClearItems();

            TopologyChanged = true;            
        }

        protected override void InsertItem(int index, DrawingPass item)
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

        protected override void SetItem(int index, DrawingPass item)
        {
            this[index].Container = null;
            base.SetItem(index, item);
            
            item.Container = this;
            
            TopologyChanged = true;
            PassOrderChanged = true;
        }

        internal int[] GetSortedOrder()
        {
            EnsureTopology();
            return topologyReorder;
        }

        private void EnsureTopology()
        {
            if (TopologyChanged)
            {
                if (PassOrderChanged)
                {
                    // Sort by pass order
                    ((List<DrawingPass>)Items).Sort(this);
                    PassOrderChanged = false;
                }

                if (topologyReorder == null || topologyReorder.Length < Count)
                    topologyReorder = new int[Count];

                // Sort by dependencies
                for (int i = 0; i < Count; i++)
                    this[i].Id = i;
                DependencyGraph.Sort(this, topologyReorder, this);

                TopologyChanged = false;
            }
        }

        int IDependencyProvider<DrawingPass>.GetDependencies(IList<DrawingPass> elements, int index, int[] dependencies)
        {
            var pass = elements[index];
            if (pass.DependentPasses == null)
                return 0;
            
            var num = 0;
            for (int i = 0; i < pass.DependentPasses.Count; i++)
            {
                var e = pass.DependentPasses.Elements[i];
                if (e.Container == this)
                    dependencies[num++] = e.Id;
            }
            return num;
        }

        int IComparer<DrawingPass>.Compare(DrawingPass x, DrawingPass y)
        {
            return x.order.CompareTo(y.order);
        }
    }
}