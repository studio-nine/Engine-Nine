namespace Nine.Graphics.UI.Controls
{
    /// <summary>
    /// A Control that displays a list of data items.
    /// </summary>
    public class ListView : ListBox
    {
        /// <summary>
        /// Gets or sets an ViewBase that defines how the data is styled and organized.
        /// </summary>
        public ViewBase View { get; set; }
    }
}
