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

    [System.Windows.Markup.ContentProperty("Children")]
    public abstract class Panel : UIElement
    {
        public Panel()
        {
            children = new NotificationCollection<UIElement>();
            children.Sender = this;
            children.Added += Child_Added;
            children.Removed += Child_Removed;
        }

        #region Children

        public IList<UIElement> Children
        {
            get { return this.children; }
        }
        private NotificationCollection<UIElement> children;

        void Child_Added(object value)
        {
            var element = value as UIElement;
            if (element != null)
            {
                Register(element);
            }
        }

        void Child_Removed(object value)
        {
            var element = value as UIElement;
            if (element != null)
            {
                Unregister(element);
            }
        }

        #endregion

        #region Methods

        protected internal override void OnRender(Nine.Graphics.UI.Renderer.IRenderer renderer)
        {
            base.OnRender(renderer);
            foreach (var child in children)
                child.OnRender(renderer);
        }

        public override IList<UIElement> GetChildren()
        {
            return this.children;
        }

        #endregion
    }
}
