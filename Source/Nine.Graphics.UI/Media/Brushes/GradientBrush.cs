namespace Nine.Graphics.UI.Media
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    [Obsolete("Not Implemented Gradient Brushes")]
    [ContentProperty("GradientStops")]
    public abstract class GradientBrush : Brush
    {
        public List<GradientStop> GradientStops { get; set; }
    }

    [Nine.Serialization.BinarySerializable]
    public class GradientStop
    {
        public Vector3 Color { get; set; }
        public float Alpha { get; set; }
        public float Offset { get; set; }
    }
}
