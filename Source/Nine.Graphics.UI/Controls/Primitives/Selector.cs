namespace Nine.Graphics.UI.Controls.Primitives
{
    using System;
    using Microsoft.Xna.Framework;

    // TODO: Error when the SelectedIndex is set before the children ,"Out Of Range"
    // TODO: Allow multiple selections

    /// <summary>
    /// A control that allows the user to select items from among its child elements.
    /// </summary>
    public abstract class Selector : ItemsControl
    {
        /// <summary>
        /// Gets or sets the index of the current selected item or 
        /// returns negative one (-1) if the selection is empty.
        /// </summary>
        public int SelectedIndex 
        {
            get { return selectedIndex; }
            set
            {
                var Children = this.Children();
                if (SelectionChanged != null)
                {
                    var NewChild = Children.Count > value ? Children[value] : null;
                    var PrevChild = Children.Count > value ? Children[selectedIndex] : null;
                    SelectionChanged(this, new SelectionChangedEventArgs(NewChild, PrevChild));
                }

                // This has a change of creating a issue
                if (Children != null)
                    selectedIndex = (int)MathHelper.Clamp(value, -1, Children.Count);
                else
                    selectedIndex = value;
            }
        }
        private int selectedIndex = 0;

        /// <summary>
        /// returns the current selected element.
        /// </summary>
        public UIElement SelectedItem()
        {
            var Children = this.Children();
            if (Children.Count > SelectedIndex)
                return null;
            else
                return Children[SelectedIndex];
        }

        /// <summary>
        /// Get Children
        /// </summary>
        /// <returns>Children's</returns>
        protected virtual System.Collections.Generic.IList<UIElement> Children()
        {
            return ItemsSource;
        }

        /// <summary>
        /// Occurs when the selection changes.
        /// </summary>
        public event EventHandler<SelectionChangedEventArgs> SelectionChanged;
    }
}
