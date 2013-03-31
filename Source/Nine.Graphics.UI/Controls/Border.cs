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
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Internal;
    using Nine.Graphics.Primitives;

    /// <summary>
    /// Draws a border and/or background around another element.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class Border : UIElement
    {
        private readonly IList<BoundingRectangle> borders = new List<BoundingRectangle>();

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        public Nine.Graphics.UI.Media.SolidColorBrush BorderBrush { get; set; }

        /// <summary>
        /// Gets or sets the borders thickness.
        /// </summary>
        public Thickness BorderThickness { get; set; }

        /// <summary>
        /// Gets or sets the padding between the border and the Content.
        /// </summary>
        public Thickness Padding { get; set; }
        
        /// <summary>
        /// Gets or sets the single child element.
        /// </summary>
        public UIElement Content 
        {
            get { return content; }
            set 
            {
                content = value;
                content.Parent = this;
            }
        }
        private UIElement content;

        #region Methods

        protected internal override void OnRender(DynamicPrimitive dynamicPrimitive)
        {
            base.OnRender(dynamicPrimitive);

            if (BorderThickness != Thickness.Empty && BorderBrush != null)
            {
                GenerateBorders();
                foreach (BoundingRectangle border in this.borders)
                {
                    var Rect = border;
                    Rect.X += AbsoluteVisualOffset.X;
                    Rect.Y += AbsoluteVisualOffset.Y;
                    dynamicPrimitive.AddRectangle(Rect, BorderBrush, null);
                }
            }

            if (Content != null)
                Content.OnRender(dynamicPrimitive);
        }

        public override IList<UIElement> GetChildren()
        {
            if (Content != null)
                return new UIElement[] { Content };
            return null;
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            UIElement child = this.Content;
            if (child != null)
            {
                var finalRect = new BoundingRectangle(finalSize.X, finalSize.Y);
                finalRect = finalRect.Deflate(this.BorderThickness);
                finalRect = finalRect.Deflate(this.Padding);
                child.Arrange(finalRect);
            }
            return finalSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            Thickness borderThicknessAndPadding = this.BorderThickness + this.Padding;

            UIElement child = this.Content;
            if (child != null)
            {
                child.Measure(availableSize.Deflate(borderThicknessAndPadding));
                return child.DesiredSize.Inflate(borderThicknessAndPadding);
            }

            return borderThicknessAndPadding.Collapse();
        }

        private void GenerateBorders()
        {
            this.borders.Clear();

            if (this.BorderThickness.Left > 0)
            {
                this.borders.Add(new BoundingRectangle(0, 0, this.BorderThickness.Left, this.ActualHeight));
            }

            if (this.BorderThickness.Top > 0)
            {
                this.borders.Add(
                    new BoundingRectangle(
                        this.BorderThickness.Left, 
                        0, 
                        this.ActualWidth - this.BorderThickness.Left, 
                        this.BorderThickness.Top));
            }

            if (this.BorderThickness.Right > 0)
            {
                this.borders.Add(
                    new BoundingRectangle(
                        this.ActualWidth - this.BorderThickness.Right, 
                        this.BorderThickness.Top, 
                        this.BorderThickness.Right, 
                        this.ActualHeight - this.BorderThickness.Top));
            }

            if (this.BorderThickness.Bottom > 0)
            {
                this.borders.Add(
                    new BoundingRectangle(
                        this.BorderThickness.Left, 
                        this.ActualHeight - this.BorderThickness.Bottom, 
                        this.ActualWidth - (this.BorderThickness.Left + this.BorderThickness.Right), 
                        this.BorderThickness.Bottom));
            }
        }

        #endregion
    }
}
