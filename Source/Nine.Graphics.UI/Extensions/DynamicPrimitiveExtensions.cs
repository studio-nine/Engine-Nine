namespace Nine.Graphics.Primitives
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class DynamicPrimitiveExtensions
    {
        public static void AddRectangle(this DynamicPrimitive dynamicPrimitive, BoundingRectangle bound, Brush brush, Matrix? world)
        {
            var color = Color.White;
            var texture = Nine.Graphics.GraphicsResources<BlankTexture>.GetInstance(dynamicPrimitive.GraphicsDevice).Texture;

            if (brush.GetType().Equals(typeof(SolidColorBrush)))
            {
                color = (brush as SolidColorBrush).ToColor();
            }
            else if (brush.GetType().Equals(typeof(ImageBrush)))
            {

            }
            else
                throw new System.ArgumentNullException("brush");

            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, texture, world);
            {
                dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y + bound.Height, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y + bound.Height, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y, 0), color);

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(0);
            }
            dynamicPrimitive.EndPrimitive();
        }
    }
}
