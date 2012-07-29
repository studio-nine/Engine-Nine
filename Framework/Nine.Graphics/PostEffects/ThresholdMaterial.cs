namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
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
            GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
            if ((GraphicsDevice.Textures[0] = texture) != null)
            {
                var halfPixel = new Vector2();
                halfPixel.X = 0.5f / texture.Width;
                halfPixel.Y = 0.5f / texture.Height;
                effect.HalfTexel.SetValue(halfPixel);
            }
            effect.Threshold.SetValue(Threshold);
        }
    }
}