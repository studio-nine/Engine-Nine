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
    using System.Collections.Generic;

    /// <summary>
    ///     Represents a control with a single piece of content.
    /// </summary>
    public class ContentControl : Control
    {
        public static readonly ReactiveProperty<UIElement> ContentProperty =
            ReactiveProperty<UIElement>.Register("Content", typeof(ContentControl), null, ContentPropertyChangedCallback);

        public UIElement Content
        {
            get { return this.GetValue(ContentProperty); }
            set { this.SetValue(ContentProperty, value); }
        }


        public override IList<UIElement> GetChildren()
        {
            var child = Content;
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
            UIElement content = this.Content;
            if (content != null)
            {
                content.Arrange(new BoundingRectangle(finalSize.X, finalSize.Y));
            }

            return finalSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            UIElement content = this.Content;
            if (content == null)
            {
                return Vector2.Zero;
            }

            content.Measure(availableSize);
            return content.DesiredSize;
        }

        protected virtual void OnContentChanged(UIElement oldContent, UIElement newContent)
        {
        }

        private static void ContentPropertyChangedCallback(
            ReactiveObject source, ReactivePropertyChangeEventArgs<UIElement> change)
        {
            var contentControl = (ContentControl)source;
            contentControl.InvalidateMeasure();

            UIElement oldContent = change.OldValue;
            if (oldContent != null)
            {
                oldContent.Parent = null;
            }

            UIElement newContent = change.NewValue;
            if (newContent != null)
            {
                newContent.Parent = contentControl;
            }

            contentControl.OnContentChanged(oldContent, newContent);
        }
    }
}