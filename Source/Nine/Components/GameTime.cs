#region File Description
//-----------------------------------------------------------------------------
// DrawableGameComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Microsoft.Xna.Framework
{
    /// <summary>
    /// Snapshot of the game timing state expressed in values that can be used by
    /// variable-step (real time) or fixed-step (game time) games.
    /// </summary>
    public class GameTime
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GameTime"/> class.
        /// </summary>
        public GameTime() { }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTime"/> class.
        /// </summary>
        /// <param name="totalGameTime">The total game time.</param>
        /// <param name="elapsedGameTime">The elapsed game time.</param>
        public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime)
        {
            ElapsedGameTime = elapsedGameTime;
            TotalGameTime = totalGameTime;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GameTime"/> class.
        /// </summary>
        /// <param name="totalGameTime">The total game time.</param>
        /// <param name="elapsedGameTime">The elapsed game time.</param>
        /// <param name="isRunningSlowly">if set to <c>true</c> [is running slowly].</param>
        public GameTime(TimeSpan totalGameTime, TimeSpan elapsedGameTime, bool isRunningSlowly)
        {
            ElapsedGameTime = elapsedGameTime;
            TotalGameTime = totalGameTime;
            IsRunningSlowly = isRunningSlowly;
        }

        /// <summary>
        /// The amount of elapsed game time since the last update.
        /// </summary>
        public TimeSpan ElapsedGameTime { get; internal set; }

        /// <summary>
        /// The amount of game time since the start of the game.
        /// </summary>
        public TimeSpan TotalGameTime { get; internal set; }

        /// <summary>
        /// Gets a value indicating that the game loop is taking longer than its TargetElapsedTime.
        /// In this case, the game loop can be considered to be running too slowly and
        /// should do something to "catch up."
        /// </summary>
        public bool IsRunningSlowly { get; internal set; }
    }
}
