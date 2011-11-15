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
using Microsoft.Xna.Framework.Input;
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
        internal static void DrawFullscreenQuad(this GraphicsDevice graphics, Texture2D texture, SamplerState samplerState, Color color, Effect effect)
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

        internal static void DrawFullscreenQuad(this GraphicsDevice graphics, Texture2D texture, SamplerState samplerState, BlendState blendState, Color color, Effect effect)
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

        internal static void SetViewport(this IEffectMatrices effect, Rectangle viewport)
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
#if SILVERLIGHT
                var graphicsDevice = System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice;
#else
                var graphicsDevice = renderTarget.GraphicsDevice;
#endif
                if (renderTargetStacks == null)
                    renderTargetStacks = new Dictionary<GraphicsDevice, Stack<RenderTarget2D>>();

                Stack<RenderTarget2D> stack = null;
                if (!renderTargetStacks.TryGetValue(graphicsDevice, out stack))
                    renderTargetStacks.Add(graphicsDevice, stack = new Stack<RenderTarget2D>());

                // Get old render target
                RenderTarget2D previous = null;

                RenderTargetBinding[] bindings = graphicsDevice.GetRenderTargets();

                if (bindings.Length > 0)
                    previous = bindings[0].RenderTarget as RenderTarget2D;

                stack.Push(previous);

                graphicsDevice.SetRenderTarget(renderTarget);
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
#if SILVERLIGHT
            PopRenderTarget(System.Windows.Graphics.GraphicsDeviceManager.Current.GraphicsDevice);
#else
            PopRenderTarget(renderTarget.GraphicsDevice);
#endif
            return renderTarget;
        }
        #endregion

        #region EnablePerfHudProfiling
#if WINDOWS
        /// <summary>
        /// Enables the profiling using nVidia PerfHud.
        /// </summary>
        public static void EnablePerfHudProfiling(this GraphicsDeviceManager graphicsDeviceManager)
        {
            graphicsDeviceManager.PreparingDeviceSettings += (sender, e) =>
            {
                foreach (GraphicsAdapter currentAdapter in GraphicsAdapter.Adapters)
                {
                    if (currentAdapter.Description.Contains("PerfHUD"))
                    {
                        e.GraphicsDeviceInformation.Adapter = currentAdapter;
                        GraphicsAdapter.UseReferenceDevice = true;
                        break;
                    }
                }
            };
        }
#endif
        #endregion
    }

    /// <summary>
    /// Contains extension method for Viewport.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class ViewportExtensions
    {
        /// <summary>
        /// Creates a picking ray from screen position.
        /// </summary>
        public static Ray CreatePickRay(this Viewport viewport, int x, int y, Matrix view, Matrix projection)
        {
            // create 2 positions in screen space using the cursor position. 0 is as
            // close as possible to the camera, 1 is as far away as possible.
            Vector3 nearSource = new Vector3(x, y, 0f);
            Vector3 farSource = new Vector3(x, y, 1f);

            // use Viewport.Unproject to tell what those two screen space positions
            // would be in world space. we'll need the projection matrix and view
            // matrix, which we have saved as member variables. We also need a world
            // matrix, which can just be identity.
            Vector3 nearPoint = viewport.Unproject(nearSource,
                projection, view, Matrix.Identity);

            Vector3 farPoint = viewport.Unproject(farSource,
                projection, view, Matrix.Identity);

            // find the direction vector that goes from the nearPoint to the farPoint
            // and normalize it....
            Vector3 direction = farPoint - nearPoint;
            direction.Normalize();

            // and then create a new ray using nearPoint as the source.
            return new Ray(nearPoint, direction);
        }

        /// <summary>
        /// Creates a picking frustum from screen rectangle.
        /// </summary>
        public static BoundingFrustum CreatePickFrustum(this Viewport viewport, Point point1, Point point2, Matrix view, Matrix projection)
        {
            Rectangle rectangle = new Rectangle();

            rectangle.X = Math.Min(point1.X, point2.X);
            rectangle.Y = Math.Min(point1.Y, point2.Y);
            rectangle.Width = Math.Abs(point1.X - point2.X);
            rectangle.Height = Math.Abs(point1.Y - point2.Y);

            return CreatePickFrustum(viewport, rectangle, view, projection);
        }

        /// <summary>
        /// Creates a picking frustum from screen rectangle.
        /// </summary>
        public static BoundingFrustum CreatePickFrustum(this Viewport viewport, Rectangle rectangle, Matrix view, Matrix projection)
        {
            rectangle.X -= viewport.X;
            rectangle.Y -= viewport.Y;

            Matrix innerProject;

            float left = (float)(2 * rectangle.Left - viewport.Width) / viewport.Width;
            float right = (float)(2 * rectangle.Right - viewport.Width) / viewport.Width;
            float bottom = -(float)(2 * rectangle.Top - viewport.Height) / viewport.Height;
            float top = -(float)(2 * rectangle.Bottom - viewport.Height) / viewport.Height;

            float farClip = Math.Abs(projection.M43 / (Math.Abs(projection.M33) - 1));
            float nearClip = Math.Abs(projection.M43 / projection.M33);

            Matrix projectionInverse = Matrix.Invert(projection);

            Vector3 max = Vector3.Transform(new Vector3(1, 1, 0), projectionInverse) * nearClip;
            Vector3 min = Vector3.Transform(new Vector3(-1, -1, 0), projectionInverse) * -nearClip;

            Matrix.CreatePerspectiveOffCenter(
                left * min.X, right * max.X, bottom * min.Y, top * max.Y, nearClip, farClip, out innerProject);

            return new BoundingFrustum(view * innerProject);
        }

        /// <summary>
        /// Gets the picked position in world space from current mouse coordinates.
        /// </summary>
        public static bool TryPick(this IPickable pickable, Viewport viewport, Matrix view, Matrix projection, out Vector3 position)
        {
            MouseState mouse = Mouse.GetState();
            return TryPick(pickable, viewport, mouse.X, mouse.Y, view, projection, out position);
        }

        /// <summary>
        /// Gets the picked position in world space from screen coordinates.
        /// </summary>
        public static bool TryPick(this IPickable pickable, Viewport viewport, int x, int y, Matrix view, Matrix projection, out Vector3 position)
        {
            Ray pickRay = viewport.CreatePickRay(x, y, view, projection);
            float? distance = pickable.Intersects(pickRay);
            if (distance == null)
            {
                position = Vector3.Zero;
                return false;
            }
            position = pickRay.Position + pickRay.Direction * distance.Value;
            return true;
        }
    }
}
