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
    using Nine.Graphics.UI.Renderer;
    using Nine.Graphics.UI.Media;

    /// <summary>
    /// Draws a border and/or background around another element.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class Border : UIElement, IContainer
    {
        #region Properties

        private readonly BoundingRectangle[] borders = new BoundingRectangle[4];

        /// <summary>
        /// Gets or sets the border color.
        /// </summary>
        public Brush BorderBrush { get; set; }

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
            get { return content[0]; }
            set 
            {
                content[0] = value;
                content[0].Parent = this;
            }
        }
        private UIElement[] content = new UIElement[1];

        System.Collections.IList IContainer.Children { get { return content; } }

        #endregion 

        #region Constructor

        public Border()
            : this(new SolidColorBrush(Color.White), Thickness.Empty, Thickness.Empty)
        {

        }

        public Border(SolidColorBrush borderBrush, Thickness borderThickness)
            : this(borderBrush, borderThickness, Thickness.Empty)
        {

        }

        public Border(SolidColorBrush borderBrush, Thickness borderThickness, Thickness padding)
        {
            this.BorderBrush = borderBrush;
            this.BorderThickness = borderThickness;
            this.Padding = padding;
        }

        #endregion

        #region Methods

        protected override void OnDraw(Renderer renderer)
        {
            if (BorderThickness != Thickness.Empty && BorderBrush != null)
            {
                GenerateBorders();
                foreach (var border in this.borders)
                {
                    if (border == BoundingRectangle.Empty) continue;
                    var Rect = border;                                    // Cache this with Absolute Offset? 
                    Rect.X += AbsoluteVisualOffset.X;                     // So we dont have to recreate this over and over
                    Rect.Y += AbsoluteVisualOffset.Y;
                    renderer.Draw(Rect, BorderBrush);
                }
            }

            if (Content != null)
            {
                Content.Draw(renderer);
            }
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
            if (this.BorderThickness.Left > 0)
            {
                this.borders[(int)Direction.Left] = new BoundingRectangle(0, 0, this.BorderThickness.Left, this.ActualHeight);
            }
            else borders[(int)Direction.Left] = BoundingRectangle.Empty;

            if (this.BorderThickness.Top > 0)
            {
                this.borders[(int)Direction.Top] = new BoundingRectangle(this.BorderThickness.Left, 0, this.ActualWidth - this.BorderThickness.Left, this.BorderThickness.Top);
            }
            else borders[(int)Direction.Top] = BoundingRectangle.Empty;

            if (this.BorderThickness.Right > 0)
            {
                this.borders[(int)Direction.Right] = new BoundingRectangle(this.ActualWidth - this.BorderThickness.Right, this.BorderThickness.Top, this.BorderThickness.Right, this.ActualHeight - this.BorderThickness.Top);
            }
            else borders[(int)Direction.Right] = BoundingRectangle.Empty;

            if (this.BorderThickness.Bottom > 0)
            {
                this.borders[(int)Direction.Bottom] = new BoundingRectangle(this.BorderThickness.Left, this.ActualHeight - this.BorderThickness.Bottom, this.ActualWidth - (this.BorderThickness.Left + this.BorderThickness.Right), this.BorderThickness.Bottom);
            }
            else borders[(int)Direction.Bottom] = BoundingRectangle.Empty;
        }

        internal BoundingRectangle GetBorder(Direction direction)
        {
            return borders[(int)direction];
        }

        #endregion
    }
}
