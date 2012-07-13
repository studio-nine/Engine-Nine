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

        public void Sort()
        {
            Array.Sort(Elements, 0, Count);
        }
    }

    class DrawingQueueEntry : IComparable<DrawingQueueEntry>
    {
        public IDrawableObject Drawable;
        public Material Material;

        public int CompareTo(DrawingQueueEntry other)
        {
            return Material.SortOrder - other.Material.SortOrder;
        }
    }
}