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
        /// <summary>
        /// Adds a wireframe'ish rectangle.
        /// </summary>
        /// <param name="dynamicPrimitive"></param>
        /// <param name="bound"></param>
        /// <param name="color"></param>
        /// <param name="world"></param>
        public static void AddRectangle(this DynamicPrimitive dynamicPrimitive, BoundingRectangle bound, Color color, float lineWidth, Matrix? world)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.LineList, null, world, lineWidth);
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

        public static void AddRectangle(this DynamicPrimitive dynamicPrimitive, BoundingRectangle bound, Rectangle? Source, Texture2D texture, Color color, Flip flip, Matrix? world)
        {
            Vector3[] Verts = 
            {
                new Vector3(bound.X, bound.Y, 0),
                new Vector3(bound.X, bound.Y + bound.Height, 0),
                new Vector3(bound.X + bound.Width, bound.Y + bound.Height, 0),
                new Vector3(bound.X + bound.Width, bound.Y, 0),
            };

            var texCoords = Nine.Graphics.UI.UIExtensions.TextureCoords(flip);
            // TODO: Calculate Source Rectangle with texCoords
            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, texture, world);
            {
                dynamicPrimitive.AddVertex(Verts[0], color, texCoords[0]);
                dynamicPrimitive.AddVertex(Verts[1], color, texCoords[1]);
                dynamicPrimitive.AddVertex(Verts[2], color, texCoords[2]);
                dynamicPrimitive.AddVertex(Verts[3], color, texCoords[3]);

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
