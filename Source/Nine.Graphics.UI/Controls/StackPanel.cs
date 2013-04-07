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

    /// <summary>
    /// Arranges child elements into a single line that can be oriented horizontally or vertically.
    /// </summary>
    public class StackPanel : Panel
    {
        /// <summary>
        /// Gets or sets the Orientation of the Childrens Arrangement.
        /// </summary>
        public Orientation Orientation { get; set; }

        protected override Vector2 ArrangeOverride(Vector2 arrangeSize)
        {
            bool isHorizontalOrientation = this.Orientation == Orientation.Horizontal;
            var finalRect = new BoundingRectangle(0, 0, arrangeSize.X, arrangeSize.Y);
            float width = 0;
            float height = 0;
            foreach (UIElement child in Children)
            {
                if (child != null)
                {
                    if (isHorizontalOrientation)
                    {
                        finalRect.X += width;
                        width = child.DesiredSize.X;
                        finalRect.Width = width;
                        finalRect.Height = Math.Max(arrangeSize.Y, child.DesiredSize.Y);
                    }
                    else
                    {
                        finalRect.Y += height;
                        height = child.DesiredSize.Y;
                        finalRect.Height = height;
                        finalRect.Width = Math.Max(arrangeSize.X, child.DesiredSize.X);
                    }
                    child.Arrange(finalRect);
                }
            }

            return arrangeSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var size = new Vector2();
            bool isHorizontalOrientation = this.Orientation == Orientation.Horizontal;
            if (isHorizontalOrientation)
                availableSize.X = float.PositiveInfinity;
            else
                availableSize.Y = float.PositiveInfinity;

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
                Vector2 desiredSize = child.DesiredSize;
                if (isHorizontalOrientation)
                {
                    size.X += desiredSize.X;
                    size.Y = Math.Max(size.Y, desiredSize.Y);
                }
                else
                {
                    size.X = Math.Max(size.X, desiredSize.X);
                    size.Y += desiredSize.Y;
                }
            }
            return size;
        }
    }
}
