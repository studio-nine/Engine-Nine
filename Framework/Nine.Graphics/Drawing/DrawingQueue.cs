namespace Nine.Graphics.Drawing
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

    /// <summary>
    /// Each item in the queue will hold a strong reference to the IDrawableObject.
    /// By using a circular queue, chances are that any unused old references will be
    /// replaced by newly added objects as long as the render loop goes on.
    /// </summary>
    class DrawingQueue
    {
        private MaterialSortComparer materialSortComparer;
        private ViewDistanceSortComparer viewDistanceSortComparer;

        public int Count;
        public DrawingQueueEntry[] Elements = new DrawingQueueEntry[32];

        public void Add(IDrawableObject drawable, Material material)
        {
            if (Count >= Elements.Length)
                Array.Resize(ref Elements, Elements.Length * 2);

            var entry = Elements[Count];
            if (entry != null)
            {
                entry.Drawable = drawable;
                entry.Material = material;
                Count++;
                return;
            }

            Elements[Count++] = new DrawingQueueEntry() 
            {
                Drawable = drawable,
                Material = material,
            };
        }

        public void Clear()
        {
            Count = 0;
        }

        public void SortByMaterial()
        {
            Array.Sort(Elements, 0, Count, materialSortComparer);
        }

        public void SortByViewDistance(ref Vector3 cameraPosition)
        {
            for (var i = 0; i < Count; ++i)
            {
                var entry = Elements[i];
                entry.ViewDistanceSq = entry.Drawable.GetDistanceToCamera(cameraPosition);
            }

            Array.Sort(Elements, 0, Count, viewDistanceSortComparer);
        }

        class MaterialSortComparer : IComparer<DrawingQueueEntry>
        {
            public int Compare(DrawingQueueEntry x, DrawingQueueEntry y)
            {
                return x.Material.SortOrder - y.Material.SortOrder;
            }
        }

        class ViewDistanceSortComparer : IComparer<DrawingQueueEntry>
        {
            public int Compare(DrawingQueueEntry x, DrawingQueueEntry y)
            {
                return x.ViewDistanceSq.CompareTo(y.ViewDistanceSq);
            }
        }
    }

    class DrawingQueueEntry
    {
        public IDrawableObject Drawable;
        public Material Material;
        public float ViewDistanceSq;
    }
}