namespace Nine.Animations
{
    using System;

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
        void OnStarted();

        /// <summary>
        /// Stops the animation at the current position.
        /// </summary>
        void OnStopped();

        /// <summary>
        /// Pauses the animation at the current position.
        /// </summary>
        void OnPaused();

        /// <summary>
        /// Resumes the animation from the current position.
        /// </summary>
        void OnResumed();

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