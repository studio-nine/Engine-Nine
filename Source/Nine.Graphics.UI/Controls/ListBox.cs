namespace Nine.Graphics.UI.Controls
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    /// <summary>
    /// A Control that display's a list of selectable elements.
    /// </summary>
    [System.Windows.Markup.ContentProperty("Children")]
    public class ListBox : Nine.Graphics.UI.Controls.Primitives.Selector
    {
        /// <summary>
        /// Gets or sets select behavior.
        /// </summary>
        public SelectionMode SelectionMode { get; set; }

        public Orientation Orientation { get { return panel.Orientation; } set { panel.Orientation = value; } }
        public IList<UIElement> Children { get { return panel.Children; } }

        public SolidColorBrush SelectorBrush { get; set; }
        public float SelectorBrushThickness { get; set; }

        private StackPanel panel;

        public ListBox()
        {
            panel = (StackPanel)this.Register(new StackPanel());
            SelectorBrush = new SolidColorBrush(Color.Blue);
            SelectorBrushThickness = 2;
        }

        // TODO: Input Selection

        public override UIElement SelectedItem()
        {
            var Children = panel.GetChildren();
            if (SelectedIndex > Children.Count)
                return null;
            else
                return Children[SelectedIndex];
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            panel.Measure(availableSize);
            return base.MeasureOverride(availableSize);
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            panel.Arrange(new BoundingRectangle(finalSize.X, finalSize.Y));
            return base.ArrangeOverride(finalSize);
        }

        protected internal override void OnRender(Nine.Graphics.UI.Renderer.IRenderer renderer)
        {
            base.OnRender(renderer);

            var selectedItem = SelectedItem();
            if (selectedItem != null)
            { // This design is going to be changed in a later release
                renderer.Draw(selectedItem.AbsoluteRenderTransform, SelectorBrush);
            }
            panel.OnRender(renderer);
        }
    }
}
