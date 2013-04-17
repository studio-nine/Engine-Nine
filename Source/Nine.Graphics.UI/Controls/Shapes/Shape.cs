namespace Nine.Graphics.UI.Controls.Shapes
{
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    [System.Obsolete()]
    public abstract class Shape
    { // This should contain UIElement, But I have to clean it up a bit more :)

        public SolidColorBrush Fill { get; set; }
        public SolidColorBrush Stroke { get; set; }
        public float StrokeThickness { get; set; }

        protected internal virtual void OnRender(DynamicPrimitive dynamicPrimitive)
        {

        }
    }
}
