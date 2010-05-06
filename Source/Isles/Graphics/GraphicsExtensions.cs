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
using System.ComponentModel;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Isles.Graphics
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GraphicsExtensions
    {
        #region DrawSprite
        static SpriteBatch spriteBatch;

        public static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, Vector2 position, Rectangle? source, Color color, Effect effect)
        {
            Rectangle rc = source.HasValue ? source.Value : texture.Bounds;

            rc.X = rc.Y = 0;

            rc.X += (int)position.X;
            rc.Y += (int)position.Y;

            DrawSprite(graphics, texture, rc, source, color, effect);
        }

        public static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, Rectangle? destination, Rectangle? source, Color color, Effect effect)
        {
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphics);

            // Setup matrix parameters for effects with a vertex shader
            if (effect != null && effect is IEffectMatrices)
            {
                IEffectMatrices matrices = effect as IEffectMatrices;

                Matrix projection = Matrix.CreateOrthographicOffCenter(0, graphics.Viewport.Width, graphics.Viewport.Height, 0, 0, 1);
                Matrix halfPixelOffset = Matrix.CreateTranslation(-0.5f, -0.5f, 0);

                matrices.World = Matrix.Identity;
                matrices.View = Matrix.Identity;
                matrices.Projection = halfPixelOffset * projection;
            }
                        
            if (destination == null)
            {
                destination = new Rectangle(graphics.Viewport.X,
                                            graphics.Viewport.Y,
                                            graphics.Viewport.Width,
                                            graphics.Viewport.Height);
            }

            spriteBatch.Begin(SpriteSortMode.Immediate, null, null, null, null, effect);
            
            spriteBatch.Draw(texture, destination.Value, source, color);

            spriteBatch.End();
        }
        #endregion
        
        #region RenderTargetStack
        static Stack<RenderTarget2D> renderTargetStack;

        public static bool Begin(this RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException();

            if (renderTargetStack == null)
                renderTargetStack = new Stack<RenderTarget2D>();
                        
            // Get old render target
            RenderTarget2D previous = null;

            RenderTargetBinding[] bindings = renderTarget.GraphicsDevice.GetRenderTargets();

            if (bindings.Length > 0)
                previous = bindings[0].RenderTarget as RenderTarget2D;

            renderTargetStack.Push(previous);

            renderTarget.GraphicsDevice.SetRenderTarget(renderTarget);

            return true;
        }

        public static Texture2D End(this RenderTarget2D renderTarget)
        {
            if (renderTarget == null)
                throw new ArgumentNullException();

            renderTarget.GraphicsDevice.SetRenderTarget(renderTargetStack.Pop());

            return renderTarget;
        }
        #endregion
    }
}
