namespace Nine.Graphics.UI.Controls.DataVisualization
{
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    public class LineGraph : UIElement
    {
        // TODO: Text

        public bool AdaptiveLimits { get; set; }
        public int ValuesToGraph { get; private set; }
        public Color LineColor { get; set; }

        protected int MinimumValue;
        protected int MaxValue;
        protected int AdaptiveMinimum;
        protected int AdaptiveMaximum = 1000;
        protected int AverageValue;

        protected readonly List<float> GraphValues = new List<float>();

        public LineGraph() : this(100) { }
        public LineGraph(int limitValues)
        {
            this.AdaptiveLimits = true;
            this.ValuesToGraph = limitValues;
            this.LineColor = Color.Red;
        }

        public void Add(float value)
        {
            GraphValues.Add(value);

            if (GraphValues.Count > ValuesToGraph + 1)
                GraphValues.RemoveAt(0);

            if (GraphValues.Count <= 2)
                return;

            MaxValue = (int)GraphValues.Max();
            AverageValue = (int)GraphValues.Average();
            MinimumValue = (int)GraphValues.Min();

            if (AdaptiveLimits)
            {
                AdaptiveMaximum = MaxValue;
                AdaptiveMinimum = 0;
            }
        }

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            if (Visible == Visibility.Collapsed) 
                return Vector2.Zero;
            return base.MeasureOverride(availableSize);
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            if (Visible == Visibility.Collapsed) 
                return Vector2.Zero;
            return base.ArrangeOverride(finalSize);
        }

        protected internal override void OnRender(Renderer.Renderer renderer)
        {
            // TODO: Render to Texture
            if (Visible != Visibility.Visible) 
                return;
            base.OnRender(renderer);

            var Bounds = AbsoluteRenderTransform;
            float x = Bounds.X;
            float deltaX = Bounds.Width / (float)ValuesToGraph;
            float yScale = Bounds.Bottom - Bounds.Top;

            if (GraphValues.Count <= 2)
                return;

            for (int i = GraphValues.Count - 1; i > 0; i--)
            {
                var y1 = Bounds.Bottom - ((GraphValues[i] / (AdaptiveMaximum - AdaptiveMinimum)) * yScale);
                var y2 = Bounds.Bottom - ((GraphValues[i - 1] / (AdaptiveMaximum - AdaptiveMinimum)) * yScale);

                var x1 = new Vector2(MathHelper.Clamp(x, Bounds.Left, Bounds.Right), MathHelper.Clamp(y1, Bounds.Top, Bounds.Bottom));
                var x2 = new Vector2(MathHelper.Clamp(x + deltaX, Bounds.Left, Bounds.Right), MathHelper.Clamp(y2, Bounds.Top, Bounds.Bottom));

                renderer.Draw(x1, x2, LineColor);
                x += deltaX;
            }
        }
    }
}
