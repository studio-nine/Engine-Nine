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
using System.Collections;
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
    /// Represents a basic animation player that can play multiple animation
    /// sequences using different channels.
    /// </summary>
    public class AnimationPlayer : AnimationPlayerChannel
    {
        Dictionary<object, AnimationPlayerChannel> channels;

        /// <summary>
        /// Gets the dictionary that stores any animation data.
        /// </summary>
        public IDictionary<string, IAnimation> Animations { get; private set; }
        
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
            base.animations = this.Animations;
        }

        public bool Contains(string name)
        {
            return Animations.ContainsKey(name);
        }

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
                    channel.animations = Animations;
                    channels.Add(channelIdentifier, channel);
                }

                return channel;
            }
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (channels != null)
            {
                foreach (AnimationPlayerChannel channel in channels.Values)
                    channel.Update(gameTime);
            }
        }
    }

    /// <summary>
    /// Represents a channel used by <c>AnimationPlayer</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class AnimationPlayerChannel : Animation
    {
        internal IDictionary<string, IAnimation> animations;

        public string CurrentName { get; private set; }

        public IAnimation Current { get; private set; }

        /// <summary>
        /// Plays the animation with the specified name.
        /// </summary>
        public void Play(string animationName)
        {
            IAnimation current;

            if (!animations.TryGetValue(animationName, out current))
            {
                throw new KeyNotFoundException("name");
            }

            if (Current != null)
                Current.Stop();

            Current = current;
            CurrentName = animationName;

            Play();
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

            if (animations.ContainsKey(animationName))
                throw new ArgumentException(
                    string.Format("An animation with the name {0} already exists.", animationName));
            
            if (Current != null)
                Current.Stop();

            Current = current;
            CurrentName = animationName;

            Play();
        }

        protected override void OnStarted()
        {
            base.OnStarted();

            if (Current != null)
                Current.Play();
        }

        protected override void OnPaused()
        {
            base.OnPaused();

            if (Current != null)
                Current.Pause();
        }

        protected override void OnStopped()
        {
            base.OnStopped();

            if (Current != null)
                Current.Stop();
        }

        protected override void OnResumed()
        {
            base.OnResumed();

            if (Current != null)
                Current.Resume();
        }

        public override void Update(GameTime gameTime)
        {
            if (Current is IUpdateObject && State == AnimationState.Playing)
            {
                ((IUpdateObject)Current).Update(gameTime);

                if (Current.State == AnimationState.Stopped)
                {
                    if (Completed != null)
                        Completed(this, EventArgs.Empty);

                    Stop();
                }
            }
        }

        public override event EventHandler Completed;
    }
}