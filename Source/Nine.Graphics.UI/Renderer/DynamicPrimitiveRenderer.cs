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

        [Obsolete("Not Supported")]
        public override void DrawString(SpriteFont Font, string Text, Vector2 position, Color color)
        {
            var Size = Font.MeasureString(Text);
            dynamicPrimitive.AddRectangle(new BoundingRectangle(position.X, position.Y, Size.X, Size.Y), color, 2, null);
        }
    }
}
