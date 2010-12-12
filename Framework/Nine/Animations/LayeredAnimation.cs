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
    /// Contains several animation clips that are played concurrently.
    /// The animation completes when all of its containing animations
    /// had finished playing.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LayeredAnimationBase<T> : Animation, IEnumerable<T> where T : IAnimation
    {
        /// <summary>
        /// Gets all the layers in the animation.
        /// </summary>
        public IList<T> Animations { get; private set; }

        /// <summary>
        /// Gets or sets the key animation of this LayeredAnimation.
        /// A LayeredAnimation ends either when the last contained 
        /// animation stops or when the specifed KeyAnimation ends.
        /// </summary>
        public T KeyAnimation
        {
            get { return keyAnimation; }
            set
            {
                if (State != AnimationState.Stopped)
                    throw new InvalidOperationException(
                        "Cannot modify the collection when the animation is been played.");

                if (value != null && !Animations.Contains(value))
                    throw new ArgumentException(
                        "The specified animation must be added to this animation.");

                keyAnimation = value;
            }
        }

        private T keyAnimation;

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c>.
        /// </summary>
        public LayeredAnimationBase() 
        {
            Animations = new LayeredAnimationCollection<T>(this);
        }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public LayeredAnimationBase(IEnumerable<T> animations)
        {
            Animations = new LayeredAnimationCollection<T>(this);

            foreach (T animation in animations)
                Animations.Add(animation);
        }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public LayeredAnimationBase(params T[] animations)
        {
            Animations = new LayeredAnimationCollection<T>(this);

            foreach (T animation in animations)
                Animations.Add(animation);
        }
        
        protected override void OnStarted()
        {
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Play();

            base.OnStarted();
        }
        
        protected override void OnStopped()
        {
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Stop();

            base.OnStopped();
        }

        protected override void OnPaused()
        {
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Pause();

            base.OnPaused();
        }

        protected override void OnResumed()
        {
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Resume();

            base.OnResumed();
        }

        public override event EventHandler Completed;

        public override void Update(GameTime gameTime)
        {
            if (State == AnimationState.Playing)
            {
                for (int i = 0; i < Animations.Count; i++)
                {
                    IUpdateObject update = Animations[i] as IUpdateObject;

                    if (update != null)
                        update.Update(gameTime);
                }

                bool allStopped = true;

                if (KeyAnimation != null)
                {
                    allStopped = (KeyAnimation.State == AnimationState.Stopped);
                }
                else
                {
                    for (int i = 0; i < Animations.Count; i++)
                    {
                        if (Animations[i].State != AnimationState.Stopped)
                        {
                            allStopped = false;
                        }
                    }
                }

                if (allStopped)
                {
                    Stop();
                    OnCompleted();
                }
            }
 	    
            base.Update(gameTime);
        }

        protected virtual void OnCompleted()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return Animations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Animations.GetEnumerator();
        }
    }

    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LayeredAnimationCollection<T> : EnumerableCollection<T> where T : IAnimation
    {
        LayeredAnimationBase<T> Animation;

        internal LayeredAnimationCollection(LayeredAnimationBase<T> animation)
        {
            this.Animation = animation;
        }

        protected override void OnAdded(int index, T value)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (Animation.State != AnimationState.Stopped)
                throw new InvalidOperationException(
                    "Cannot modify the collection when the animation is been played.");

            base.OnAdded(index, value);
        }

        protected override void OnChanged(int index, T value, T previousValue)
        {
            if (value == null)
                throw new ArgumentNullException("value");

            if (Animation.State != AnimationState.Stopped)
                throw new InvalidOperationException(
                    "Cannot modify the collection when the animation is been played.");

            base.OnChanged(index, value, previousValue);
        }

        protected override void OnRemoved(int index, T value)
        {
            if (Animation.State != AnimationState.Stopped)
                throw new InvalidOperationException(
                    "Cannot modify the collection when the animation is been played.");

            base.OnRemoved(index, value);
        }
    }

    /// <summary>
    /// Contains several animation clips that are played concurrently.
    /// The animation completes when all of its containing animations
    /// had finished playing.
    /// </summary>
    public class LayeredAnimation : LayeredAnimationBase<IAnimation>
    {
        /// <summary>
        /// Creates a new <c>LayeredAnimation</c>.
        /// </summary>
        public LayeredAnimation() { }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> with the specified animations.
        /// </summary>
        public LayeredAnimation(IEnumerable<IAnimation> animations) : base(animations) { }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> with the specified animations.
        /// </summary>
        public LayeredAnimation(params IAnimation[] animations) : base(animations) { }
    }
}