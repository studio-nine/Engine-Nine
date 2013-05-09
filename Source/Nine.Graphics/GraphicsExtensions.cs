namespace Nine.Graphics
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;

    /// <summary>
    /// Contains extension methods related to graphics.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class GraphicsExtensions
    {
        #region DrawSprite
        public static void Draw(this SpriteBatch spriteBatch, Texture2D texture, ref Matrix transform, Rectangle? sourceRectangle, Color color, SpriteEffects effects, float layerDepth)
        {
            Vector2 position, scale;
            float rotation;
            MatrixHelper.Decompose(ref transform, out scale, out rotation, out position);

            spriteBatch.Draw(texture, position, sourceRectangle, color, rotation, Vector2.Zero, scale, effects, layerDepth);
        }

        public static void Draw(this SpriteBatch spriteBatch, SpriteFont font, string text, ref Matrix transform, Color color, SpriteEffects effects, float layerDepth)
        {
            Vector2 position, scale;
            float rotation;
            MatrixHelper.Decompose(ref transform, out scale, out rotation, out position);

            spriteBatch.DrawString(font, text, position, color, rotation, Vector2.Zero, scale, effects, layerDepth);
        }

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

        internal static void PopRenderTarget(GraphicsDevice graphics)
        {
            Stack<RenderTarget2D> stack = null;
            if (renderTargetStacks.TryGetValue(graphics, out stack))
            {
                stack.Pop();
                graphics.SetRenderTarget(stack.Peek());
            }
        }

        internal static void PushRenderTarget(GraphicsDevice graphicsDevice, RenderTarget2D renderTarget)
        {
            if (renderTargetStacks == null)
                renderTargetStacks = new Dictionary<GraphicsDevice, Stack<RenderTarget2D>>();

            Stack<RenderTarget2D> stack = null;
            if (!renderTargetStacks.TryGetValue(graphicsDevice, out stack))
            {
                renderTargetStacks.Add(graphicsDevice, stack = new Stack<RenderTarget2D>());
                stack.Push(null);
            }

            stack.Push(renderTarget);
            if (renderTarget != null)
                graphicsDevice.SetRenderTarget(renderTarget);
        }

        public static void Begin(this RenderTarget2D renderTarget)
        {
            PushRenderTarget(renderTarget.GraphicsDevice, renderTarget);
        }

        public static Texture2D End(this RenderTarget2D renderTarget)
        {
            PopRenderTarget(renderTarget.GraphicsDevice);
            return renderTarget;
        }
        #endregion

        #region EnablePerfHudProfiling
#if WINDOWS
        /// <summary>
        /// Enables profiling using nVidia PerfHud.
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
        
        #region CreateMockedGraphicsDevice
#if WINDOWS
        /// <summary>
        /// Creates a new hidden graphics device.
        /// </summary>
        public static GraphicsDevice CreateHiddenGraphicsDevice(GraphicsProfile profile)
        {
            return CreateHiddenGraphicsDevice(1, 1, profile);
        }

        /// <summary>
        /// Creates a new hidden graphics device.
        /// </summary>
        public static GraphicsDevice CreateHiddenGraphicsDevice(int width, int height, GraphicsProfile profile)
        {
            // Create graphics device
            System.Windows.Forms.Form dummy = new System.Windows.Forms.Form();

            PresentationParameters parameters = new PresentationParameters();
            parameters.BackBufferWidth = 1;
            parameters.BackBufferHeight = 1;
            parameters.DeviceWindowHandle = dummy.Handle;

            GraphicsAdapter.UseNullDevice = true;
            return new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, parameters);
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

        /// <summary>
        /// Creates a BoundingRectangle from Viewport
        /// </summary>
        public static BoundingRectangle ToBoundingRectangle(this Viewport viewport)
        {
            return new BoundingRectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        }
    }
}
