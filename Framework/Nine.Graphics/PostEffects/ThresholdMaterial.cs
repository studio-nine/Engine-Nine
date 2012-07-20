namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ThresholdMaterial
    {
        public float Threshold { get; set; }

        partial void OnCreated()
        {
            Threshold = 0.5f;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, ThresholdMaterial previousMaterial)
        {
            effect.Threshold.SetValue(Threshold);
        }
    }
}