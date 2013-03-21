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
            scale.X = 1f / viewport.Width;
            scale.Y = 1f / viewport.Height;
            effect.Scale.SetValue(scale);
            effect.NearPlane.SetValue(MatrixHelper.GetNearClip(context.Projection));
            effect.Projection.SetValue(context.matrices.projection);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, ThickLineMaterial previousMaterial)
        {
            Matrix worldView;
            Matrix.Multiply(ref world, ref context.matrices.view, out worldView);
            effect.WorldView.SetValue(worldView);
            effect.Thickness.SetValue(Thickness);            
        }
    }
}