namespace Nine.Graphics.UI
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    
    /// <summary>
    /// A Pass for drawing Scene's inside a element.
    /// </summary>
    public class ScenePass : Pass, IGraphicsObject, IPostEffect
    {
        public Scene Scene { get; set; }


        public Texture2D InputTexture
        {
            get { return output; }
            set { output = value; }
        }
        private Texture2D output;

        public SurfaceFormat? InputFormat
        {
            get { return null; }
        }


        public ScenePass()
            : this(null)
        {

        }

        public ScenePass(Scene scene)
        {
            this.Scene = scene;
        }

        public override void Draw(DrawingContext context, System.Collections.Generic.IList<IDrawableObject> drawables)
        {
            if (Scene != null)
            {
                Scene.Draw(context.GraphicsDevice, context.ElapsedTime);
            }
        }

        void IGraphicsObject.OnAdded(DrawingContext context)
        {
            context.Passes.Add(this);
            AddDependency(context.MainPass);
        }

        void IGraphicsObject.OnRemoved(DrawingContext context)
        {
            context.Passes.Remove(this);
        }

    }
}
