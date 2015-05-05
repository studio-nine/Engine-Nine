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
    using Nine.AttachedProperty;
    using Nine.Graphics.Primitives;
    using System;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// <see cref="ItemsControl">ItemsControl</see> allows you to represent a collection of items and provides scaffolding to generate the UI for each item.
    /// </summary>
    [ContentProperty("ItemsSource")]
    public class ItemsControl : Control, IContainer, INotifyCollectionChanged<UIElement>
    {
        /// <summary>
        /// Gets of sets the <see cref="Panel">Panel</see> used to control the layout of items.
        /// </summary>
        public StackPanel ItemsPanel 
        {
            get { return itemsPanel; }
            set
            {
                itemsPanel = value;
                itemsPanel.Parent = this;
            }
        }
        private StackPanel itemsPanel;

        /// <summary>
        /// Gets the collection of items to be displayed.
        /// </summary>
        public IList<UIElement> ItemsSource 
        {
            get { return ItemsPanel.Children; }
        }

        /// <summary>
        /// Occurs when a Child is Added.
        /// </summary>
        public event Action<UIElement> Added;

        /// <summary>
        /// Occurs when a Child is Removed.
        /// </summary>
        public event Action<UIElement> Removed;

        IList IContainer.Children { get { return (IList)ItemsSource; } }

        #region Constructor

        /// <summary>
        /// Initializes a new instance of the <see cref="ItemsControl">ItemsControl</see> class.
        /// </summary>
        public ItemsControl()
        {
            ItemsPanel = new StackPanel();
            // event calls event, works for now.
            ItemsPanel.Added += (e) => { if (Added != null) { Added(e); }; };
            ItemsPanel.Removed += (e) => { if (Removed != null) { Removed(e); }; }; 
        }

        #endregion

        #region Methods

        protected override void OnDraw(Nine.Graphics.UI.Renderer.Renderer renderer)
        {
            this.ItemsPanel.Draw(renderer);
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (ItemsPanel != null)
                ItemsPanel.Arrange(new BoundingRectangle(finalSize.X, finalSize.Y));

            return finalSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (ItemsPanel == null)
                return Vector2.Zero;

            ItemsPanel.Measure(availableSize);
            return ItemsPanel.DesiredSize;
        }

        #endregion
    }
}
