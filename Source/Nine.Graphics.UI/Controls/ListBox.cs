namespace Nine.Graphics.UI.Controls
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    // TODO: Input Selection
    // TODO: Clean up this Control

    /// <summary>
    /// A Control that display's a list of selectable elements.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Children")]
    public class ListBox : Nine.Graphics.UI.Controls.Primitives.Selector
    {
        /// <summary>
        /// Gets or sets select behavior.
        /// </summary>
        public SelectionMode SelectionMode { get; set; }

        public Orientation Orientation 
        { 
            get { return ItemsPanel.Orientation; }
            set { ItemsPanel.Orientation = value; } 
        }

        public IList<UIElement> Children { get { return this.ItemsSource; } }
    }
}
