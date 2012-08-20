namespace Nine.Graphics
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Animations;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;

    public class Sprite : Transformable, IDrawableObject, ISceneObject
    {
        public bool Visible { get; set; }
        public Material Material { get; set; }

        public Color Color { get; set; }

        public bool FlipX { get; set; }
        public bool FlipY { get; set; }

        public Vector2 Position { get; set; }
        public Vector2 Scale { get; set; }
        public float Rotation { get; set; }

        public Texture2D Texture { get; set; }
        public Rectangle SourceRectangle { get; set; }

        public AnimationPlayer Animations { get; private set; }

        public void Draw(DrawingContext context, Material material)
        {
        }

        void IDrawableObject.OnAddedToView(DrawingContext context) { }
        void ISceneObject.OnAdded(DrawingContext context) { }
        void ISceneObject.OnRemoved(DrawingContext context) { }
    }
}
