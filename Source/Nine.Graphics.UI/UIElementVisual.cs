namespace Nine.Graphics.UI
{
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Internal;
    using Nine.Graphics.UI.Media;
    using System;
    
    public abstract partial class UIElement
    {
        #region Methods

        protected virtual BoundingRectangle? GetClippingRect(Vector2 finalSize)
        {
            if (!this.isClippingRequired)
                return null;

            var max = new MinMax(this);
            Vector2 renderSize = this.RenderSize;

            float maxWidth = float.IsPositiveInfinity(max.MaxWidth) ? renderSize.X : max.MaxWidth;
            float maxHeight = float.IsPositiveInfinity(max.MaxHeight) ? renderSize.Y : max.MaxHeight;

            bool isClippingRequiredDueToMaxSize = maxWidth.IsLessThan(renderSize.X) ||
                                                  maxHeight.IsLessThan(renderSize.Y);

            renderSize.X = Math.Min(renderSize.X, max.MaxWidth);
            renderSize.Y = Math.Min(renderSize.Y, max.MaxHeight);

            Thickness margin = this.Margin;
            float horizontalMargins = margin.Left + margin.Right;
            float verticalMargins = margin.Top + margin.Bottom;

            var clientSize = new Vector2(
                (finalSize.X - horizontalMargins).EnsurePositive(),
                (finalSize.Y - verticalMargins).EnsurePositive());

            bool isClippingRequiredDueToClientSize = clientSize.X.IsLessThan(renderSize.X) ||
                                                     clientSize.Y.IsLessThan(renderSize.Y);

            if (isClippingRequiredDueToMaxSize && !isClippingRequiredDueToClientSize)
            {
                return new BoundingRectangle(0f, 0f, maxWidth, maxHeight);
            }

            if (!isClippingRequiredDueToClientSize)
            {
                return BoundingRectangle.Empty;
            }

            Vector2 offset = this.ComputeAlignmentOffset(clientSize, renderSize);
            var clipRect = new BoundingRectangle(-offset.X, -offset.Y, clientSize.X, clientSize.Y);

            if (isClippingRequiredDueToMaxSize)
            {
            }

            return clipRect;
        }

        #endregion 

        #region Draw

        internal bool Visible
        {
            get { return this.Visibility != Visibility.Visible; }
        }

        public void Draw(Renderer.Renderer renderer)
        {
            // TODO: Fix Clipping
            renderer.GraphicsDevice.RasterizerState = needsClipping ?
                BaseWindow.WithClipping : BaseWindow.WithoutClipping;
            if (clip.HasValue)
            {
                var c = clip.Value;
                c.X += AbsoluteVisualOffset.X;
                c.Y += AbsoluteVisualOffset.Y;
                renderer.GraphicsDevice.ScissorRectangle = c;
            }

            // Should I move this?
            if (Background != null)
            {
                renderer.Draw(AbsoluteRenderTransform, Background);
            }

            OnDraw(renderer);
        }

        /// <summary>
        /// Called when element is drawing.
        /// </summary>
        /// <param name="renderer"></param>
        protected virtual void OnDraw(Renderer.Renderer renderer) { }


        /// <summary>
        /// Draws the RenderTransforms of every element.
        /// This is useful when debugging.
        /// </summary>
        /// <param name="renderer"></param>
        /// <param name="color"></param>
        internal void DrawBounds(Renderer.Renderer renderer, Color color)
        {
            renderer.Draw(AbsoluteRenderTransform, color);

            var container = this as IContainer;
            if (container != null)
            {
                foreach (var item in container.Children)
                {
                    var uiElement = item as UIElement;
                    if (uiElement != null)
                    {
                        uiElement.DrawBounds(renderer, color);
                    }
                }
            }
        }

        #endregion
    }
}
