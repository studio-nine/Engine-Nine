namespace Nine.Graphics.UI.Media
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Simple gradient brush.
    /// </summary>
    public class GradientBrush : Brush
    {
        // TopRight
        public SolidColorBrush G1 { get; set; }
        // TopLeft
        public SolidColorBrush G2 { get; set; }
        // BottomRight
        public SolidColorBrush G3 { get; set; }
        // BottomLeft
        public SolidColorBrush G4 { get; set; }
    }
}
