#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Animations
{
    /// <summary>
    /// Current state (playing, paused, or stopped) of an animation.
    /// </summary>
    public enum AnimationState
    {
        /// <summary>
        /// The animation is stopped.
        /// </summary>
        Stopped,

        /// <summary>
        /// The animation is playing.
        /// </summary>
        Playing,

        /// <summary>
        /// The animation is paused.
        /// </summary>
        Paused,
    }

    /// <summary>
    /// Interface for animation playback.
    /// </summary>
    public interface IAnimation
    {
        /// <summary>
        /// Gets the current state of the animation.
        /// </summary>
        AnimationState State { get; }

        /// <summary>
        /// Plays the animation from start position.
        /// </summary>
        void Play();

        /// <summary>
        /// Stops the animation and goto start position.
        /// </summary>
        void Stop();

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        void Pause();

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        void Resume();

        /// <summary>
        /// Occurs when this clock has completely finished playing.
        /// </summary>
        event EventHandler Completed;
    }
}