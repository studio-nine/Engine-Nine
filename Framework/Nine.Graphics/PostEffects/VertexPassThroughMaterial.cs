namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.ObjectModel;
    using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;

    partial class VertexPassThroughMaterial
    {
        partial void ApplyGlobalParameters(DrawingContext context)
        {
            context.HalfPixel.X = -context.HalfPixel.X;

            effect.halfPixel.SetValue(context.HalfPixel);

            context.HalfPixel.X = -context.HalfPixel.X;
        }
    }
}