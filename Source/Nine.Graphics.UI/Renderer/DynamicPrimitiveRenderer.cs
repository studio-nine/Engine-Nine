namespace Nine.Graphics.UI.Renderer
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    public class DynamicPrimitiveRenderer : Renderer
    {
        private DynamicPrimitive dynamicPrimitive;

        public DynamicPrimitiveRenderer(GraphicsDevice graphics)
            : base(graphics)
        {
            dynamicPrimitive = new DynamicPrimitive(graphics);
        }

        public override void Begin(DrawingContext context)
        {

        }

        public override void End(DrawingContext context)
        {
            dynamicPrimitive.Draw(context, null);
            dynamicPrimitive.Clear();
        }


        public override void Draw(BoundingRectangle bound, Color color)
        {
            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, null);
            {
                dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y + bound.Height, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y + bound.Height, 0), color);
                dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y, 0), color);

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(2);
            }
            dynamicPrimitive.EndPrimitive();
        }

        public override void Draw(System.Collections.Generic.IEnumerable<Vector2> poly, Color color, bool join)
        {
            // Fix this!
            Vector3 prevPoint = Vector3.Zero;
            foreach (var p in poly)
            {
                Vector3 newPoint = new Vector3(p, 0);
                if (prevPoint != Vector3.Zero)
                    dynamicPrimitive.AddLine(prevPoint, newPoint, color, 2);
                prevPoint = newPoint;
            }
        }

        public override void Draw(Vector2 from, Vector2 to, Color color)
        {
            dynamicPrimitive.AddLine(new Vector3(from, -2), new Vector3(to, -2), color, 2);
        }


        #region Texture

        public override void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source)
        {
            this.Draw(texture, bound, Source, Color.White, Flip.None);
        }

        public override void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color)
        {
            this.Draw(texture, bound, Source, color, Flip.None);
        }

        public override void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color, Flip flip)
        {
            var texCoords = Nine.Graphics.UI.UIExtensions.TextureCoords(flip);
            dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, texture, null);
            {
                dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y, 0), color, texCoords[0]);
                dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y + bound.Height, 0), color, texCoords[1]);
                dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y + bound.Height, 0), color, texCoords[2]);
                dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y, 0), color, texCoords[3]);

                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(1);
                dynamicPrimitive.AddIndex(2);
                dynamicPrimitive.AddIndex(0);
                dynamicPrimitive.AddIndex(3);
                dynamicPrimitive.AddIndex(2);
            }
            dynamicPrimitive.EndPrimitive();
        }

        #endregion

        [Obsolete("Not Supported")]
        public override void DrawString(SpriteFont spriteFont, string text, Vector2 position, Color color, float rotation, Vector2 origin, Vector2 scale)
        {
            var Size = spriteFont.MeasureString(text);
            dynamicPrimitive.AddRectangle(new BoundingRectangle(position.X, position.Y, Size.X, Size.Y), color, 2, null);
        }
    }
}
