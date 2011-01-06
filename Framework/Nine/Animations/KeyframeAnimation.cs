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
    /// Defines the behavior of the last ending keyframe.
    /// </summary>
    /// <remarks>
    /// The difference between these behaviors won't be noticeable
    /// unless the KeyframeAnimation is really slow.
    /// </remarks>
    public enum KeyframeEnding
    {
        /// <summary>
        /// The animation will wait for the last frame to finish
        /// but won't blend the last frame with the first frame.
        /// Specify this when your animation isn't looped.
        /// </summary>
        Clamp,

        /// <summary>
        /// The animation will blend between the last keyframe
        /// and the first keyframe. 
        /// Specify this when the animation is looped and the first
        /// frame doesn't equal to the last frame.
        /// </summary>
        Wrap,

        /// <summary>
        /// The animation will stop immediately when it reaches
        /// the last frame, so the ending frame has no duration.
        /// Specify this when the animation is looped and the first
        /// frame is identical to the last frame.
        /// </summary>
        Discard,
    }

    /// <summary>
    /// Event args used by KeyframeAnimation events.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class KeyframeEventArges : EventArgs
    {
        /// <summary>
        /// Gets the index of the frame.
        /// </summary>
        public int Frame { get; internal set; }
    }

    /// <summary>
    /// Basic class for all keyframed animations.
    /// </summary>
    public abstract class KeyframeAnimation : TimelineAnimation
    {
        /// <summary>
        /// Creates a new instance of <c>KeyframeAnimation</c>.
        /// </summary>
        public KeyframeAnimation()
        {
            CurrentFrame = 0;
            FramesPerSecond = 24;
            Repeat = float.MaxValue;
        }

        /// <summary>
        /// Gets or sets number of frames to be played per second.
        /// The default value is 24. You should not change this value except
        /// during initialization. Adjust the playing speed of the animation
        /// using <c>Speed</c> property instead.
        /// </summary>
        public float FramesPerSecond { get; set; }

        /// <summary>
        /// Gets the current frame index been played.
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        /// Gets the total number of frames.
        /// </summary>
        public int TotalFrames { get { return GetTotalFrames(); } }

        /// <summary>
        /// Gets or sets the behavior of the ending keyframe.
        /// The default value is KeyframeEnding.Clamp.
        /// </summary>
        public KeyframeEnding Ending { get; set; }

        /// <summary>
        /// Occurs when this animation has just entered the current frame.
        /// </summary>
        public event EventHandler<KeyframeEventArges> EnterFrame;

        /// <summary>
        /// Occurs when this animation is about to exit the current frame.
        /// </summary>
        public event EventHandler<KeyframeEventArges> ExitFrame;

        /// <summary>
        /// Gets the index of the frame at the specified position.
        /// </summary>
        private int GetFrame(TimeSpan position)
        {
            if (position < TimeSpan.Zero || position > Duration)
                throw new ArgumentOutOfRangeException("position");

            if (position == Duration)
                return Ending == KeyframeEnding.Discard ? TotalFrames - 2 : TotalFrames - 1;

            return (int)(position.TotalSeconds * FramesPerSecond);
        }

        /// <summary>
        /// Positions the animation at the specified frame.
        /// </summary>
        public void Seek(int frame)
        {
            if (frame < 0 || frame >= TotalFrames)
                throw new ArgumentOutOfRangeException("frame");

            Seek(TimeSpan.FromSeconds(1.0 * frame / FramesPerSecond));
        }

        // Use to prevent from allways calling exit frame before enter frame
        // even for the first frame.
        bool hasPlayed = false;

        protected override void OnStopped()
        {
            hasPlayed = false;

            base.OnStopped();
        }

        protected override void OnStarted()
        {
            hasPlayed = false;

            base.OnStarted();
        }

        protected override void OnCompleted()
        {
            hasPlayed = false;

            base.OnCompleted();
        }
        
        protected override sealed void OnSeek(TimeSpan position, TimeSpan previousPosition)
        {
            int current = GetFrame(position);
            int previous = GetFrame(previousPosition);

            float percentage = (float)((Position.TotalSeconds * FramesPerSecond - current));

            if (Ending == KeyframeEnding.Wrap)
                OnSeek(current, (current + 1) % TotalFrames, percentage);
            else if (Ending == KeyframeEnding.Clamp || Ending == KeyframeEnding.Discard)
                OnSeek(current, Math.Min(current + 1, TotalFrames - 1), percentage);

            if (current != previous || !hasPlayed)
            {
                if (hasPlayed)
                {
                    OnExitFrame(previous);
                }

                hasPlayed = true;

                CurrentFrame = current;
                OnEnterFrame(current);
            }
        }

        protected sealed override TimeSpan GetDuration()
        {
            int realFrames = Ending == KeyframeEnding.Discard ? (TotalFrames - 1) : TotalFrames;
            return TimeSpan.FromSeconds( realFrames / FramesPerSecond);
        }

        /// <summary>
        /// When overriden, returns the total number of keyframes.
        /// </summary>
        protected abstract int GetTotalFrames();

        /// <summary>
        /// Moves the animation at the position between start frame and end frame
        /// specified by percentage.
        /// </summary>
        protected virtual void OnSeek(int startFrame, int endFrame, float percentage) { }
        
        protected virtual void OnEnterFrame(int frame)
        {
            if (EnterFrame != null)
                EnterFrame(this, new KeyframeEventArges() { Frame = frame });
        }

        protected virtual void OnExitFrame(int frame)
        {
            if (ExitFrame != null)
                ExitFrame(this, new KeyframeEventArges() { Frame = frame });
        }
    }
}