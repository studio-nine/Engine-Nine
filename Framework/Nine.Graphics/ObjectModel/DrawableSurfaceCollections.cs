#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;

#endregion

namespace Nine.Graphics.ObjectModel
{    
    /// <summary>
    /// A collection of all the pathes in a Surface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrawableSurfacePatchCollection : ReadOnlyCollection<DrawableSurfacePatch>
    {
        private DrawableSurface surface;

        internal DrawableSurfacePatchCollection(DrawableSurface surface, IList<DrawableSurfacePatch> patches)
            : base(patches)
        {
            this.surface = surface;
        }

        /// <summary>
        /// Gets the patch for the specified index.
        /// </summary>
        /// <returns>
        /// Returns null if the input is outside the bounds.
        /// </returns>
        public DrawableSurfacePatch this[int x, int y]
        {
            get
            { 
                if (x < 0 || x >= surface.PatchCountX ||
                    y < 0 || y >= surface.PatchCountY)
                {
                    return null;
                }
                return this[y * surface.PatchCountX + x];
            }
        }
    }
}
