namespace Nine.Animations
{
    using System;
    using System.ComponentModel;
    
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
    public class KeyframeEventArgs : EventArgs
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
        /// Gets or sets the inclusive frame at which this <see cref="KeyframeAnimation"/> should begin.
        /// </summary>
        public int? BeginFrame
        {
            get { return beginFrame; }
            set { beginFrame = value; UpdateBeginEndTime(); }
        }
        private int? beginFrame;

        /// <summary>
        /// Gets or sets the exclusive frame at which this <see cref="KeyframeAnimation"/> should end.
        /// </summary>
        public int? EndFrame
        {
            get { return endFrame; }
            set { endFrame = value; UpdateBeginEndTime(); }
        }
        private int? endFrame;

        /// <summary>
        /// Gets or sets number of frames to be played per second.
        /// The default value is 24. 
        /// </summary>
        public float FramesPerSecond 
        {
            get { return framesPerSecond; }
            set { framesPerSecond = value; UpdateDuration(); }
        }
        private float framesPerSecond = 24;

        /// <summary>
        /// Gets the current frame index been played.
        /// </summary>
        public int CurrentFrame { get; private set; }

        /// <summary>
        /// Gets the total number of frames.
        /// </summary>
        public int TotalFrames
        {
            get { return totalFrames; }
            protected set { totalFrames = value; UpdateDuration(); UpdateBeginEndTime(); }
        }
        private int totalFrames;

        /// <summary>
        /// Gets or sets the behavior of the ending keyframe.
        /// The default value is KeyframeEnding.Clamp.
        /// </summary>
        public KeyframeEnding Ending
        {
            get { return ending; }
            set { ending = value; UpdateDuration(); }
        }
        private KeyframeEnding ending = KeyframeEnding.Clamp;

        /// <summary>
        /// Occurs when this animation has just entered the current frame.
        /// </summary>
        public event EventHandler<KeyframeEventArgs> EnterFrame;

        /// <summary>
        /// Occurs when this animation is about to exit the current frame.
        /// </summary>
        public event EventHandler<KeyframeEventArgs> ExitFrame;

        /// <summary>
        /// Creates a new instance of <c>KeyframeAnimation</c>.
        /// </summary>
        protected KeyframeAnimation()
        {
            CurrentFrame = 0;
            Repeat = float.MaxValue;
        }

        /// <summary>
        /// Gets the index of the frame at the specified position.
        /// </summary>
        private void GetFrame(TimeSpan position, out int frame, out float percentage)
        {
            if (position < TimeSpan.Zero || position > TotalDuration)
                throw new ArgumentOutOfRangeException("position");

            frame = (int)(position.TotalSeconds * framesPerSecond);
            percentage = (float)(position.TotalSeconds * framesPerSecond - frame);
            
            if (frame >= totalFrames)
            {
                frame = 0;
                percentage = 0;
            }
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

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected override void OnStopped()
        {
            hasPlayed = false;
            base.OnStopped();
        }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            // Override BeginTime & EndTime with BeginFrame & EndFrame.
            UpdateBeginEndTime();
            hasPlayed = false;
            base.OnStarted();
        }

        /// <summary>
        /// Called when the animation is completed.
        /// </summary>
        protected override void OnCompleted()
        {
            hasPlayed = false;
            base.OnCompleted();
        }

        /// <summary>
        /// When overridden, positions the animation at the specified location.
        /// </summary>
        protected override sealed void OnSeek(TimeSpan position, TimeSpan previousPosition)
        {
            float percentage;
            int current, previous;

            GetFrame(previousPosition, out previous, out percentage);
            GetFrame(position, out current, out percentage);

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
        
        private void UpdateDuration()
        {
            int realFrames = Math.Max(0, (ending == KeyframeEnding.Discard ? (totalFrames - 1) : totalFrames));
            TotalDuration = TimeSpan.FromSeconds(realFrames / FramesPerSecond);
        }

        private void UpdateBeginEndTime()
        {
            if (beginFrame.HasValue && totalFrames > 0)
                BeginTime = TimeSpan.FromSeconds(beginFrame.Value * TotalDuration.TotalSeconds / totalFrames);
            else
                BeginTime = null;

            if (endFrame.HasValue && totalFrames > 0)
                EndTime = TimeSpan.FromSeconds(endFrame.Value * TotalDuration.TotalSeconds / totalFrames);
            else
                EndTime = null;
        }

        /// <summary>
        /// Moves the animation at the position between start frame and end frame
        /// specified by percentage.
        /// </summary>
        protected virtual void OnSeek(int startFrame, int endFrame, float percentage) { }

        /// <summary>
        /// Called when the specified frame is entered.
        /// </summary>
        protected virtual void OnEnterFrame(int frame)
        {
            if (EnterFrame != null)
                EnterFrame(this, new KeyframeEventArgs() { Frame = frame });
        }
        
        /// <summary>
        /// Called when the specified frame is exit.
        /// </summary>
        protected virtual void OnExitFrame(int frame)
        {
            if (ExitFrame != null)
                ExitFrame(this, new KeyframeEventArgs() { Frame = frame });
        }
    }
}