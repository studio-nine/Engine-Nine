#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//  oct 2011 -- dmb -- rework to fix some bugs and handle reversing better.
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
        /// Stops the animation.
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
        /// Occurs when this animation has completely finished playing.
        /// </summary>
        event EventHandler Completed;
    }

    /// <summary>
    /// Interface for timeline based animation.
    /// </summary>
    public interface ITimelineAnimation : IAnimation
    {
        /// <summary>
        /// Gets or sets the running time for a timeline animation (excluding repeats).
        /// </summary>
        TimeSpan Duration { get; }

        /// <summary>
        /// Gets the position of the animation as an elapsed time since the start point.
        /// Counts up if the direction is Forward, down if Backward.
        /// </summary>
        TimeSpan Position { get; }

        /// <summary>
        /// Gets or sets the playing speed of the timeline animation.
        /// Multiplies the number of clock ticks on each update.
        /// </summary>
        float Speed { get; set; }
    }
}