namespace Nine.Graphics.UI.Controls
{
    using System;

    /// <summary>
    /// Provide data for <see cref="Nine.Graphics.UI.Controls.Primitives.Selector">Selector.SelectionChanged</see>.
    /// </summary>
    public class SelectionChangedEventArgs : EventArgs
    {
        public object NewSelection { get; private set; }
        public object PrevSelection { get; private set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public SelectionChangedEventArgs(object NewSelection, object PrevSelection)
        {
            this.NewSelection = NewSelection;
            this.PrevSelection = PrevSelection;
        }
    }
}
