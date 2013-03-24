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
    using System.Collections.Generic;

    using Nine.Graphics.UI.Internal;
    using Nine.Graphics.UI.Media;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    public class Border : UIElement
    {
        private readonly IList<BoundingRectangle> borders = new List<BoundingRectangle>();

        public SolidColorBrush BorderBrush { get; set; }

        public Thickness BorderThickness { get; set; }
        public Thickness Padding { get; set; }
        
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

        protected internal override void OnRender(SpriteBatch spriteBatch)
        {
            base.OnRender(spriteBatch);

            if (BorderThickness != Thickness.Empty && BorderBrush != null)
            {
                GenerateBorders();
                foreach (BoundingRectangle border in this.borders)
                {
                    var Rect = border;
                    Rect.X += AbsoluteVisualOffset.X;
                    Rect.Y += AbsoluteVisualOffset.Y;
                    spriteBatch.Draw(Rect, BorderBrush);
                }
            }

            if (Content != null)
                Content.OnRender(spriteBatch);
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
