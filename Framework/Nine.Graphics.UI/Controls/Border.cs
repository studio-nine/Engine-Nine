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

    using Nine.Graphics.UI.Graphics;
    using Nine.Graphics.UI.Internal;
    using Nine.Graphics.UI.Media;
    using Microsoft.Xna.Framework;

    public class Border : UIElement
    {
        public static readonly ReactiveProperty<Brush> BackgroundProperty =
            ReactiveProperty<Brush>.Register(
                "Background", typeof(Border), ReactivePropertyChangedCallbacks.InvalidateArrange);

        public static readonly ReactiveProperty<Brush> BorderBrushProperty =
            ReactiveProperty<Brush>.Register(
                "BorderBrush", typeof(Border), null, ReactivePropertyChangedCallbacks.InvalidateArrange);

        public static readonly ReactiveProperty<Thickness> BorderThicknessProperty =
            ReactiveProperty<Thickness>.Register(
                "BorderThickness", typeof(Border), new Thickness(), ReactivePropertyChangedCallbacks.InvalidateMeasure);

        public static readonly ReactiveProperty<UIElement> ChildProperty = ReactiveProperty<UIElement>.Register(
            "Child", typeof(Border), null, ChildPropertyChangedCallback);

        public static readonly ReactiveProperty<Thickness> PaddingProperty =
            ReactiveProperty<Thickness>.Register(
                "Padding", typeof(Border), new Thickness(), ReactivePropertyChangedCallbacks.InvalidateMeasure);

        private readonly IList<BoundingRectangle> borders = new List<BoundingRectangle>();

        public Brush Background
        {
            get { return this.GetValue(BackgroundProperty); }
            set { this.SetValue(BackgroundProperty, value); }
        }

        public Brush BorderBrush
        {
            get { return this.GetValue(BorderBrushProperty); }
            set { this.SetValue(BorderBrushProperty, value); }
        }

        public Thickness BorderThickness
        {
            get { return this.GetValue(BorderThicknessProperty); }
            set { this.SetValue(BorderThicknessProperty, value); }
        }

        public UIElement Child
        {
            get { return this.GetValue(ChildProperty); }
            set { this.SetValue(ChildProperty, value); }
        }

        public Thickness Padding
        {
            get { return this.GetValue(PaddingProperty); }
            set { this.SetValue(PaddingProperty, value); }
        }
        
        public override IList<UIElement> GetChildren()
        {
            var child = Child;
            if (child != null)
            {
                children[0] = child;
                return children;
            }
            return null;
        }
        private UIElement[] children = new UIElement[1];

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            UIElement child = this.Child;

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

            UIElement child = this.Child;
            if (child != null)
            {
                child.Measure(availableSize.Deflate(borderThicknessAndPadding));

                return child.DesiredSize.Inflate(borderThicknessAndPadding);
            }

            return borderThicknessAndPadding.Collapse();
        }

        protected override void OnRender(IDrawingContext drawingContext)
        {
            // TODO: Opt
            if (this.BorderThickness != new Thickness() && this.BorderBrush != null)
            {
                this.GenerateBorders();

                foreach (BoundingRectangle border in this.borders)
                {
                    drawingContext.DrawRectangle(border, this.BorderBrush);
                }
            }

            if (this.Background != null)
            {
                drawingContext.DrawRectangle(
                    new BoundingRectangle(0, 0, this.ActualWidth, this.ActualHeight).Deflate(this.BorderThickness), this.Background);
            }
        }

        private static void ChildPropertyChangedCallback(
            ReactiveObject source, ReactivePropertyChangeEventArgs<UIElement> change)
        {
            var border = (Border)source;
            border.InvalidateMeasure();

            UIElement oldChild = change.OldValue;
            if (oldChild != null)
            {
                oldChild.Parent = null;
            }

            UIElement newChild = change.NewValue;
            if (newChild != null)
            {
                newChild.Parent = border;
            }
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
    }
}
