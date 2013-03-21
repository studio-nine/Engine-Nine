#region File Description
//-----------------------------------------------------------------------------
// IUpdateable.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines the interface for a game component that can be updated.
    /// </summary>
    public interface IUpdateable
    {
        /// <summary>
        /// Gets whether the component should be updated.
        /// </summary>
        bool Enabled { get; }

        /// <summary>
        /// Gets the order in which the component should be updated. 
        /// Components with smaller values are updated first.
        /// </summary>
        int UpdateOrder { get; }

        /// <summary>
        /// Invoked when the Enabled property changes.
        /// </summary>
        event EventHandler<EventArgs> EnabledChanged;

        /// <summary>
        /// Invoked when the UpdateOrder property changes.
        /// </summary>
        event EventHandler<EventArgs> UpdateOrderChanged;

        /// <summary>
        /// Updates the component.
        /// </summary>
        void Update(GameTime gameTime);
    }
}
