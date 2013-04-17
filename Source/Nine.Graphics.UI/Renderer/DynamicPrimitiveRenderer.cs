namespace Nine.Graphics.UI.Renderer
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Primitives;
    using Nine.Graphics.UI.Media;

    public class DynamicPrimitiveRenderer : IRenderer
    {
        public GraphicsDevice GraphicsDevice { get { return dynamicPrimitive.GraphicsDevice; } }
        public float ElapsedTime { get; set; }

        private DynamicPrimitive dynamicPrimitive;

        public DynamicPrimitiveRenderer(GraphicsDevice graphics)
        {
            dynamicPrimitive = new DynamicPrimitive(graphics);
        }

        public void Begin(DrawingContext context)
        {

        }

        public void End(DrawingContext context)
        {
            dynamicPrimitive.Draw(context, null);
            dynamicPrimitive.Clear();
        }

        public void Draw(BoundingRectangle bound, Brush brush)
        {
            var solidColorBrush = brush as SolidColorBrush;
            if (solidColorBrush != null)
            {
                var color = solidColorBrush.ToColor();
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
                return;
            }

            var imageBrush = brush as ImageBrush;
            if (imageBrush != null)
            {
                var texture = imageBrush.Source;
                // This don't really work, make own math function
                var Scale = Nine.Graphics.UI.Internal.Controls.Viewbox.ComputeScaleFactor(
                        new Vector2(bound.Width, bound.Height), new Vector2(texture.Width, texture.Height), imageBrush.Stretch, imageBrush.StretchDirection);

                SpriteEffects Effects = SpriteEffects.None;
                if (imageBrush.Flip != Flip.None & imageBrush.Flip != Flip.Both)
                    Effects = (SpriteEffects)imageBrush.Flip;
                else if (imageBrush.Flip == Flip.Both)
                    Effects = SpriteEffects.FlipVertically | SpriteEffects.FlipHorizontally;

                //spriteBatch.Draw(texture, new Vector2(bound.X, bound.Y), imageBrush.SourceRectangle, Color.White, 0, Vector2.Zero, Scale, Effects, 0);
                return;
            }

            var gradientBrush = brush as GradientBrush;
            if (gradientBrush != null)
            {
                dynamicPrimitive.BeginPrimitive(PrimitiveType.TriangleList, null, null);
                {
                    dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y, 0), gradientBrush.G1.ToColor());
                    dynamicPrimitive.AddVertex(new Vector3(bound.X, bound.Y + bound.Height, 0), gradientBrush.G3.ToColor());
                    dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y + bound.Height, 0), gradientBrush.G4.ToColor());
                    dynamicPrimitive.AddVertex(new Vector3(bound.X + bound.Width, bound.Y, 0), gradientBrush.G2.ToColor());

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

            var visualBrush = brush as VisualBrush;
            if (visualBrush != null)
            {
                visualBrush.Visual.Measure(new Vector2(bound.Width, bound.Height));
                visualBrush.Visual.Arrange(bound);
                visualBrush.Visual.OnRender(this);
                return;
            }

            throw new NotSupportedException("brush");
        }

        public void Draw(BoundingRectangle bound, Color color)
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

        public void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source)
        {
            this.Draw(texture, bound, Source, Color.White, Flip.None);
        }

        public void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color)
        {
            this.Draw(texture, bound, Source, color, Flip.None);
        }

        public void Draw(Texture2D texture, BoundingRectangle bound, Rectangle? Source, Color color, Flip flip)
        {
            var texCoords = Nine.Graphics.UI.UIExtensions.TextureCoords(flip);
            // TODO: Calculate Source Rectangle with texCoords
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

        public void DrawString(SpriteFont Font, string Text, Vector2 position, Color color)
        {
            throw new NotSupportedException();
        }
    }
}
