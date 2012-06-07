#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Studio.Content;
using Nine.Studio.Controls;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Visualizers
{
    public abstract class GraphicsVisualizer<T> : GraphicsVisualizer<T, T> { }

    public abstract class GraphicsVisualizer<TContent, TRunTime> : Visualizer<TContent>
    {
        public string DisplayName { get; protected set; }
        public TContent Editable { get; private set; }
        public TRunTime Drawable { get; private set; }
        public DrawingSurface Surface { get; private set; }

        public ContentManager Content { get { return Surface.ContentManager; } }
        public GraphicsDevice GraphicsDevice { get { return Surface.GraphicsDevice; } }

        public GraphicsVisualizer()
        {
            DisplayName = GetType().Name;
        }

        protected override object Visualize(TContent targetObject)
        {
            Surface = new DrawingSurface();
            Surface.Draw += (sender, e) =>
            {
                if (Editable == null)
                {
                    Editable = (TContent)targetObject;
                    Drawable = CreateRuntimeObject(e.GraphicsDevice, Editable);
                    LoadContent();
                }
                Draw(e.DeltaTime);
            };
            return Surface;
        }

        protected virtual TRunTime CreateRuntimeObject(GraphicsDevice graphics, TContent content)
        {
            if (typeof(TRunTime) == typeof(TContent))
                return (TRunTime)(object)content;

            return PipelineBuilder.Convert<TContent, TRunTime>(graphics, content);
        }

        protected virtual void LoadContent() { }
        protected abstract void Draw(TimeSpan elapsedTime);
    }
}
