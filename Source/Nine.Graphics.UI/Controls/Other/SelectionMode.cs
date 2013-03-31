namespace Nine.Graphics.UI.Controls
{
    public enum SelectionMode
    {
        /// <summary>
        /// No Selection at all.
        /// </summary>
        None,
        /// <summary>
        /// Selects when the Cursor moves over or the controls changes.
        /// </summary>
        Game,
        /// <summary>
        /// Can only select one.
        /// </summary>
        Single,
        /// <summary>
        /// Can select multiple.
        /// </summary>
        Multiple,
    }
}
