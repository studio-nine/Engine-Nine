namespace Nine.Graphics.Primitives
{
    using System;
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
            var solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var color = solidColorBrush.ToColor();

                dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, world);
                {
                    dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y, 0) / 64, color);
                    dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y + bound.Height, 0) / 64, color);
                    dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y + bound.Height, 0) / 64, color);
                    dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y, 0) / 64, color);

                    dynamicPrimitive.AddIndex(0);
                    dynamicPrimitive.AddIndex(1);
                    dynamicPrimitive.AddIndex(2);
                    dynamicPrimitive.AddIndex(0);
                    dynamicPrimitive.AddIndex(3);
                    dynamicPrimitive.AddIndex(2);
                }
                dynamicPrimitive.EndPrimitive();
                return;
            }

            var imageBrush = brush as ImageBrush;
            if (imageBrush != null)
            {
                var texture = imageBrush.Source;
                // TODO: Use 'Viewbox.ComputeScaleFactor(...)' to calculate
                // var Rect = ImageBrush.Calculate(texture, bound);
                dynamicPrimitive.AddRectangle(bound, texture, Color.White, null);
                return;
            }
            
            throw new NotSupportedException("brush");
        }

        public static void AddRectangle(this DynamicPrimitive dynamicPrimitive, BoundingRectangle bound, Texture2D texture, Color color, Matrix? world)
        {
            Vector3[] Verts = 
            {
                new Vector3(bound.X, bound.Y, 0) / 64,
                new Vector3(bound.X, bound.Y + bound.Height, 0) / 64,
                new Vector3(bound.X + bound.Width, bound.Y + bound.Height, 0) / 64,
                new Vector3(bound.X + bound.Width, bound.Y, 0) / 64,
            };

            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                // TODO: Create Texture Flipping! ( Matrix or TexCoords? )
                dynamicPrimitive.AddVertex(Verts[0], color, new Vector2(0, 1));
                dynamicPrimitive.AddVertex(Verts[1], color, new Vector2(0, 0));
                dynamicPrimitive.AddVertex(Verts[2], color, new Vector2(1, 0));
                dynamicPrimitive.AddVertex(Verts[3], color, new Vector2(1, 1));

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(2);
            }
            dynamicPrimitive.EndPrimitive();
        }
    }
}
