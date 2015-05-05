namespace Nine.Graphics.UI.Media
{
    using Microsoft.Xna.Framework;
    using Nine.AttachedProperty;

    /// <summary>
    /// A Brush that allows you to render a <see cref="UIElement"/> inside an area 
    /// (this will remove all the input functions etc). 
    /// </summary>
    [ContentProperty("Visual")]
    public sealed class VisualBrush : Brush
    {
        public UIElement Visual { get; set; }

        public VisualBrush() { }
        public VisualBrush(UIElement Visual)
        {
            this.Visual = Visual;
        }

        protected internal override void OnRender(Renderer.Renderer renderer, BoundingRectangle bound)
        {
            Visual.Measure(new Vector2(bound.Width, bound.Height));
            Visual.Arrange(bound);
            Visual.Draw(renderer);
        }
    }
}
