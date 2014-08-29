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
    using Microsoft.Xna.Framework;
    using System.Collections.Generic;
    using Nine.Graphics.Primitives;
    using System.Collections;

    /// <summary>
    /// Represents a control with a single piece of content.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Content")]
    public class ContentControl : Control, IContainer
    {
        /// <summary>
        /// Gets a value indicating if it contains content
        /// </summary>
        public bool HasContent { get { return content != null; } }

        /// <summary>
        /// Gets or sets the content of <see cref="ContentControl"/>.
        /// </summary>
        public UIElement Content 
        {
            get { return content; }
            set
            {
                OnContentChanged(content, value);
                content = value;
                content.Parent = this;
            }
        }
        private UIElement content;

        IList IContainer.Children { get { return new UIElement[] { Content }; } }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (Content != null)
                Content.Arrange(new BoundingRectangle(finalSize.X, finalSize.Y));
            return finalSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (Content == null)
                return Vector2.Zero;
            Content.Measure(availableSize);
            return content.DesiredSize;
        }

        protected override void OnDraw(Nine.Graphics.UI.Renderer.Renderer renderer)
        {
            if (Content != null)
            {
                Content.Draw(renderer);
            }
        }

        protected virtual void OnContentChanged(UIElement oldContent, UIElement newContent) 
        { 
        
        }
    }
}
