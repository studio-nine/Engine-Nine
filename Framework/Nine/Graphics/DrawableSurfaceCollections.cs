#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{    
    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DrawableSurfaceCollections : Collection<Effect>
    {
        private DrawableSurface surface;

        internal DrawableSurfaceCollections(DrawableSurface surface)
        {
            this.surface = surface;
        }

        protected override void InsertItem(int index, Effect item)
        {
            foreach (DrawableSurfacePatch patch in surface.Patches)
            {
                patch.Effects.Insert(index, item);
            }

            base.InsertItem(index, item);
        }

        protected override void RemoveItem(int index)
        {
            foreach (DrawableSurfacePatch patch in surface.Patches)
            {
                patch.Effects.Remove(this[index]);
            }

            base.RemoveItem(index);
        }

        protected override void SetItem(int index, Effect item)
        {
            throw new InvalidOperationException();
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class DrawableSurfacePatchCollection : ReadOnlyCollection<DrawableSurfacePatch>
    {
        private DrawableSurface surface;

        internal DrawableSurfacePatchCollection(DrawableSurface surface, IList<DrawableSurfacePatch> patches)
            : base(patches)
        {
            this.surface = surface;
        }

        public DrawableSurfacePatch this[int x, int y]
        {
            get
            { 
                if (x < 0 || x >= surface.PatchCountX)
                    throw new ArgumentOutOfRangeException("x");

                if (y < 0 || y >= surface.PatchCountY)
                    throw new ArgumentOutOfRangeException("y");

                return this[y * surface.PatchCountX + x];
            }
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrawableSurfaceTriangleCollection : IEnumerable<DrawableSurfaceTriangle>
    {
        internal DrawableSurface Surface;

        public DrawableSurfaceTriangle this[float x, float y]
        {
            get { return Surface.GetTriangle(x, y); }
        }

        public IEnumerator<DrawableSurfaceTriangle> GetEnumerator()
        {
            foreach (DrawableSurfacePatch patch in Surface.Patches)
            {
                foreach (DrawableSurfacePatchPart part in patch.PatchParts)
                {
                    foreach (DrawableSurfaceTriangle triangle in part.Triangles)
                        yield return triangle;
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
