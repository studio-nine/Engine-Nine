namespace Nine.Animations
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// Represents a basic animation player that can play multiple animation
    /// sequences using different channels.
    /// </summary>
    [ContentSerializable]
    public class AnimationPlayer : AnimationPlayerChannel
    {
        private Dictionary<object, AnimationPlayerChannel> channels;

        /// <summary>
        /// Creates a new instance of AnimationPlayer.
        /// </summary>
        public AnimationPlayer() : base(new Dictionary<string, IAnimation>())
        {

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
                    channel = new AnimationPlayerChannel(Animations);
                    channels.Add(channelIdentifier, channel);
                }
                return channel;
            }
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var channel in channels.Values)
                    channel.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Represents a channel used by <c>AnimationPlayer</c>.
    /// </summary>
    [ContentProperty("Animations")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AnimationPlayerChannel : IUpdateable, IDisposable
    {
        /// <summary>
        /// Gets the dictionary that stores any animation data.
        /// </summary>
        public IDictionary<string, IAnimation> Animations
        {
            get { return animations; }
        }
        private IDictionary<string, IAnimation> animations;

        /// <summary>
        /// Gets the name of the current animation.
        /// </summary>
        public string CurrentName
        {
            get { return currentName; }
        }
        private string currentName;

        /// <summary>
        /// Gets the current animation.
        /// </summary>
        public IAnimation Current
        {
            get { return current; }
        }
        private IAnimation current;

        /// <summary>
        /// Initializes a new instance of AnimationPlayerChannel.
        /// </summary>
        internal AnimationPlayerChannel(IDictionary<string, IAnimation> animations) 
        {
            this.animations = animations;
        }

        /// <summary>
        /// Plays the first or default animation.
        /// </summary>
        /// <returns></returns>
        public IAnimation Play()
        {
            return Play(animations.Values.FirstOrDefault());
        }

        /// <summary>
        /// Plays the animation with the specified name.
        /// </summary>
        public IAnimation Play(string animationName)
        {
            if (current != null)
                current.Stop();
            currentName = null;

            IAnimation animation;
            if (animations.TryGetValue(animationName, out animation))
            {
                current = animation;
                currentName = animationName;
                current.Play();
            }
            return animation;
        }

        /// <summary>
        /// Plays the specified animation.
        /// </summary>
        public IAnimation Play(IAnimation animation)
        {
            if (current != null)
                current.Stop();
            currentName = null;
            if (animation != null)
            {
                current = animation;
                current.Play();
            }
            return current;
        }

        /// <summary>
        /// Plays the specified animation with a delay.
        /// </summary>
        public IAnimation Play(IAnimation animation, TimeSpan delay)
        {
            if (current != null)
                current.Stop();
            currentName = null;
            if (animation != null)
            {
                current = new AnimationSequence(new DelayAnimation(delay), animation);
                current.Play();
            }
            return current;
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public virtual void Update(TimeSpan elapsedTime)
        {
            var updateable = Current as Nine.IUpdateable;
            if (updateable != null)
                updateable.Update(elapsedTime);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var animation in animations.Values)
                {
                    var disposable = animation as IDisposable;
                    if (disposable != null)
                        disposable.Dispose();
                }
            }
        }

        ~AnimationPlayerChannel()
        {
            Dispose(false);
        }
    }
}