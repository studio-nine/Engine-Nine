namespace Nine.Graphics.UI.Controls.Shapes
{
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Primitives;

    public class Line : Shape
    {
        public Vector2 P1 { get; set; }
        public Vector2 P2 { get; set; }

        public Line() { }
        public Line(Vector2 P1, Vector2 P2)
        {
            this.P1 = P1;
            this.P2 = P2;
        }

        protected internal override void OnRender(DynamicPrimitive dynamicPrimitive)
        {
            base.OnRender(dynamicPrimitive);
            dynamicPrimitive.AddLine(new Vector3(P1, 0), new Vector3(P2, 0), Stroke.ToColor(), StrokeThickness);
        }
    }
}
