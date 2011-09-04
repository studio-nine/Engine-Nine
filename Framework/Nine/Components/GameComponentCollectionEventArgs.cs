#region File Description
//-----------------------------------------------------------------------------
// GameComponentCollectionEventArgs.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// An event args structure used by the GameCompoenentCollection when components are added or removed.
    /// </summary>
    public class GameComponentCollectionEventArgs : EventArgs
    {
        /// <summary>
        /// The component that was added or removed.
        /// </summary>
        public IGameComponent GameComponent { get; private set; }

        /// <summary>
        /// Creates a new GameComponentCollectionEventArgs.
        /// </summary>
        /// <param name="component">The component that was added or removed.</param>
        public GameComponentCollectionEventArgs(IGameComponent component)
        {
            GameComponent = component;
        }
    }
}
