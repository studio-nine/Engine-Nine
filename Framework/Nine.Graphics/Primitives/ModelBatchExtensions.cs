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
using Nine.Graphics.Effects;
#endregion

namespace Nine.Graphics.Primitives
{
    /// <summary>
    /// Contains extension method for <c>ModelBatch</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModelBatchExtensions
    {
        public static void DrawPrimitive(this ModelBatch modelBatch, ICustomPrimitive primitive, Matrix world, Effect effect)
        {
            modelBatch.DrawVertices(primitive.VertexBuffer, primitive.IndexBuffer, 0,
                                    primitive.VertexBuffer.VertexCount, 0,
                                    primitive.IndexBuffer.IndexCount / 3, world,
                                    effect, null, null, primitive.BoundingSphere);
        }

#if !WINDOWS_PHONE
        public static void DrawSkyBox(this ModelBatch modelBatch, TextureCube skyBoxTexture)
        {
            SkyBoxEffect effect = GraphicsResources<SkyBoxEffect>.GetInstance(modelBatch.GraphicsDevice);
            effect.Texture = skyBoxTexture;

            // FIXME: When models batch is not in immediate mode, InvertWindingOrder would have no effect.
            Cube cube = GraphicsResources<Cube>.GetInstance(modelBatch.GraphicsDevice);
            cube.InvertWindingOrder = true;
            modelBatch.DrawPrimitive(cube, Matrix.CreateScale(2), effect);
            cube.InvertWindingOrder = false;
        }
#endif
    }
}
