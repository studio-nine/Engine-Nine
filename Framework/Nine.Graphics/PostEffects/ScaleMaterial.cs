namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Nine.Graphics.Drawing;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class ScaleMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            effect.PixelSize.SetValue(context.PixelSize);
        }
    }
}