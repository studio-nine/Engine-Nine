namespace Nine
{
    using System;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;


    /// <summary>
    /// Interface for a surface with Z axis facing up.
    /// </summary>
    public interface ISurface
    {
        /// <summary>
        /// Gets the height and normal of the specified position on the surface.
        /// </summary>
        /// <returns>
        /// Returns true if the point resides in the boundary of the surface.
        /// </returns>
        bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal);
    }

    /// <summary>
    /// A collection of surfaces that can be queried together.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SurfaceCollection : Collection<ISurface>, ISurface
    {
        /// <summary>
        /// Gets or sets the height of the object used to query surface height and normal.
        /// </summary>
        public float ObjectHeight { get; set; }

        /// <summary>
        /// Gets the height and normal of the specified position on the surface.
        /// </summary>
        /// <returns>
        /// Returns true if the point resides in the boundary of the surface.
        /// </returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            // TODO: Include object height

            Vector3 v = Vector3.Up;
            float h = 0;
            float min = float.MaxValue;
            bool result = false;

            height = 0;
            normal = Vector3.Zero;

            for (int i = 0; i < Count; ++i)
            {
                var surface = this[i];
                if (surface != null &&
                    surface.TryGetHeightAndNormal(position, out h, out v))
                {
                    if (Math.Abs(position.Y - h) < min)
                    {
                        height = h;
                        normal = v;

                        result = true;
                    }
                }
            }

            return result;
        }
    }
}