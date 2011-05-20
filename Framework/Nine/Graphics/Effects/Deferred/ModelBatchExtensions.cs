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

namespace Nine.Graphics.Effects.Deferred
{
    /// <summary>
    /// Contains extension method for <c>ModelBatch</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ModelBatchExtensions
    {
        /// <summary>
        /// Begins the rendering of lights for DeferredEffect.
        /// </summary>
        public static void BeginLights(this ModelBatch modelBatch, Matrix view, Matrix projection)
        {
            modelBatch.Begin(ModelSortMode.Effect, view, projection, GraphicsBuffer.LightBlendState, null, DepthStencilState.None, null);
        }

        /// <summary>
        /// Draws a light instance for DeferredEffect.
        /// </summary>
        public static void DrawLight(this ModelBatch modelBatch, Texture2D normalBuffer, Texture2D depthBuffer, IDeferredLight light)
        {
            if (modelBatch.BlendState != GraphicsBuffer.LightBlendState)
                throw new InvalidOperationException(Strings.InvalidDeferredLightBlendState);

            if (modelBatch.DepthStencilState != DepthStencilState.None)
                throw new InvalidOperationException(Strings.InvalidDeferredLightDepthStencilState);

            IEffectTexture texture = light.Effect as IEffectTexture;
            if (texture != null)
            {
                texture.SetTexture(TextureNames.NormalMap, normalBuffer);
                texture.SetTexture(TextureNames.DepthMap, depthBuffer);
            }

            modelBatch.DrawVertices(light.VertexBuffer, light.IndexBuffer, 0, 
                                    light.VertexBuffer.VertexCount, 0,
                                    light.IndexBuffer.IndexCount / 3, Matrix.Identity, 
                                    light.Effect, null, null);
        }

        /// <summary>
        /// Ends the rendering of lights for DeferredEffect.
        /// </summary>
        public static void EndLights(this ModelBatch modelBatch)
        {
            modelBatch.End();
        }
    }
}
