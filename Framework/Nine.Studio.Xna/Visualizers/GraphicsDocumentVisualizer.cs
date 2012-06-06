#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Nine.Studio.Controls;
using Nine.Studio.Content;
using Nine.Studio.Extensibility;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Studio.Visualizers
{
    public abstract class GraphicsDocumentVisualizer<T> : GraphicsDocumentVisualizer<T, T> { }

    public abstract class GraphicsDocumentVisualizer<TContent, TRunTime> : IDocumentVisualizer
    {
        public TContent Editable { get; private set; }
        public TRunTime Drawable { get; private set; }
        public DrawingSurface Surface { get; private set; }

        public Type TargetType { get { return typeof(TContent); } }
        public ContentManager Content { get { return Surface.ContentManager; } }
        public GraphicsDevice GraphicsDevice { get { return Surface.GraphicsDevice; } }

        public virtual object Visualize(object targetObject)
        {
            if (targetObject is TContent)
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
            return null;
        }

        protected virtual TRunTime CreateRuntimeObject(GraphicsDevice graphics, TContent content)
        {
            if (typeof(TRunTime) == typeof(TContent))
                return (TRunTime)(object)content;

            PipelineBuilder<TContent> builder = new PipelineBuilder<TContent>();
            return builder.BuildAndLoad<TRunTime>(graphics, content);
        }

        protected virtual void LoadContent() { }
        protected abstract void Draw(TimeSpan elapsedTime);
    }
}
