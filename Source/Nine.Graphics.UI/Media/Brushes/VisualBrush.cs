namespace Nine.Graphics.UI.Media
{
    [System.Obsolete("Brushes are not messured and arranged")]
    [System.Windows.Markup.ContentProperty("Visual")]
    public sealed class VisualBrush : Brush
    {
        public UIElement Visual { get; set; }
    }
}
