namespace Nine.Animations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// Represents a basic animation player that can play multiple animation
    /// sequences using different channels.
    /// </summary>
    public class AnimationPlayer : AnimationPlayerChannel
    {
        Dictionary<object, AnimationPlayerChannel> channels;

        /// <summary>
        /// Gets the dictionary that stores any animation data.
        /// </summary>
        [ContentSerializerIgnore]
        public IDictionary<string, IAnimation> Animations { get; private set; }
        
        [ContentSerializer(ElementName = "Animations")]
        internal IDictionary<string, object> AnimationsSerializer
        {
            get { return null; }
            set
            {
                Animations.Clear();
                if (value != null)
                {
                    foreach (var pair in value)
                    {
                        if (pair.Value is IAnimation)
                            Animations.Add(pair.Key, (IAnimation)pair.Value);
                    }
                }
            }
        }
        
        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        /// <summary>
        /// Creates a new instance of AnimationPlayer.
        /// </summary>
        public AnimationPlayer()
        {
            this.Animations = new Dictionary<string, IAnimation>();
            base.SharedAnimations = this.Animations;
        }

        /// <summary>
        /// Determines whether this animation player contains an animation with the specified name.
        /// </summary>
        public bool Contains(string name)
        {
            return Animations.ContainsKey(name);
        }

        /// <summary>
        /// Gets the <see cref="Nine.Animations.AnimationPlayerChannel"/> with the specified channel identifier.
        /// </summary>
        public AnimationPlayerChannel this[object channelIdentifier]
        {
            get
            {
                if (channels == null)
                    channels = new Dictionary<object, AnimationPlayerChannel>();
                
                AnimationPlayerChannel channel;

                if (!channels.TryGetValue(channelIdentifier, out channel))
                {
                    channel = new AnimationPlayerChannel();
                    channel.SharedAnimations = Animations;
                    channels.Add(channelIdentifier, channel);
                }

                return channel;
            }
        }

        /// <summary>
        /// Called when the animation player has started.
        /// </summary>
        protected override void OnStarted()
        {
            if (Current == null)
            {
                CurrentName = Animations.Keys.FirstOrDefault();
                if (CurrentName != null)
                    Current = Animations[CurrentName];
            }
            base.OnStarted();
        }

        /// <summary>
        /// Updates the specified elapsed time.
        /// </summary>
        public override void Update(TimeSpan elapsedTime)
        {
            base.Update(elapsedTime);

            if (channels != null)
            {
                foreach (AnimationPlayerChannel channel in channels.Values)
                    channel.Update(elapsedTime);
            }
        }
    }

    /// <summary>
    /// Represents a channel used by <c>AnimationPlayer</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AnimationPlayerChannel : Animation
    {
        internal IDictionary<string, IAnimation> SharedAnimations;

        /// <summary>
        /// Gets the name of the current animation.
        /// </summary>
        public string CurrentName { get; internal set; }

        /// <summary>
        /// Gets the current animation.
        /// </summary>
        public IAnimation Current { get; internal set; }

        /// <summary>
        /// Plays the animation with the specified name.
        /// </summary>
        public void Play(string animationName)
        {
            IAnimation current;

            if (!SharedAnimations.TryGetValue(animationName, out current))
            {
                throw new KeyNotFoundException(string.Format("Animation not found {0}", animationName));
            }

            if (Current != null)
                Current.Stop();

            Current = current;
            CurrentName = animationName;

            Play();
        }

        /// <summary>
        /// Tries to play the animation with the specified name.
        /// </summary>
        public void TryPlay(string animationName)
        {
            IAnimation current;

            if (SharedAnimations.TryGetValue(animationName, out current))
            {

                if (Current != null)
                    Current.Stop();

                Current = current;
                CurrentName = animationName;

                Play();
            }
        }

        /// <summary>
        /// Plays the specified animation.
        /// </summary>
        public void Play(IAnimation animation)
        {
            IAnimation current = animation;

            if (Current != null)
                Current.Stop();

            Current = current;
            CurrentName = null;

            Play();
        }

        /// <summary>
        /// Plays the specified animation with a delay.
        /// </summary>
        public void Play(IAnimation animation, TimeSpan delay)
        {
            if (animation == null)
                throw new ArgumentNullException("animation");

            IAnimation current = animation;

            if (Current != null)
                Current.Stop();

            Current = new SequentialAnimation(new DelayAnimation(delay), current);
            CurrentName = null;

            Play();
        }

        /// <summary>
        /// Plays the specified animation and adds it to the animation collection.
        /// </summary>
        public void Play(string animationName, IAnimation animation)
        {
            IAnimation current = animation;

            if (SharedAnimations.ContainsKey(animationName))
                throw new ArgumentException(
                    string.Format("An animation with the name {0} already exists.", animationName));
            
            if (Current != null)
                Current.Stop();

            Current = current;
            CurrentName = animationName;

            Play();
        }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            if (Current != null)
            {
                Current.Play();
                base.OnStarted();
            }
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected override void OnPaused()
        {
            if (Current != null)
            {
                Current.Pause();
                base.OnPaused();
            }
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected override void OnStopped()
        {
            if (Current != null)
            {
                Current.Stop();
                base.OnStopped();
            }
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected override void OnResumed()
        {
            if (Current != null)
            {
                Current.Resume();
                base.OnResumed();
            }
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public override void Update(TimeSpan elapsedTime)
        {
            if (Current is IUpdateable && State == AnimationState.Playing)
            {
                ((IUpdateable)Current).Update(elapsedTime);

                if (Current.State == AnimationState.Stopped)
                {
                    Stop();
                    OnCompleted();
                }
            }
        }
    }
}