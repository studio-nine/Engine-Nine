namespace Nine.Graphics.Materials
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Drawing;

    partial class LuminanceMaterial
    {
        public bool IsDownScale;

        partial void BeginApplyLocalParameters(DrawingContext context, LuminanceMaterial previousMaterial)
        {
            var halfPixel = new Vector2();
            var viewport = context.GraphicsDevice.Viewport;
            halfPixel.X = 0.5f / viewport.Width;
            halfPixel.Y = 0.5f / viewport.Height;

            effect.HalfPixel.SetValue(halfPixel);
            effect.shaderIndex.SetValue(IsDownScale ? 1 : 0);
        }
    }
}