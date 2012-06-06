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
using Nine.Studio.Extensibility;
using System.ComponentModel;
using Nine.Studio.Controls;
using Nine.Studio.Content;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Studio.Visualizers
{
    public abstract class GameVisualizer<T> : GameVisualizer<T, T> { }

    public abstract class GameVisualizer<TContent, TRunTime> : IDocumentVisualizer
    {
        public Type TargetType { get { return typeof(TContent); } }

        public virtual object Visualize(object targetObject)
        {
            if (targetObject is TContent)
            {
                GameHost gameHost = new GameHost();
                gameHost.Loaded += (sender, e) =>
                {
                    if (gameHost.Game == null)
                    {
                        EditorGame<TContent, TRunTime> editorGame = CreateGame();
                        gameHost.Game = editorGame;
                        gameHost.GameLoaded += (sender1, e1) =>
                        {
                            editorGame.Editable = (TContent)targetObject;
                            editorGame.Drawable = editorGame.CreateRuntimeObject(editorGame.GraphicsDevice, editorGame.Editable);
                        };
                    }
                };
                return gameHost;
            }
            return null;
        }

        protected abstract EditorGame<TContent, TRunTime> CreateGame();
    }
}
