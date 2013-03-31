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
    using Nine.Graphics.UI.Controls.Primitives;

    /// <summary>
    /// A Scrollable area with visible childrens.
    /// </summary>
    public class ScrollViewer : ContentControl
    {
        #region Properties

        /// <summary>
        /// Gets or sets, if you can scroll Horizontally.
        /// </summary>
        public bool CanHorizontallyScroll
        {
            get { return this.scrollInfo != null ? this.scrollInfo.CanHorizontallyScroll : false; }
            set
            {
                if (this.scrollInfo != null)
                {
                    this.scrollInfo.CanHorizontallyScroll = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets, if you can scroll Vertically.
        /// </summary>
        public bool CanVerticallyScroll
        {
            get { return this.scrollInfo != null ? this.scrollInfo.CanVerticallyScroll : false; }
            set
            {
                if (this.scrollInfo != null)
                {
                    this.scrollInfo.CanVerticallyScroll = value;
                }
            }
        }

        /// <summary>
        /// Gets how much there is to display.
        /// </summary>
        public Vector2 Extent
        {
            get { return this.scrollInfo != null ? this.scrollInfo.Extent : new Vector2(); }
        }

        /// <summary>
        /// Gets the current scroll offset.
        /// </summary>
        public Vector2 Viewport
        {
            get { return this.scrollInfo != null ? this.scrollInfo.Viewport : new Vector2(); }
        }

        #endregion

        private bool isInsertingScrollContentPresenter;
        private IScrollInfo scrollInfo;

        protected override void OnContentChanged(UIElement oldContent, UIElement newContent)
        {
            var oldScrollContentPresenter = oldContent as ScrollContentPresenter;
            if (oldScrollContentPresenter != null)
            {
                oldScrollContentPresenter.Content.Parent = null;
            }

            var newScrollInfo = newContent as IScrollInfo;
            if (newScrollInfo != null)
            {
                this.scrollInfo = newScrollInfo;

                if (oldContent != null && this.isInsertingScrollContentPresenter)
                {
                    ((ScrollContentPresenter)newContent).Content = oldContent;
                }

                this.isInsertingScrollContentPresenter = false;
            }
            else
            {
                this.isInsertingScrollContentPresenter = true;
                this.Content = new ScrollContentPresenter();
            }
        }
    }
}
