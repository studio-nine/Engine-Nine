namespace Nine.Graphics.Materials
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Drawing;

    partial class LuminanceMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            effect.halfPixel.SetValue(context.HalfPixel);
        }
    }
}