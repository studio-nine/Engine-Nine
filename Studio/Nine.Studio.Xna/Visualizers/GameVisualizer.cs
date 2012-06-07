#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Studio.Content;
using Nine.Studio.Controls;
using Nine.Studio.Extensibility;
#endregion

namespace Nine.Studio.Visualizers
{
    public abstract class GameVisualizer<T> : GameVisualizer<T, T> { }

    public abstract class GameVisualizer<TContent, TRunTime> : Game, IVisualizer
    {
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
            GameHost gameHost = new GameHost();
            gameHost.Loaded += (sender, e) =>
            {
                if (gameHost.Game == null)
                {
                    gameHost.Game = this;
                    gameHost.GameLoaded += (sender1, e1) =>
                    {
                        Content = new PipelineContentManager(GraphicsDevice);
                        Editable = (TContent)targetObject;
                        Drawable = CreateDrawable(GraphicsDevice, Editable);
                    };
                }
            };
            return gameHost;
        }

        protected internal virtual TRunTime CreateDrawable(GraphicsDevice graphics, TContent content)
        {
            if (typeof(TRunTime) == typeof(TContent))
                return (TRunTime)(object)content;

            return PipelineBuilder.Convert<TContent, TRunTime>(graphics, content);
        }
    }
}
