namespace Nine.Graphics.UI.Controls.Primitives
{
    using System;
    using Microsoft.Xna.Framework;

    /// <summary>
    /// A method that will handle <see cref="Nine.Graphics.UI.Controls.Primitives.Selector">Selector.SelectionChanged</see> event.
    /// </summary>
    /// <param name="sender">Selector</param>
    /// <param name="e">Event Data</param>
    public delegate void SelectionChangedEventHandler(object sender, SelectionChangedEventArgs e);

    /// <summary>
    /// A control that allows the user to select items from among its child elements.
    /// </summary>
    public abstract class Selector : ItemsControl
    {
        // TODO: Allow multiple selections

        /// <summary>
        /// Gets or sets the index of the current selected item or 
        /// returns negative one (-1) if the selection is empty.
        /// </summary>
        public int SelectedIndex 
        {
            get { return selectedIndex; }
            set
            {
                var Children = this.GetChildren();

                if (selectedIndex > Children.Count)
                    throw new IndexOutOfRangeException("value");

                // TODO: Event
                //if (SelectionChanged != null)
                //    SelectionChanged(this, new SelectionChangedEventArgs(Children[value], Children[selectedIndex]));

                if (value < -1)
                    selectedIndex = -1;
                else
                    selectedIndex = value;
            }
        }
        private int selectedIndex = 0;

        /// <summary>
        /// returns the current selected element.
        /// </summary>
        public virtual UIElement SelectedItem()
        {
            var Children = this.GetChildren();
            if (Children.Count > SelectedIndex)
                return null;
            else
                return Children[SelectedIndex];
        }

        /// <summary>
        /// Occurs when the selection changes.
        /// </summary>
        //public event SelectionChangedEventHandler SelectionChanged;
    }
}
