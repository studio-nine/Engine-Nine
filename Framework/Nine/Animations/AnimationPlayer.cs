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
    }

    /// <summary>
    /// Represents a channel used by <c>AnimationPlayer</c>.
    /// </summary>
    [ContentProperty("Animations")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AnimationPlayerChannel : IUpdateable, IDictionary<string, IAnimation>
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
            var updateable = Current as IUpdateable;
            if (updateable != null)
                updateable.Update(elapsedTime);
        }

        #region Dictionary
        // Implements Dictionary interface to make it more xaml friendly.
        void IDictionary<string, IAnimation>.Add(string key, IAnimation value)
        {
            animations.Add(key, value);
        }

        bool IDictionary<string, IAnimation>.ContainsKey(string key)
        {
            return animations.ContainsKey(key);
        }

        ICollection<string> IDictionary<string, IAnimation>.Keys
        {
            get { return animations.Keys; }
        }

        bool IDictionary<string, IAnimation>.Remove(string key)
        {
            return animations.Remove(key);
        }

        bool IDictionary<string, IAnimation>.TryGetValue(string key, out IAnimation value)
        {
            return animations.TryGetValue(key, out value);
        }

        ICollection<IAnimation> IDictionary<string, IAnimation>.Values
        {
            get { return animations.Values; }
        }

        IAnimation IDictionary<string, IAnimation>.this[string key]
        {
            get { return animations[key]; }
            set { animations[key] = value; }
        }

        void ICollection<KeyValuePair<string, IAnimation>>.Add(KeyValuePair<string, IAnimation> item)
        {
            ((ICollection<KeyValuePair<string, IAnimation>>)animations).Add(item);
        }

        void ICollection<KeyValuePair<string, IAnimation>>.Clear()
        {
            animations.Clear();
        }

        bool ICollection<KeyValuePair<string, IAnimation>>.Contains(KeyValuePair<string, IAnimation> item)
        {
            return ((ICollection<KeyValuePair<string, IAnimation>>)animations).Contains(item);
        }

        void ICollection<KeyValuePair<string, IAnimation>>.CopyTo(KeyValuePair<string, IAnimation>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, IAnimation>>)animations).CopyTo(array, arrayIndex);
        }

        int ICollection<KeyValuePair<string, IAnimation>>.Count
        {
            get { return animations.Count; }
        }

        bool ICollection<KeyValuePair<string, IAnimation>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, IAnimation>>.Remove(KeyValuePair<string, IAnimation> item)
        {
            return ((ICollection<KeyValuePair<string, IAnimation>>)animations).Remove(item);
        }

        IEnumerator<KeyValuePair<string, IAnimation>> IEnumerable<KeyValuePair<string, IAnimation>>.GetEnumerator()
        {
            return animations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return animations.GetEnumerator();
        }
        #endregion
    }
}