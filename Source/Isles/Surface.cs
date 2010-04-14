#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles
{
    /// <summary>
    /// Represents a peace of surface with Z axis facing up.
    /// </summary>
    public interface ISurface
    {
        /// <summary>
        /// Returns true if the point is on the surface.
        /// </summary>
        bool TryGetHeightAndNormal(Vector3 position, ref float height, ref Vector3 normal);
    }


    public class FlatSurface : ISurface
    {
        public float Height { get; set; }

        public bool TryGetHeightAndNormal(Vector3 position, ref float height, ref Vector3 normal)
        {
            height = Height;
            normal = Vector3.UnitZ;

            return true;
        }
    }


    public class SurfaceCollection : Collection<ISurface>, ISurface
    {
        public float ObjectHeight { get; set; }

        public bool TryGetHeightAndNormal(Vector3 position, ref float height, ref Vector3 normal)
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
                    surface.TryGetHeightAndNormal(position, ref h, ref v))
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