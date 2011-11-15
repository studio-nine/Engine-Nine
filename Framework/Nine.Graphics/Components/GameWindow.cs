#region File Description
//-----------------------------------------------------------------------------
// GameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Graphics;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// The system window associated with a Game.
    /// </summary>
    public class GameWindow
    {
        Game game;

        /// <summary>
        /// Gets the handle to the system window.
        /// This value will always be IntPtr.Zero on Silverlight.
        /// </summary>
        public IntPtr Handle { get { return IntPtr.Zero; } }

        public event EventHandler<EventArgs> ClientSizeChanged;

        internal GameWindow(Game game)
        {
            this.game = game;
            game.SizeChanged += new SizeChangedEventHandler(game_SizeChanged);
        }

        void game_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (ClientSizeChanged != null)
                ClientSizeChanged(this, EventArgs.Empty);
        }
    }
}