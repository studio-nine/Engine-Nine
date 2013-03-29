namespace Nine.Graphics.UI.Controls
{
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Controls.Primitives;

    /// <summary>
    /// A Control that display's a list of selectable elements.
    /// </summary>
    public class ListBox : Selector
    {
        /// <summary>
        /// Gets or sets select behavior.
        /// </summary>
        public SelectionMode SelectionMode { get; set; }

        // TODO: Input Selection
    }
}
