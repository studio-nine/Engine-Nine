namespace Nine.Graphics.UI.Controls
{
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;
    using Nine.Graphics.UI.Internal;

    /// <summary>
    /// A Control that can show progress of an operation.
    /// </summary>
    public class ProgressBar : RangeBase
    {
        /// <summary>
        /// Gets or sets the Orientation of the Progress display.
        /// </summary>
        public Orientation Orientation { get; set; }

        /// <summary>
        /// Gets or sets the Color of the Progress bar. 
        /// </summary>
        public SolidColorBrush BarBrush { get; set; }

        /// <summary>
        /// Gets or sets the Padding to the bar.
        /// </summary>
        public Thickness Padding { get; set; }

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public ProgressBar()
        {
            Orientation = Controls.Orientation.Horizontal;
            Background = new SolidColorBrush(new Color(240, 240, 240));
            BarBrush = new SolidColorBrush(new Color(6, 176, 37));
            Padding = new Thickness(4);
        }

        protected override void OnRender(Nine.Graphics.UI.Renderer.Renderer renderer)
        {
            switch (Orientation)
            {
                case Controls.Orientation.Horizontal:
                    var HorzBar = AbsoluteRenderTransform;
                    HorzBar = HorzBar.Deflate(Padding);
                    HorzBar.Width = HorzBar.Width * ((Value - Minimum) / (Maximum - Minimum));
                    renderer.Draw(HorzBar, BarBrush);
                    break;
                case Controls.Orientation.Vertical: // Should I make this top to down?
                    var VertBar = AbsoluteRenderTransform;
                    VertBar = VertBar.Deflate(Padding);
                    VertBar.Height = VertBar.Height * ((Value - Minimum) / (Maximum - Minimum));
                    renderer.Draw(VertBar, BarBrush);
                    break;
            }
        }
    }
}
