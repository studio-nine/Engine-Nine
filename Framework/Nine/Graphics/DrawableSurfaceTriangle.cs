#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// The smallest triangle that makes up a drawable surface.
    /// </summary>
    public class DrawableSurfaceTriangle
    {
        internal byte Index;
        internal DrawableSurfacePatchPart Part;

        /// <summary>
        /// Gets the visiblity of this triangle.
        /// Call DrawableSurface.Invalidate to flush the changes.
        /// </summary>
        public bool Visible 
        {
            get { return (((Part.Mask >> Index) & 0x01) == 1); }
            set 
            {
                if (value)
                    Part.Mask = (byte)(Part.Mask | (0x01 << Index));
                else
                    Part.Mask = (byte)(Part.Mask & ~(0x01 << Index)); 
            }
        }

        /// <summary>
        /// Gets the positions of three vertices of this triangle.
        /// </summary>
        public Triangle Triangle
        {
            get
            {
                int i = 0;
                Vector3[] result = new Vector3[3];

                foreach (Point pt in Part.GetIndicesForTriangle(Index))
                {
                    result[i++] = Part.Patch.GetPosition(Part, pt);
                }

                return new Triangle(result[0], result[1], result[2]);
            }
        }

        /// <summary>
        /// Gets or sets any user data.
        /// </summary>
        public object Tag;
    }
}
