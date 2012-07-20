namespace Nine.Graphics.ObjectModel
{    
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;

    /// <summary>
    /// A collection of all the pathes in a Surface.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SurfacePatchCollection : ReadOnlyCollection<SurfacePatch>
    {
        private Surface surface;

        internal SurfacePatchCollection(Surface surface, IList<SurfacePatch> patches)
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
        public SurfacePatch this[int x, int z]
        {
            get
            { 
                if (x < 0 || x >= surface.patchCountX ||
                    z < 0 || z >= surface.patchCountZ)
                {
                    return null;
                }
                return this[z * surface.patchCountX + x];
            }
        }
    }
}
