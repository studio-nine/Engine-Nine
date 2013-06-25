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
        /// <summary> TopRight Corner </summary>
        public SolidColorBrush G1 { get; set; }

        /// <summary> TopLeft Corner </summary>
        public SolidColorBrush G2 { get; set; }

        /// <summary> BottomRight Corner </summary>
        public SolidColorBrush G3 { get; set; }

        /// <summary> BottomLeft Corner </summary>
        public SolidColorBrush G4 { get; set; }

        protected internal override void OnRender(Renderer.Renderer renderer, BoundingRectangle bound)
        {
            // How should I do this? Hm.
            base.OnRender(renderer, bound);
        }
    }
}
