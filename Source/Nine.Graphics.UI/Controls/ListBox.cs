namespace Nine.Graphics.UI.Controls
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Controls.Primitives;
    using Nine.Graphics.UI.Media;

    /// <summary>
    /// A Control that display's a list of selectable elements.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Children")]
    public class ListBox : Selector
    {
        /// <summary>
        /// Gets or sets select behavior.
        /// </summary>
        public SelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the Orientation of the <see cref="ListBox"/>.
        /// </summary>
        public Orientation Orientation 
        { 
            get { return ItemsPanel.Orientation; }
            set { ItemsPanel.Orientation = value; } 
        }

        public IList<UIElement> Children { get { return this.ItemsSource; } }
 
        // Should the input be more changable?
        protected override void OnKeyDown(KeyboardEventArgs e)
        {
            switch (Orientation)
            {
                case Controls.Orientation.Vertical:
                    {
                        if (e.Key == Microsoft.Xna.Framework.Input.Keys.Down)
                        {
                            if (SelectedIndex < (Children.Count - 1))
                                SelectedIndex++;
                        }
                        else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Up)
                        {
                            if (SelectedIndex > 0)
                                SelectedIndex--;
                        }
                        break;
                    }
                case Controls.Orientation.Horizontal:
                    {
                        if (e.Key == Microsoft.Xna.Framework.Input.Keys.Right)
                        {
                            if (SelectedIndex < (Children.Count - 1))
                                SelectedIndex++;
                        }
                        else if (e.Key == Microsoft.Xna.Framework.Input.Keys.Left)
                        {
                            if (SelectedIndex > 0)
                                SelectedIndex--;
                        }
                        break;
                    }
            }
        }
    }
}
