namespace Nine.Graphics.UI.Media
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    [Obsolete("Not Implemented Gradient Brushes")]
    public abstract class GradientBrush : Brush
    {
        public List<GradientStop> GradientStops { get; set; }
    }

    public class GradientStop
    {
        public Color Color { get; set; }
        public float Offset { get; set; }
    }
}
