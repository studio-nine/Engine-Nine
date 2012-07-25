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
            var halfPixel = new Vector2();
            var viewport = GraphicsDevice.Viewport;
            halfPixel.X = -0.5f / viewport.Width;
            halfPixel.Y = 0.5f / viewport.Height;

            effect.HalfPixel.SetValue(halfPixel);
        }
    }
}