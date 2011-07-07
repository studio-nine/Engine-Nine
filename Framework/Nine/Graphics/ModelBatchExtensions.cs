#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Contains extension method for <c>ModelBatch</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModelBatchExtensions
    {
        /// <summary>
        /// Draws a drawable surface using ModelBatch.
        /// </summary>
        public static void DrawSurface(this ModelBatch modelBatch, DrawableSurface surface, Effect effect)
        {
            foreach (var patch in surface.Patches)
            {
                DrawSurface(modelBatch, patch, effect);
            }
        }

        /// <summary>
        /// Draws a drawable surface using ModelBatch.
        /// </summary>
        public static void DrawSurface(this ModelBatch modelBatch, DrawableSurfacePatch surfacePatch, Effect effect)
        {
            modelBatch.DrawVertices(surfacePatch.VertexBuffer, surfacePatch.IndexBuffer, 0,
                                    surfacePatch.VertexCount, 0, surfacePatch.PrimitiveCount,
                                    surfacePatch.Transform, effect, null, null);
        }
    }
}
