#region File Description
//-----------------------------------------------------------------------------
// IGameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Defines the interface for a basic game component.
    /// </summary>
    public interface IGameComponent
    {
        /// <summary>
        /// Initializes the component.
        /// </summary>
        void Initialize();
    }
}
