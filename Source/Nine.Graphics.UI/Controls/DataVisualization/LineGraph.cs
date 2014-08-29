namespace Nine.Graphics.UI.Controls.DataVisualization
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;

    public abstract class LineGraph : UIElement
    {
        public bool AdaptiveLimits { get; set; }
        public int ValuesToGraph { get; private set; }
        public Color LineColor { get; set; }

        protected TimeSpan UpdateFrequency { get; set; }

        protected float ElapsedTimeSinceLastUpdate { get { return elapsedTimeSinceLastUpdate; } }
        private float elapsedTimeSinceLastUpdate = 0;

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

        protected void Add(float value)
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

        #region Measure & Arrange

        protected override Vector2 MeasureOverride(Vector2 availableSize)
        {
            return base.MeasureOverride(availableSize);
        }

        protected override Vector2 ArrangeOverride(Vector2 finalSize)
        {
            return base.ArrangeOverride(finalSize);
        }

        #endregion

        protected abstract void Update(float elapsedTime);

        protected override void OnDraw(Renderer.Renderer renderer)
        {
            elapsedTimeSinceLastUpdate += renderer.ElapsedTime;
            if (elapsedTimeSinceLastUpdate >= UpdateFrequency.TotalSeconds)
            {
                Update(renderer.ElapsedTime);
                elapsedTimeSinceLastUpdate -= (float)UpdateFrequency.TotalSeconds;
            }

            // TODO: Render to Texture

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
