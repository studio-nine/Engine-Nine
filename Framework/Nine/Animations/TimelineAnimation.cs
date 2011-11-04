#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
    /// Defines whether the animation is playing forward or backward.
    /// </summary>
    public enum AnimationDirection
    {
        /// <summary>
        /// The animation is playing forward.
        /// </summary>
        Forward,

        /// <summary>
        /// The animation is playing backward.
        /// </summary>
        Backward,
    }

    /// <summary>
    /// Basic class for all timeline based animations.
    /// </summary>
    public abstract class TimelineAnimation : Animation, ITimelineAnimation
    {
        /// <summary>
        /// Creates a new instance of <c>TimelineAnimation</c>.
        /// </summary>
        protected TimelineAnimation()
        {
            ElapsedTime = TimeSpan.Zero;
            StartupDirection = AnimationDirection.Forward;
        }

        #region ITimelineAnimation Members

        /// <summary>
        /// Gets or set the running time for one run of a timeline animation, excluding repeats.
        /// </summary>
        public TimeSpan Duration { get { return DurationValue; } }

        /// <summary>
        /// When overriden, returns the duration of this animation.
        /// </summary>
        protected abstract TimeSpan DurationValue { get; }

        /// <summary>
        /// Gets the position of the animation as an elapsed time since the begin point.
        /// Counts up if the direction is Forward, down if Backward.
        /// </summary>
        public TimeSpan Position { get; private set; }

        /// <summary>
        /// Gets or sets the playing speed of this animation.
        /// </summary>
        public float Speed
        {
            get { return _speed; }
            set
            {
                if (value <= 0)
                    throw new InvalidOperationException("Speed must be greater than zero.");
                _speed = value;
            }
        }
        private float _speed = 1.0f;

        #endregion

        /// <summary>
        /// Gets the elapsed time since the animation started playing.
        /// Accumulates on each update, and updated by seek to ensure the animation
        /// stops at the right time.
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// Gets whether this animation should play backwards after it reaches the end.
        /// Takes effect when an animation would otherwise complete.
        /// </summary>
        public bool AutoReverse { get; set; }

        /// <summary>
        /// Gets or sets whether the animation is playing forward or backward on startup.
        /// Takes effect only when <c>Play</c> is called.
        /// </summary>
        public AnimationDirection StartupDirection { get; set; }

        /// <summary>
        /// Gets or set whether the animation is currently playing forward or backward.
        /// Takes effect on an animation that is playing or paused.
        /// </summary>
        public AnimationDirection Direction { get; set; }

        /// <summary>
        /// Gets or sets the number of times this animation will be played.
        /// When set to a fractional value, the animation will be stopped and completed part way.
        /// Float.MaxValue means forever. The default value is 1.
        /// </summary>
        public float Repeat
        {
            get { return _repeat; }
            set
            {
                if (value <= 0)
                    throw new InvalidOperationException("Repeat must be greater than zero.");
                _repeat = value;
            }
        }
        private float _repeat = 1;

        /// <summary>
        /// Occurs when this animation has reached the end and has just repeated.
        /// </summary>
        public event EventHandler Repeated;

        /// <summary>
        /// Positions the animation at the specified fraction of <c>Duration</c>.
        /// Takes effect on an animation that is playing or paused.
        /// </summary>
        public void Seek(float fraction)
        {
            Seek(TimeSpan.FromSeconds(Duration.TotalSeconds * fraction));
        }

        /// <summary>
        /// Positions the animation at the specified time value between 0 and Duration.
        /// Takes effect on an animation that is playing or paused.
        /// Adjusts elapsed time, so that animation will stop on time.
        /// </summary>
        public void Seek(TimeSpan position)
        {
            if (position < TimeSpan.Zero || position > Duration)
                throw new ArgumentOutOfRangeException("position");
            if (position == Position)
                return;
            TimeSpan previousPosition = Position;
            Position = position;
            ElapsedTime += (Direction == AnimationDirection.Forward) ? Position - previousPosition : previousPosition - Position;
            OnSeek(Position, previousPosition);
        }

        /// <summary>
        /// When overridden, positions the animation at the specified location.
        /// </summary>
        protected abstract void OnSeek(TimeSpan position, TimeSpan previousPosition);

        protected virtual void OnRepeated()
        {
            if (Repeated != null)
                Repeated(this, EventArgs.Empty);
        }

        protected override void OnStarted()
        {
            Direction = StartupDirection;
            ElapsedTime = TimeSpan.Zero;
            Position = (Direction == AnimationDirection.Forward) ? TimeSpan.Zero : Duration;
            OnSeek(Position, Position);
            base.OnStarted();
        }

        /// <summary>
        /// Update the animation by a specified amount of elapsed time.
        /// Handle playing either forwards or backwards.
        /// Determines whether animation should terminate or continue.
        /// Signals related events.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public override void Update(TimeSpan elapsedTime)
        {
            if (State != AnimationState.Playing)
                return;
            if (Duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Update: elapsed time cannot be negative");
            TimeSpan increment = TimeSpan.FromTicks((long)(elapsedTime.Ticks * (double)Speed));

            // Repeat sets an absolute upper bound on how long the animation may run
            TimeSpan max_elapsed = TimeSpan.MaxValue;
            double maxSeconds = Repeat * Duration.TotalSeconds;
            if (max_elapsed.TotalSeconds > maxSeconds)
                max_elapsed = TimeSpan.FromSeconds(maxSeconds);

            if (ElapsedTime + increment > max_elapsed)
                increment = max_elapsed - ElapsedTime;

            // Loop to handle reverse or repeat without losing excess time.
            while (increment > TimeSpan.Zero && ElapsedTime < max_elapsed)
            {
                // Update, but only by enough to stay within the allowed range
                // Notify new position
                TimeSpan previousPosition = Position;
                if (Direction == AnimationDirection.Forward)
                    Position = (Position + increment > Duration) ? Duration : Position + increment;
                else
                    Position = (Position - increment < TimeSpan.Zero) ? TimeSpan.Zero : Position - increment;
                var part_inc = (Position - previousPosition).Duration();
                ElapsedTime += part_inc;
                increment -= part_inc;
                OnSeek(Position, previousPosition);

                // If time left and not complete, then reverse or repeat and notify that too.
                if (increment > TimeSpan.Zero && ElapsedTime < max_elapsed)
                {
                    if (AutoReverse)
                        Direction = (Direction == AnimationDirection.Forward) ? AnimationDirection.Backward : AnimationDirection.Forward;
                    else
                    {
                        previousPosition = Position;
                        Position = (Direction == AnimationDirection.Forward) ? TimeSpan.Zero : Duration;
                        OnSeek(Position, previousPosition);
                    }
                    OnRepeated();
                }
            }
            if (!(ElapsedTime < max_elapsed))
            {
                Stop();
                OnCompleted();
            }
        }
    }
}