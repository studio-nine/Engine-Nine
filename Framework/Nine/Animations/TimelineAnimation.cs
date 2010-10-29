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
        /// Gets the elapsed time of this animation.
        /// </summary>
        public TimeSpan ElapsedTime { get; private set; }

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

        private float repeat = float.MaxValue;
        private float repeatCounter = float.MaxValue;

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

            if (position == Duration)
                position = TimeSpan.Zero;

            if (position == ElapsedTime)
                return;

            if (Direction == AnimationDirection.Forward)
            {
                repeatCounter -= (float)((position - ElapsedTime).TotalSeconds / Duration.TotalSeconds);
            }
            else if (StartupDirection == AnimationDirection.Backward)
            {
                if (ElapsedTime == TimeSpan.Zero)
                    repeatCounter += (float)((position - Duration).TotalSeconds / Duration.TotalSeconds);
                else
                    repeatCounter += (float)((position - ElapsedTime).TotalSeconds / Duration.TotalSeconds);
            }

            TimeSpan previousPosition = ElapsedTime;

            ElapsedTime = position;

            OnSeek(position, previousPosition);
        }

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
            repeatCounter = repeat;

            Seek(TimeSpan.Zero);

            base.OnStarted();
        }

        protected override void OnStopped()
        {
            Direction = StartupDirection;
            repeatCounter = repeat;

            Seek(TimeSpan.Zero);

            base.OnStopped();
        }

        public override void Update(GameTime gameTime)
        {
            if (Duration < TimeSpan.Zero)
                throw new ArgumentOutOfRangeException("Duration cannot be negative");

            if (Duration == TimeSpan.Zero || State != AnimationState.Playing)
                return;

            TimeSpan previousPosition = ElapsedTime;

            double increment = gameTime.ElapsedGameTime.TotalSeconds * Speed;

            if (Direction == AnimationDirection.Forward)
            {
                ElapsedTime += TimeSpan.FromSeconds(increment);
            }
            else
            {
                ElapsedTime -= TimeSpan.FromSeconds(increment);
            }

            while (ElapsedTime >= Duration || ElapsedTime < TimeSpan.Zero)
            {
                if (Direction == AnimationDirection.Forward)
                {
                    if (AutoReverse)
                    {
                        Direction = AnimationDirection.Backward;
                        ElapsedTime = Duration - (ElapsedTime - Duration);
                        
                        while (ElapsedTime >= Duration)
                            ElapsedTime -= Duration;

                        OnRepeated();
                    }
                    else
                    {
                        ElapsedTime -= Duration;
                    }
                }
                else
                {
                    if (AutoReverse)
                    {
                        Direction = AnimationDirection.Forward;
                        ElapsedTime = -ElapsedTime;

                        while (ElapsedTime < TimeSpan.Zero)
                            ElapsedTime += Duration;

                        OnRepeated();
                    }
                    else
                    {
                        ElapsedTime = Duration + ElapsedTime;
                    }
                }
            }

            OnSeek(ElapsedTime, previousPosition);

            repeatCounter -= (float)(increment / Duration.TotalSeconds);

            if (repeatCounter <= 0)
            {
                OnCompleted();
                Stop();
            }
        }
    }
}