namespace Nine.Graphics.UI.Controls
{
    using Microsoft.Xna.Framework;
    using Nine.Graphics.UI.Media;

    public class ProgressBar : RangeBase
    {
        public Orientation Orientation { get; set; }
        public SolidColorBrush BarBrush { get; set; }

        public Thickness BarMargin { get; set; }

        public ProgressBar()
        {
            Orientation = Controls.Orientation.Horizontal;
            Background = new SolidColorBrush(new Color(240, 240, 240));
            BarBrush = new SolidColorBrush(new Color(6, 176, 37));
            BarMargin = new Thickness(4);
        }

        protected internal override void OnRender(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            base.OnRender(spriteBatch);
            switch (Orientation)
            {
                case Controls.Orientation.Horizontal:
                    var SliderTransform = AbsoluteRenderTransform;

                    SliderTransform.X += BarMargin.Left;
                    SliderTransform.Y += BarMargin.Top;
                    SliderTransform.Width -= BarMargin.Right * 2;
                    SliderTransform.Height -= BarMargin.Bottom * 2;

                    SliderTransform.Width = SliderTransform.Width * ((Value - Minimum) / (Maximum - Minimum));
                    spriteBatch.Draw(SliderTransform, BarBrush);
                    break;
                case Controls.Orientation.Vertical:
                    break;
            }
        }
    }
}
