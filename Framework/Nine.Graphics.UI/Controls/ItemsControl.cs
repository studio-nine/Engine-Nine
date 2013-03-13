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
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;

    /// <summary>
    ///     <see cref = "ItemsControl">ItemsControl</see> allows you to represent a collection of items and provides scaffolding to generate the UI for each item.
    /// </summary>
    public class ItemsControl : Control
    {
        private IDisposable changingItems; // Why u mad Visual Studio?!
        private bool isItemsSourceNew;

        /// <summary>
        ///     Gets or sets a function that is used to generate the <see cref = "UIElement">UIElement</see> for each item in the <see cref = "ItemsSource">ItemsSource</see>.
        ///     The function takes one argument of type <see cref = "object">object</see> that represents the item's <see cref = "UIElement.DataContext">DataContext</see>.
        /// </summary>
        public Func<object, UIElement> ItemTemplate { get; set; }

        /// <summary>
        ///     Gets of sets the <see cref = "Panel">Panel</see> used to control the layout of items.
        /// </summary>
        public Panel ItemsPanel { get; set; }

        /// <summary>
        ///     Gets or sets the collection of items to be displayed.
        /// </summary>
        public IEnumerable ItemsSource { get; set; }

        #region Constructor

        /// <summary>
        ///     Initializes a new instance of the <see cref = "ItemsControl">ItemsControl</see> class.
        /// </summary>
        public ItemsControl()
        {
            this.ItemsPanel = new StackPanel();
        }

        #endregion

        #region Methods

        public override IList<UIElement> GetChildren()
        {
            return new UIElement[] { ItemsPanel };
        }

        public override void OnApplyTemplate()
        {
            if (this.isItemsSourceNew)
            {
                this.PopulatePanelFromItemsSource();
                this.isItemsSourceNew = false;
            }
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            Panel child = this.ItemsPanel;
            if (child != null)
                child.Arrange(new BoundingRectangle(finalSize.X, finalSize.Y));

            return finalSize;
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            Panel child = this.ItemsPanel;
            if (child == null)
                return Vector2.Zero;

            child.Measure(availableSize);
            return child.DesiredSize;
        }

        private void PopulatePanelFromItemsSource()
        {
            var children = (ITemplatedList<UIElement>)this.ItemsPanel.Children;
            children.Clear();

            if (this.ItemsSource != null)
            {
                foreach (object item in this.ItemsSource)
                {
                    children.Add(item, this.ItemTemplate);
                }
            }
        }

        #endregion
    }
}
