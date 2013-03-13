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
    using System.Linq;

    using Nine.Graphics.UI.Media;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework;

    public abstract class Panel : UIElement
    {
        public IList<UIElement> Children
        {
            get { this.EnsureChildrenCollection(); return this.children; }
            protected set { this.children = value; }
        }

        public SolidColorBrush Background { get; set; }
        private IList<UIElement> children;

        #region Methods

        public override void OnRender(SpriteBatch spriteBatch)
        {
            if (this.Background != null)
                spriteBatch.Draw(RenderTransform, Background.Color);

            foreach (var child in children)
                child.OnRender(spriteBatch);
        }

        public override IList<UIElement> GetChildren()
        {
            return this.children;
        }

        protected virtual IList<UIElement> CreateChildrenCollection()
        {
            return new ElementCollection(this);
        }

        private void EnsureChildrenCollection()
        {
            if (this.children == null)
                this.children = this.CreateChildrenCollection();
        }

        #endregion
    }
}
