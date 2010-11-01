#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine
{
    /// <summary>
    /// Interface for a surface with Z axis facing up.
    /// </summary>
    public interface ISurface
    {
        /// <summary>
        /// Gets the height and normal of the specifed position on the surface.
        /// </summary>
        /// <returns>
        /// Returns true if the point resides in the boundary of the surface.
        /// </returns>
        bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal);
    }

    /// <summary>
    /// A simple flat surface that implements <see cref="Nine.ISurface"/>.
    /// </summary>
    public class FlatSurface : ISurface
    {
        /// <summary>
        /// Gets or sets the height of this flat surface.
        /// </summary>
        public float Height { get; set; }

        /// <summary>
        /// Gets the height and normal of the specifed position on the surface.
        /// </summary>
        /// <returns>
        /// Returns true if the point resides in the boundary of the surface.
        /// </returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            height = Height;
            normal = Vector3.UnitZ;

            return true;
        }
    }

    /// <summary>
    /// A collection of surfaces that can be queried together.
    /// </summary>
    public class SurfaceCollection : Collection<ISurface>, ISurface
    {
        /// <summary>
        /// Gets or sets the height of the object used to query surface height and normal.
        /// </summary>
        public float ObjectHeight { get; set; }

        /// <summary>
        /// Gets the height and normal of the specifed position on the surface.
        /// </summary>
        /// <returns>
        /// Returns true if the point resides in the boundary of the surface.
        /// </returns>
        public bool TryGetHeightAndNormal(Vector3 position, out float height, out Vector3 normal)
        {
            // TODO: Include object height

            Vector3 v = Vector3.UnitZ;
            float h = 0;
            float min = float.MaxValue;
            bool result = false;

            height = 0;
            normal = Vector3.Zero;

            foreach (ISurface surface in this)
            {
                if (surface != null &&
                    surface.TryGetHeightAndNormal(position, out h, out v))
                {
                    if (Math.Abs(position.Z - h) < min)
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