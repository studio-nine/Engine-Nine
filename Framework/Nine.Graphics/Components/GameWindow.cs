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
        /// <summary>
        /// Gets the handle to the system window.
        /// This value will always be IntPtr.Zero on Silverlight.
        /// </summary>
        public IntPtr Handle { get { return IntPtr.Zero; } }
    }
}