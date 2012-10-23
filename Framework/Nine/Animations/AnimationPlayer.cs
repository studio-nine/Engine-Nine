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
        public AnimationPlayerChannel GetChannel(object channelIdentifier)
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

        /// <summary>
        /// Updates the specified elapsed time.
        /// </summary>
        public override void Update(float elapsedTime)
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
                if (channels != null)
                    foreach (var channel in channels.Values)
                        channel.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    /// <summary>
    /// Represents a channel used by <c>AnimationPlayer</c>.
    /// </summary>
    public class AnimationPlayerChannel : IUpdateable, IDisposable, IDictionary<string, IAnimation>
    {
        [ContentSerializer]
        internal IDictionary<string, IAnimation> Animations;

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
            this.Animations = animations;
        }

        /// <summary>
        /// Plays the first or default animation.
        /// </summary>
        /// <returns></returns>
        public IAnimation Play()
        {
            return Play(Animations.Values.FirstOrDefault());
        }

        /// <summary>
        /// Plays the animation with the specified name.
        /// </summary>
        public IAnimation Play(string animationName)
        {
            if (current != null)
                current.Stop();

            IAnimation animation;
            if (Animations.TryGetValue(animationName, out animation))
            {
                current = animation;
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
            if (animation != null)
            {
                current = animation;
                current.Play();
            }
            return current;
        }
        
        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public virtual void Update(float elapsedTime)
        {
            var updateable = Current as Nine.IUpdateable;
            if (updateable != null)
                updateable.Update(elapsedTime);
        }

        #region IDisposable
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (var animation in Animations.Values)
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
        #endregion

        #region Dictionary
        public void Add(string animationName, IAnimation value)
        {
            Animations.Add(animationName, value);
        }

        public bool ContainsKey(string animationName)
        {
            return Animations.ContainsKey(animationName);
        }

        ICollection<string> IDictionary<string, IAnimation>.Keys
        {
            get { return Animations.Keys; }
        }

        public bool Remove(string animationName)
        {
            return Animations.Remove(animationName);
        }

        public bool TryGetValue(string animationName, out IAnimation value)
        {
            return Animations.TryGetValue(animationName, out value);
        }

        ICollection<IAnimation> IDictionary<string, IAnimation>.Values
        {
            get { return Animations.Values; }
        }

        public IAnimation this[string animationName]
        {
            get { return Animations[animationName]; }
            set { Animations[animationName] = value; }
        }

        void ICollection<KeyValuePair<string, IAnimation>>.Add(KeyValuePair<string, IAnimation> item)
        {
            ((ICollection<KeyValuePair<string, IAnimation>>)Animations).Add(item);
        }

        public void Clear()
        {
            Animations.Clear();
        }

        bool ICollection<KeyValuePair<string, IAnimation>>.Contains(KeyValuePair<string, IAnimation> item)
        {
            return ((ICollection<KeyValuePair<string, IAnimation>>)Animations).Contains(item);
        }

        void ICollection<KeyValuePair<string, IAnimation>>.CopyTo(KeyValuePair<string, IAnimation>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<string, IAnimation>>)Animations).CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return Animations.Count; }
        }

        bool ICollection<KeyValuePair<string, IAnimation>>.IsReadOnly
        {
            get { return false; }
        }

        bool ICollection<KeyValuePair<string, IAnimation>>.Remove(KeyValuePair<string, IAnimation> item)
        {
            return ((ICollection<KeyValuePair<string, IAnimation>>)Animations).Remove(item);
        }

        IEnumerator<KeyValuePair<string, IAnimation>> IEnumerable<KeyValuePair<string, IAnimation>>.GetEnumerator()
        {
            return Animations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Animations.GetEnumerator();
        }
        #endregion
    }
}