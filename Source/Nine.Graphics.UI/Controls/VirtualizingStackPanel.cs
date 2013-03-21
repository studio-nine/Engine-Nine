#region License
/* The MIT License
 *
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
    /*
    public class VirtualizingStackPanel : StackPanel, IScrollInfo
    {
        private VirtualizingElementCollection children;

        private ScrollData scrollData;

        public bool CanHorizontallyScroll
        {
            get
            {
                return this.scrollData.CanHorizontallyScroll;
            }

            set
            {
                this.scrollData.CanHorizontallyScroll = value;
            }
        }

        public bool CanVerticallyScroll
        {
            get
            {
                return this.scrollData.CanVerticallyScroll;
            }

            set
            {
                this.scrollData.CanVerticallyScroll = value;
            }
        }

        public Size Extent
        {
            get
            {
                return this.scrollData.Extent;
            }
        }

        public Vector Offset
        {
            get
            {
                return this.scrollData.Offset;
            }
        }

        public Size Viewport
        {
            get
            {
                return this.scrollData.Viewport;
            }
        }

        public override IEnumerable<UIElement> GetVisualChildren()
        {
            return this.children.RealizedElements;
        }

        public void SetHorizontalOffset(float offset)
        {
        }

        public void SetVerticalOffset(float offset)
        {
        }

        protected override IList<UIElement> CreateChildrenCollection()
        {
            this.children = new VirtualizingElementCollection(this);
            return this.children;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            var viewportUsed = new Size();

            var availableSizeForContent = availableSize;
            if (this.Orientation == Orientation.Horizontal || this.scrollData.CanHorizontallyScroll)
            {
                availableSizeForContent.X = float.PositiveInfinity;
            }

            if (this.Orientation == Orientation.Vertical || this.scrollData.CanVerticallyScroll)
            {
                availableSizeForContent.Y = float.PositiveInfinity;
            }

            // TODO: work out what the first visible child is from the scrolldata offset
            int firstVisibleChild = 0;
            bool isLastVisibleChild = false;
            foreach (var child in this.children.GetCursor(firstVisibleChild))
            {
                if (isLastVisibleChild)
                {
                    break;
                }

                if (child != null)
                {
                    child.Measure(availableSizeForContent);
                    var childDesiredSize = child.DesiredSize;
                    if (this.Orientation == Orientation.Horizontal)
                    {
                        isLastVisibleChild = viewportUsed.X + childDesiredSize.X > availableSize.X;
                        viewportUsed.X += childDesiredSize.X;
                        viewportUsed.Y = Math.Max(viewportUsed.Y, childDesiredSize.Y);
                    }
                    else
                    {
                        isLastVisibleChild = viewportUsed.Y + childDesiredSize.Y > availableSize.Y;
                        viewportUsed.X = Math.Max(viewportUsed.X, childDesiredSize.X);
                        viewportUsed.Y += childDesiredSize.Y;
                    }
                }
            }

            this.UpdateScrollData(availableSize, viewportUsed);
            viewportUsed.X = Math.Min(availableSize.X, viewportUsed.X);
            viewportUsed.Y = Math.Min(availableSize.Y, viewportUsed.Y);

            return viewportUsed;
        }

        private void UpdateScrollData(Size viewport, Size extent)
        {
            this.scrollData.Viewport = viewport;
            this.scrollData.Extent = extent;

            float x = this.scrollData.Offset.X.Coerce(
                0f, this.scrollData.Extent.X - this.scrollData.Viewport.X);
            float y = this.scrollData.Offset.Y.Coerce(
                0f, this.scrollData.Extent.Y - this.scrollData.Viewport.Y);

            this.scrollData.Offset = new Vector(x, y);
        }
    }
*/
}
