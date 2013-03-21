namespace Nine.Studio.Visualizers
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Content;
    using Nine.Studio.Controls;
    using Nine.Studio.Extensibility;

    public abstract class GameVisualizer<T> : GameVisualizer<T, T> { }

    public abstract class GameVisualizer<TContent, TRunTime> : Game, IVisualizer
    {
        public GameHost GameHost { get; private set; }
        public string DisplayName { get; protected set; }
        public Type TargetType { get { return typeof(TContent); } }

        public TContent Editable { get; internal set; }
        public TRunTime Drawable { get; internal set; }

        public GameVisualizer()
        {
            DisplayName = GetType().Name;
        }

        object IVisualizer.Visualize(object targetObject)
        {
            if (targetObject is TContent)
                return Visualize((TContent)targetObject);
            return null;
        }

        protected virtual object Visualize(TContent targetObject)
        {
            if (GameHost == null)
            {
                GameHost = new GameHost();
                GameHost.Loaded += (sender, e) =>
                {
                    if (GameHost.Game == null)
                    {
                        GameHost.Game = this;
                        GameHost.GameLoaded += (sender1, e1) =>
                        {
                            /* TODO:
                            Content = new Nine.Content.Pipeline.PipelineContentManager(GraphicsDevice);
                            Editable = (TContent)targetObject;
                            Drawable = CreateDrawable(GraphicsDevice, Editable);
                             */
                        };
                    }
                };
            }
            return GameHost;
        }

        protected internal virtual TRunTime CreateDrawable(GraphicsDevice graphics, TContent content)
        {
            if (typeof(TRunTime) == typeof(TContent))
                return (TRunTime)(object)content;

            throw new NotImplementedException();
        }
    }
}
