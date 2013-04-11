namespace Nine.Graphics.UI.Media
{
    [System.Windows.Markup.ContentProperty("Visual")]
    public sealed class VisualBrush : Brush
    {
        public UIElement Visual { get; set; }
    }
}
