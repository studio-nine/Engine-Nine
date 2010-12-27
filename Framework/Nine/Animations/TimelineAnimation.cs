#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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
    public abstract class TimelineAnimation : Animation
    {
        /// <summary>
        /// Creates a new instance of <c>TimelineAnimation</c>.
        /// </summary>
        public TimelineAnimation()
        {
            ElapsedTime = TimeSpan.Zero;
            StartupDirection = AnimationDirection.Forward;
        }

        /// <summary>
        /// Gets the elapsed time since the playing of this animation.
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

        /// <summary>
        /// Gets the elapsed time since the beginning of this animation.
        /// </summary>
        public TimeSpan Position
        {
            get
            {
                TimeSpan result = undirectedPosition;
                if (Direction == AnimationDirection.Backward)
                {
                    result = Duration - undirectedPosition;

                    if (result == Duration)
                        result = result - TimeSpanEpsilon;
                }
                return result;
            }
        }

        TimeSpan undirectedPosition;

        /// <summary>
        /// Gets the smallest TimeSpan greater then TimeSpan.Zero.
        /// </summary>
        internal static TimeSpan TimeSpanEpsilon = TimeSpan.FromTicks(1);

        /// <summary>
        /// Gets the total length of the animation without been affected
        /// by <c>Speed</c> factor.
        /// </summary>
        public TimeSpan Duration { get { return GetDuration(); } }

        /// <summary>
        /// When implemented, returns the duration of this animation.
        /// </summary>
        protected abstract TimeSpan GetDuration();

        /// <summary>
        /// Gets or sets the playing speed of this animation.
        /// </summary>
        public float Speed
        {
            get { return speed; }
            set
            {
                if (value <= 0)
                    throw new InvalidOperationException("Speed must be greater then zero.");
                speed = value;
            }
        }

        private float speed = 1.0f;

        /// <summary>
        /// Gets whether this animation should play backwards after it reaches the end.
        /// Set this value before <c>Play</c> is called.
        /// </summary>
        public bool AutoReverse { get; set; }

        /// <summary>
        /// Gets or sets whether the animation is playing forward or backward on startup.
        /// Set this value before <c>Play</c> is called.
        /// </summary>
        public AnimationDirection StartupDirection { get; set; }

        /// <summary>
        /// Gets whether the animation is currently playing forward or backward.
        /// </summary>
        public AnimationDirection Direction { get; private set; }

        /// <summary>
        /// Gets or sets number of times this animation will be played.
        /// When this value is set to fractional, the animation will be
        /// stopped and completed halfway between the start and end.
        /// The default value float.MaxValue means forever.
        /// Set this value before <c>Play</c> is called.
        /// </summary>
        public float Repeat
        {
            get { return repeat; }
            set
            {
                if (value <= 0)
                    throw new InvalidOperationException("Speed must be greater then zero.");

                repeat = value;
            }
        }

        private float repeat = 1;
        private TimeSpan targetElapsedTime = TimeSpan.Zero;

        /// <summary>
        /// Occurs when this animation has completely finished playing.
        /// </summary>
        public override event EventHandler Completed;

        /// <summary>
        /// Occurs when this animation has reached the end and repeated.
        /// </summary>
        public event EventHandler Repeated;

        /// <summary>
        /// Positions the animation at the specified value.
        /// </summary>
        public void Seek(TimeSpan position)
        {
            if (position < TimeSpan.Zero || position > Duration)
                throw new ArgumentOutOfRangeException("position");

            if (position == undirectedPosition)
                return;

            TimeSpan previousPosition = Position;

            undirectedPosition = position;
            if (Direction == AnimationDirection.Backward)
            {
                undirectedPosition = Duration - undirectedPosition;
            }

            TimeSpan increment = Position - previousPosition;

            if (Direction == AnimationDirection.Forward)
                targetElapsedTime -= increment;
            else
                targetElapsedTime += increment;

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

        protected virtual void OnCompleted()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        protected override void OnStarted()
        {
            Direction = StartupDirection;
            ElapsedTime = TimeSpan.Zero;
            undirectedPosition = TimeSpan.Zero;

            targetElapsedTime = TimeSpan.FromSeconds(Math.Min
            (
                repeat * Duration.TotalSeconds,
                TimeSpan.MaxValue.TotalSeconds * 0.9
            ));

            Seek(TimeSpan.Zero);

            base.OnStarted();
        }

        protected override void OnStopped()
        {
            base.OnStopped();
        }

        public override void Update(GameTime gameTime)
        {
            if (Duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Duration cannot be negative");

            if (State != AnimationState.Playing)
                return;

            if (Duration == TimeSpan.Zero)
            {
                Stop();
                OnCompleted();
                return;
            }

            TimeSpan previousPosition = Position;
            TimeSpan increment = TimeSpan.FromTicks((long)(gameTime.ElapsedGameTime.Ticks * (double)Speed));

            if (increment == TimeSpan.Zero)
                return;

            ElapsedTime += increment;
            undirectedPosition += increment;

            while (undirectedPosition >= Duration)
            {
                undirectedPosition -= Duration;
                if (AutoReverse)
                {
                    if (Direction == AnimationDirection.Backward)
                        Direction = AnimationDirection.Forward;
                    else
                        Direction = AnimationDirection.Backward;
                }
                OnRepeated();
            }

            if (ElapsedTime >= targetElapsedTime)
            {
                undirectedPosition -= (ElapsedTime - targetElapsedTime);
                ElapsedTime = targetElapsedTime;

                OnSeek(Duration, previousPosition);
                                
                Stop();
                OnCompleted();
                return;
            }

            OnSeek(Position, previousPosition);
        }
    }
}