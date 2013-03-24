namespace Nine.Animations
{
    using System;

    /// <summary>
    /// Base class for all playable animations.
    /// </summary>
    [Nine.Serialization.BinarySerializable]
    public abstract class Animation : Nine.Object, IUpdateable, IAnimation
    {
        /// <summary>
        /// Gets the current state of the animation.
        /// </summary>
        public AnimationState State { get; private set; }

        /// <summary>
        /// Plays the animation from start position.
        /// </summary>
        void IAnimation.OnStarted()
        {
            State = AnimationState.Playing;
            OnStarted();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        void IAnimation.OnStopped()
        {
            State = AnimationState.Stopped;
            OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        void IAnimation.OnPaused()
        {
            if (State == AnimationState.Playing)
            {
                State = AnimationState.Paused;
                OnPaused();
            }
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        void IAnimation.OnResumed()
        {
            if (State == AnimationState.Paused)
            {
                State = AnimationState.Playing;
                OnResumed();
            }
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public abstract void Update(float elapsedTime);

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected virtual void OnStarted()
        {
            var started = Started;
            if (started != null)
                started(this, EventArgs.Empty);
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected virtual void OnStopped()
        {
            var stopped = Stopped;
            if (stopped != null)
                stopped(this, EventArgs.Empty);
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected virtual void OnPaused()
        {
            var paused = Paused;
            if (paused != null)
                paused(this, EventArgs.Empty);
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected virtual void OnResumed()
        {
            var resumed = Resumed;
            if (resumed != null)
                resumed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Call this in derived classes when this animation has completely finished playing.
        /// </summary>
        protected virtual void OnCompleted()
        {
            State = AnimationState.Stopped;
            OnStopped();

            var completed = Completed;
            if (completed != null)
                completed(this, EventArgs.Empty);
        }

        /// <summary>
        /// Occurs when this animation has completely finished playing.
        /// </summary>
        public event EventHandler Completed;

        /// <summary>
        /// Occurs when this animation has started playing.
        /// </summary>
        public event EventHandler Started;

        /// <summary>
        /// Occurs when this animation has stopped.
        /// </summary>
        public event EventHandler Stopped;

        /// <summary>
        /// Occurs when this animation has been paused.
        /// </summary>
        public event EventHandler Paused;

        /// <summary>
        /// Occurs when this animation has been resumed after pause.
        /// </summary>
        public event EventHandler Resumed;
    }

}