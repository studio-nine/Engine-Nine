#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// Each item in the queue will hold a strong reference to the IDrawableObject.
    /// By using a circular queue, chances are that any unused old references will be
    /// replaced by newly added objects as long as the render loop goes on.
    /// </summary>
    class DrawingQueue
    {
        private FastList<IDrawableObject> entries = new FastList<IDrawableObject>();
        private FastList<DrawingQueueEntry> sortingEntries = new FastList<DrawingQueueEntry>();

        public void Add(IDrawableObject drawable)
        {
            var material = drawable.Material;
            if (material == null)
            {
                entries.Add(drawable);
            }
            else
            {
                var entry = new DrawingQueueEntry();
                entry.Drawable = drawable;
                entry.SortOrder = 0;
                sortingEntries.Add(entry);
            }
        }

        public void Clear()
        {
            entries.Clear();
            sortingEntries.Clear();
        }

        public void Sort()
        {
            Array.Sort(sortingEntries.Elements, 0, sortingEntries.Count);
        }

        public void Draw(DrawingContext context, Material material)
        {
            int count;
            count = sortingEntries.Count;
            for (int i = 0; i < count; i++)
            {
                var drawable = sortingEntries.Elements[i].Drawable;
                drawable.Draw(context, drawable.Material);
            }
            count = entries.Count;
            for (int i = 0; i < count; i++)
                entries.Elements[i].Draw(context, null);
        }
    }

    struct DrawingQueueEntry : IComparable<DrawingQueueEntry>
    {
        public int SortOrder;
        public IDrawableObject Drawable;

        public int CompareTo(DrawingQueueEntry other)
        {
            return SortOrder - other.SortOrder;
        }
    }
}