#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
#endif
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
            Cube cube = GraphicsResources<CubeInvert>.GetInstance(modelBatch.GraphicsDevice);
            modelBatch.DrawPrimitive(cube, Matrix.CreateScale(2), effect);
        }
#endif
    }
}
