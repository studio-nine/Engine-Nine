namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    partial class ThickLineMaterial
    {
        public float Thickness = 1;

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            var scale = new Vector2();
            var viewport = GraphicsDevice.Viewport;
            var aspectRatio = viewport.AspectRatio;

            // Calculate the magic scale
            scale.X = 1f / viewport.Width;
            scale.Y = 1f / viewport.Width * aspectRatio;

            effect.Scale.SetValue(scale);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, ThickLineMaterial previousMaterial)
        {
            effect.WorldViewProjection.SetValue(world * context.matrices.ViewProjection);
            effect.Thickness.SetValue(Thickness);
        }
    }
}