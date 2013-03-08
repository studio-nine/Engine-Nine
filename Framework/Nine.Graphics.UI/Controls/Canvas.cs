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
    using Microsoft.Xna.Framework;
    using System;

    public class Canvas : Panel
    {
        public static readonly ReactiveProperty<float> LeftProperty = ReactiveProperty<float>.Register(
            "Left", typeof(Canvas), float.NaN, ReactivePropertyChangedCallbacks.InvalidateArrange);

        public static readonly ReactiveProperty<float> TopProperty = ReactiveProperty<float>.Register(
            "Top", typeof(Canvas), float.NaN, ReactivePropertyChangedCallbacks.InvalidateArrange);

        public static float GetLeft(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return element.GetValue(LeftProperty);
        }

        public static float GetTop(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return element.GetValue(TopProperty);
        }

        public static void SetLeft(UIElement element, float value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(LeftProperty, value);
        }

        public static void SetTop(UIElement element, float value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(TopProperty, value);
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            foreach (UIElement element in this.GetChildren())
            {
                if (element == null)
                {
                    continue;
                }

                float x = 0f;
                float y = 0f;

                float left = GetLeft(element);
                if (!float.IsNaN(left))
                {
                    x = left;
                }

                float top = GetTop(element);
                if (!float.IsNaN(top))
                {
                    y = top;
                }

                element.Arrange(new BoundingRectangle(x, y, element.DesiredSize.X, element.DesiredSize.Y));
            }

            return finalSize;
        }

        protected override BoundingRectangle? GetClippingRect(Vector2 finalSize)
        {
            return null;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            var infiniteAvailableSize = new Vector2(float.PositiveInfinity, float.PositiveInfinity);

            foreach (UIElement element in this.GetChildren())
            {
                if (element != null)
                {
                    element.Measure(infiniteAvailableSize);
                }
            }

            return new Vector2();
        }
    }
}
