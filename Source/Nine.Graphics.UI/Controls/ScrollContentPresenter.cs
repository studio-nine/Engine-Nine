#region License
/* The MIT License
 *
 * Copyright (c) 2013 Engine Nine
 * Copyright (c) 2011 Red Badger Consulting
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 * 
 * The above copyright notice and this permission notice shall be included in
 * all copies or substantial portions of the Software.
 * 
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
 * THE SOFTWARE.
*/
#endregion

namespace Nine.Graphics.UI.Controls
{
    using System;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Controls.Primitives;
    using Nine.Graphics.UI.Internal;
    using Nine.Graphics.UI.Internal.Controls;

    /// <summary>
    /// Displays the content of a <see cref="Nine.Graphics.UI.Controls.ScrollViewer">ScrollViewer</see> control.
    /// </summary>
    public class ScrollContentPresenter : ContentControl, IScrollInfo
    {
        #region Properties

        /// <summary>
        /// Gets or sets, if you can scroll Horizontally.
        /// </summary>
        public bool CanHorizontallyScroll
        {
            get { return this.scrollData.CanHorizontallyScroll; }
            set { this.scrollData.CanHorizontallyScroll = value; }
        }

        /// <summary>
        /// Gets or sets, if you can scroll Vertically.
        /// </summary>
        public bool CanVerticallyScroll
        {
            get { return this.scrollData.CanVerticallyScroll; }
            set { this.scrollData.CanVerticallyScroll = value; }
        }

        /// <summary>
        /// Gets how much there is to display.
        /// </summary>
        public Vector2 Extent
        {
            get { return this.scrollData.Extent; }
        }

        /// <summary>
        /// Gets the current scroll offset.
        /// </summary>
        public Vector2 Offset
        {
            get { return this.scrollData.Offset; }
        }

        /// <summary>
        /// Gets the current viewport.
        /// </summary>
        public Vector2 Viewport
        {
            get { return this.scrollData.Viewport; }
        }

        #endregion

        private bool isClippingRequired;
        private ScrollData scrollData;

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ScrollContentPresenter()
        {
            this.scrollData.CanHorizontallyScroll = true;
            this.scrollData.CanVerticallyScroll = true;
        }

        #region Methods

        /// <summary>
        /// Sets the Horizontal Scroll Offset.
        /// </summary>
        /// <param name="offset">Scroll Offset</param>
        public void SetHorizontalOffset(float offset)
        {
            if (!this.CanHorizontallyScroll)
                return;

            if (float.IsNaN(offset))
                throw new ArgumentOutOfRangeException("offset");

            offset = Math.Max(0f, offset);

            if (this.scrollData.Offset.X.IsDifferentFrom(offset))
            {
                this.scrollData.Offset.X = offset;
                this.InvalidateArrange();
            }
        }

        /// <summary>
        /// Sets the Vertical Scroll Offset.
        /// </summary>
        /// <param name="offset">Scroll Offset</param>
        public void SetVerticalOffset(float offset)
        {
            if (!this.CanVerticallyScroll)
                return;

            if (float.IsNaN(offset))
                throw new ArgumentOutOfRangeException("offset");

            offset = Math.Max(0f, offset);

            if (this.scrollData.Offset.Y.IsDifferentFrom(offset))
            {
                this.scrollData.Offset.Y = offset;
                this.InvalidateArrange();
            }
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (Visible == Visibility.Collapsed) return Vector2.Zero;

            UIElement content = this.Content;
            this.UpdateScrollData(finalSize, this.scrollData.Extent);

            if (content != null)
            {
                var finalRect = new BoundingRectangle(
                    -this.scrollData.Offset.X, 
                    -this.scrollData.Offset.Y, 
                    content.DesiredSize.X, 
                    content.DesiredSize.Y);

                this.isClippingRequired = finalSize.X.IsLessThan(finalRect.Width) ||
                                          finalSize.Y.IsLessThan(finalRect.Height);

                finalRect.Width = Math.Max(finalRect.Width, finalSize.X);
                finalRect.Height = Math.Max(finalRect.Height, finalSize.Y);

                content.Arrange(finalRect);
            }

            return finalSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (Visible == Visibility.Collapsed) return Vector2.Zero;

            UIElement content = this.Content;
            var desiredSize = new Vector2();
            var extent = new Vector2();

            if (content != null)
            {
                Vector2 availableSizeForContent = availableSize;
                if (this.scrollData.CanHorizontallyScroll)
                    availableSizeForContent.X = float.PositiveInfinity;

                if (this.scrollData.CanVerticallyScroll)
                    availableSizeForContent.Y = float.PositiveInfinity;

                content.Measure(availableSizeForContent);
                desiredSize = content.DesiredSize;

                extent = content.DesiredSize;
            }

            this.UpdateScrollData(availableSize, extent);
            desiredSize.X = Math.Min(availableSize.X, desiredSize.X);
            desiredSize.Y = Math.Min(availableSize.Y, desiredSize.Y);

            return desiredSize;
        }

        protected override BoundingRectangle? GetClippingRect(Vector2 finalSize)
        {
            if (isClippingRequired)
                return new BoundingRectangle(this.RenderSize.X, this.RenderSize.Y);
            return null;
        }

        private void UpdateScrollData(Vector2 viewport, Vector2 extent)
        {
            this.scrollData.Viewport = viewport;
            this.scrollData.Extent = extent;

            float x = MathHelper.Clamp(this.scrollData.Offset.X,
                0f, this.scrollData.Extent.X - this.scrollData.Viewport.X);
            float y = MathHelper.Clamp(this.scrollData.Offset.Y,
                0f, this.scrollData.Extent.Y - this.scrollData.Viewport.Y);

            this.scrollData.Offset = new Vector2(x, y);
        }

        #endregion
    }
}
