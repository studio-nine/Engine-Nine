namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ThresholdMaterial
    {
        public float Threshold
        {
            get { return threshold; }
            set { threshold = value; }
        }
        private float threshold;

        partial void OnCreated()
        {
            Threshold = 0.5f;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, ThresholdMaterial previousMaterial)
        {
            var halfPixel = new Vector2();
            var viewport = context.GraphicsDevice.Viewport;
            halfPixel.X = 0.5f / viewport.Width;
            halfPixel.Y = 0.5f / viewport.Height;

            effect.HalfPixel.SetValue(halfPixel);
            effect.Threshold.SetValue(Threshold);
        }
    }
}