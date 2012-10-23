namespace Nine.Studio.Visualizers
{
    using System;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Studio.Controls;
    using Nine.Studio.Extensibility;

    public abstract class GraphicsVisualizer<T> : GraphicsVisualizer<T, T> { }

    public abstract class GraphicsVisualizer<TContent, TRunTime> : Visualizer<TContent>
    {
        static DrawingSurface sharedDrawingSurface;

        public string DisplayName { get; protected set; }
        public TContent Editable { get; private set; }
        public TRunTime Drawable { get; private set; }
        public DrawingSurface Surface { get { return sharedDrawingSurface; } }

        public ContentManager Content { get { return Surface.ContentManager; } }
        public GraphicsDevice GraphicsDevice { get { return Surface.GraphicsDevice; } }

        public GraphicsVisualizer()
        {
            DisplayName = GetType().Name;
        }

        protected override object Visualize(TContent targetObject)
        {
            if (sharedDrawingSurface == null)
            {
                sharedDrawingSurface = new DrawingSurface();
                sharedDrawingSurface.Draw += (sender, e) =>
                {
                    if (Editable == null)
                    {
                        Editable = (TContent)targetObject;
                        Drawable = CreateRuntimeObject(e.GraphicsDevice, Editable);
                        LoadContent();
                    }
                    Draw(e.DeltaTime);
                };
            }
            return sharedDrawingSurface;
        }

        protected virtual TRunTime CreateRuntimeObject(GraphicsDevice graphics, TContent content)
        {
            if (typeof(TRunTime) == typeof(TContent))
                return (TRunTime)(object)content;

            throw new NotImplementedException();
        }

        protected virtual void LoadContent() { }
        protected abstract void Draw(float elapsedTime);
    }
}
