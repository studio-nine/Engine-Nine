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
                    var HorzBar = AbsoluteRenderTransform;

                    HorzBar.X += BarMargin.Left;
                    HorzBar.Y += BarMargin.Top;
                    HorzBar.Width -= BarMargin.Right * 2;
                    HorzBar.Height -= BarMargin.Bottom * 2;

                    HorzBar.Width = HorzBar.Width * ((Value - Minimum) / (Maximum - Minimum));
                    spriteBatch.Draw(HorzBar, BarBrush);
                    break;
                case Controls.Orientation.Vertical:
                    var VertBar = AbsoluteRenderTransform;

                    VertBar.X += BarMargin.Left;
                    VertBar.Y += BarMargin.Top;
                    VertBar.Width -= BarMargin.Right * 2;
                    VertBar.Height -= BarMargin.Bottom * 2;

                    VertBar.Height = VertBar.Height * ((Value - Minimum) / (Maximum - Minimum));
                    spriteBatch.Draw(VertBar, BarBrush);
                    break;
            }
        }
    }
}
