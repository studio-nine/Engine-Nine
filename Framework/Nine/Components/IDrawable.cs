#region File Description
//-----------------------------------------------------------------------------
// IDrawable.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines the interface for a game component that can be drawn.
    /// </summary>
    public interface IDrawable
    {
        /// <summary>
        /// Gets whether the component should be drawn.
        /// </summary>
        bool Visible { get; }

        /// <summary>
        /// Gets the order in which the component should be drawn. 
        /// Components with smaller values are drawn first.
        /// </summary>
        int DrawOrder { get; }

        /// <summary>
        /// Invoked when the Visible property changes.
        /// </summary>
        event EventHandler<EventArgs> VisibleChanged;

        /// <summary>
        /// Invoked when the DrawOrder property changes.
        /// </summary>
        event EventHandler<EventArgs> DrawOrderChanged;

        /// <summary>
        /// Draws the component.
        /// </summary>
        void Draw(GameTime gameTime);
    }
}
