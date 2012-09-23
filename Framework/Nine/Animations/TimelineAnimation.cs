namespace Nine.Animations
{
    using System;

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

        /// <summary>
        /// Gets the total duration of the timeline animation without been trimmed by StartTime and EndTime.
        /// </summary>
        public TimeSpan TotalDuration
        {
            get { return totalDuration; }

            protected set
            {
                if (value < TimeSpan.Zero)
                    throw new ArgumentOutOfRangeException("TotalDuration cannot be negative");
                totalDuration = value; 
                durationNeedsUpdate = true;
            }
        }
        private TimeSpan totalDuration;

        /// <summary>
        /// Gets or set the running time for one run of a timeline animation, excluding repeats.
        /// </summary>
        public TimeSpan Duration 
        {
            get
            {
                if (durationNeedsUpdate)
                {
                    duration = GetEndPosition() - GetBeginPosition();
                    durationNeedsUpdate = false;
                }
                return duration;
            }
        }
        private bool durationNeedsUpdate;
        private TimeSpan duration;

        /// <summary>
        /// Gets or sets the time at which this <see cref="TimelineAnimation"/> should begin.
        /// </summary>
        public TimeSpan? BeginTime
        {
            get { return beginTime; }
            set { beginTime = value; durationNeedsUpdate = true; }
        }
        private TimeSpan? beginTime;

        /// <summary>
        /// Gets or sets the time at which this <see cref="TimelineAnimation"/> should end.
        /// </summary>
        public TimeSpan? EndTime
        {
            get { return endTime; }
            set { endTime = value; durationNeedsUpdate = true; }
        }
        private TimeSpan? endTime;

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
        /// Gets the position of the animation as an elapsed time since the begin point.
        /// Counts up if the direction is Forward, down if Backward.
        /// </summary>
        public TimeSpan Position
        {
            get { return position; }
            set { Seek(Position); }
        }
        private TimeSpan position = TimeSpan.Zero;

        /// <summary>
        /// Occurs when this animation has reached the end and has just repeated.
        /// </summary>
        public event EventHandler Repeated;

        /// <summary>
        /// Positions the animation at the specified fraction of <c>Duration</c>.
        /// Takes effect on an animation that is playing or paused.
        /// </summary>
        public void Seek(float percentage)
        {
            Seek(TimeSpan.FromSeconds(Duration.TotalSeconds * percentage));
        }

        /// <summary>
        /// Positions the animation at the specified time value between 0 and Duration.
        /// Takes effect on an animation that is playing or paused.
        /// Adjusts elapsed time, so that animation will stop on time.
        /// </summary>
        public void Seek(TimeSpan position)
        {
            if (position < TimeSpan.Zero || position > TotalDuration)
                throw new ArgumentOutOfRangeException("position");
            if (position == Position)
                return;
            TimeSpan previousPosition = Position;
            this.position = position;
            ElapsedTime += (Direction == AnimationDirection.Forward) ? Position - previousPosition : previousPosition - Position;
            OnSeek(this.position, previousPosition);
        }

        /// <summary>
        /// When overridden, positions the animation at the specified location.
        /// </summary>
        protected abstract void OnSeek(TimeSpan position, TimeSpan previousPosition);

        /// <summary>
        /// Called when this animation is repeated.
        /// </summary>
        protected virtual void OnRepeated()
        {
            if (Repeated != null)
                Repeated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            Direction = StartupDirection;
            ElapsedTime = TimeSpan.Zero;
            this.position = (Direction == AnimationDirection.Forward) ? GetBeginPosition() : GetEndPosition();
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

            TimeSpan increment = TimeSpan.FromTicks((long)(elapsedTime.Ticks * (double)Speed));

            // Repeat sets an absolute upper bound on how long the animation may run
            TimeSpan max_elapsed = TimeSpan.MaxValue;
            double maxSeconds = Repeat * Duration.TotalSeconds;
            if (max_elapsed.TotalSeconds > maxSeconds)
                max_elapsed = TimeSpan.FromSeconds(maxSeconds);

            if (ElapsedTime + increment > max_elapsed)
                increment = max_elapsed - ElapsedTime;

            var beginPosition = GetBeginPosition();
            var endPosition = GetEndPosition();

            // Loop to handle reverse or repeat without losing excess time.
            while (increment > TimeSpan.Zero && ElapsedTime < max_elapsed)
            {
                // Update, but only by enough to stay within the allowed range
                // Notify new position
                TimeSpan previousPosition = Position;
                if (Direction == AnimationDirection.Forward)
                    position = (position + increment > endPosition) ? endPosition : position + increment;
                else
                    position = (position - increment < beginPosition) ? beginPosition : position - increment;
                
                var part_inc = (position - previousPosition).Duration();
                ElapsedTime += part_inc;
                increment -= part_inc;

                OnSeek(this.position, previousPosition);

                // If time left and not complete, then reverse or repeat and notify that too.
                if (increment > TimeSpan.Zero && ElapsedTime < max_elapsed)
                {
                    if (AutoReverse)
                    {
                        Direction = (Direction == AnimationDirection.Forward) ? AnimationDirection.Backward : AnimationDirection.Forward;
                    }
                    else
                    {
                        previousPosition = Position;
                        position = (Direction == AnimationDirection.Forward) ? beginPosition : endPosition;
                        OnSeek(position, previousPosition);
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

        private TimeSpan GetBeginPosition()
        {
            return (beginTime.HasValue && beginTime.Value > TimeSpan.Zero) ? beginTime.Value : TimeSpan.Zero;
        }

        private TimeSpan GetEndPosition()
        {
            return (endTime.HasValue && endTime.Value < totalDuration) ? endTime.Value : totalDuration;
        }
    }
}