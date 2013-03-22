namespace Nine.Graphics.UI.Controls.Shapes
{
    using Nine.Graphics.UI.Media;

    public class Rectangle : Shape
    {
        protected internal override void OnRender(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch)
        {
            spriteBatch.Draw(RenderTransform, Fill.Color);
        }
    }
}
