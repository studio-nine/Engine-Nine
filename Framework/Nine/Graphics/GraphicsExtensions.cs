#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Contains extension methods related to graphics.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GraphicsExtensions
    {
        #region DrawSprite
        internal static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, Vector2 position, Rectangle? sourceRectangle, Color color, Effect effect)
        {
            SpriteBatch spriteBatch = PrepareSprite(graphics, effect);            
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, null, null, null, effect);
            spriteBatch.Draw(texture, position, sourceRectangle, color);
            spriteBatch.End();
        }

        internal static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, SamplerState samplerState, Color color, Effect effect)
        {
            SpriteBatch spriteBatch = PrepareSprite(graphics, effect);
                        

            Rectangle destination = new Rectangle(graphics.Viewport.X,
                                                  graphics.Viewport.Y,
                                                  graphics.Viewport.Width,
                                                  graphics.Viewport.Height);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque, samplerState, null, null, effect);
            spriteBatch.Draw(texture, destination, null, color);
            spriteBatch.End();
        }

        internal static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, SamplerState samplerState, BlendState blendState, Color color)
        {
            SpriteBatch spriteBatch = PrepareSprite(graphics, null);


            Rectangle destination = new Rectangle(graphics.Viewport.X,
                                                  graphics.Viewport.Y,
                                                  graphics.Viewport.Width,
                                                  graphics.Viewport.Height);

            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, samplerState, null, null);
            spriteBatch.Draw(texture, destination, null, color);
            spriteBatch.End();
        }

        internal static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, SamplerState samplerState, BlendState blendState, Color color, Effect effect)
        {
            SpriteBatch spriteBatch = PrepareSprite(graphics, effect);

            if (effect != null)
                effect.SetTexture(texture);

            Rectangle destination = new Rectangle(graphics.Viewport.X,
                                                  graphics.Viewport.Y,
                                                  graphics.Viewport.Width,
                                                  graphics.Viewport.Height);

            spriteBatch.Begin(SpriteSortMode.Immediate, blendState, samplerState, null, null, effect);
            spriteBatch.Draw(texture, destination, null, color);
            spriteBatch.End();
        }

        private static SpriteBatch PrepareSprite(GraphicsDevice graphics, Effect effect)
        {
            SetViewport(effect as IEffectMatrices, graphics.Viewport.Bounds);

            return GraphicsResources<SpriteBatch>.GetInstance(graphics);
        }

        public static void SetViewport(this IEffectMatrices effect, Rectangle viewport)
        {
            if (effect != null)
            {
                Matrix projection = Matrix.CreateOrthographicOffCenter(viewport.Left, viewport.Right, viewport.Bottom, viewport.Top, 0, 1);
                Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

                effect.World = Matrix.Identity;
                effect.View = Matrix.Identity;
                effect.Projection = halfPixelOffset * projection;
            }
        }
        #endregion
        
        #region RenderTargetStack
        static Dictionary<GraphicsDevice, Stack<RenderTarget2D>> renderTargetStacks;

        internal static RenderTarget2D PopRenderTarget(GraphicsDevice graphics)
        {
            if (renderTargetStacks == null)
                return null;

            Stack<RenderTarget2D> stack = null;
            if (!renderTargetStacks.TryGetValue(graphics, out stack))
                return null;

            RenderTarget2D renderTarget = stack.Pop();
            graphics.SetRenderTarget(renderTarget);
            return renderTarget;
        }

        internal static void PushRenderTarget(RenderTarget2D renderTarget)
        {
            if (renderTarget != null)
            {
                if (renderTargetStacks == null)
                    renderTargetStacks = new Dictionary<GraphicsDevice, Stack<RenderTarget2D>>();

                Stack<RenderTarget2D> stack = null;
                if (!renderTargetStacks.TryGetValue(renderTarget.GraphicsDevice, out stack))
                    renderTargetStacks.Add(renderTarget.GraphicsDevice, stack = new Stack<RenderTarget2D>());

                // Get old render target
                RenderTarget2D previous = null;

                RenderTargetBinding[] bindings = renderTarget.GraphicsDevice.GetRenderTargets();

                if (bindings.Length > 0)
                    previous = bindings[0].RenderTarget as RenderTarget2D;

                stack.Push(previous);

                renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);
            }
        }

        public static void Begin(this RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException();

            PushRenderTarget(renderTarget);
        }

        public static Texture2D End(this RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException();

            PopRenderTarget(renderTarget.GraphicsDevice);
            return renderTarget;
        }
        #endregion
    }
}
