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
    /// Contains several animation clips that are played one after another.
    /// The animation completes when the last animation has finished playing.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class SequentialAnimationBase<T> : Animation, IEnumerable<T> where T : IAnimation
    {
        List<T> animations = new List<T>();

        /// <summary>
        /// Gets all the animations.
        /// </summary>
        public IList<T> Animations { get { return animations; } }

        /// <summary>
        /// Creates a new <c>SequentialAnimation</c>.
        /// </summary>
        public SequentialAnimationBase() { }

        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimationBase(IEnumerable<T> animations)
        {
            foreach (T animation in animations)
                this.animations.Add(animation);
        }
        
        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimationBase(params T[] animations)
        {
            foreach (T animation in animations)
                this.animations.Add(animation);
        }

        /// <summary>
        /// Gets the index of the current animation clip been played.
        /// </summary>
        public int CurrentIndex { get; private set; }

        /// <summary>
        /// Gets the current animation clip been played
        /// </summary>
        public IAnimation Current 
        {
            get
            {
                if (CurrentIndex >= 0 && CurrentIndex < animations.Count)
                    return animations[CurrentIndex];

                return null;
            }
        }

        /// <summary>
        /// Plays the specified animation clip.
        /// </summary>
        public void Play(T animation)
        {
            int index = animations.IndexOf(animation);

            if (index < 0)
                throw new ArgumentOutOfRangeException(
                    "The specified animation must be added to this animation.");
            
            Play(index);
        }

        /// <summary>
        /// Plays the specified animation clip with the given index.
        /// </summary>
        public void Play(int index)
        {
            Seek(index);
        }

        private void Seek(int index)
        {
            if (CurrentIndex != index && Current != null)
                Current.Stop();

            CurrentIndex = index;

            if (Current != null)
                Current.Play();
        }

        protected override void OnStarted()
        {
            Seek(0);

            base.OnStarted();
        }

        protected override void OnStopped()
        {
            if (Current != null)
            {
                Current.Stop();
                CurrentIndex = 0;
            }

            base.OnStopped();
        }

        protected override void OnPaused()
        {
            if (Current != null)
                Current.Pause();

            base.OnPaused();
        }

        protected override void OnResumed()
        {
            if (Current != null)
                Current.Resume();

            base.OnResumed();
        }

        public override event EventHandler Completed;

        public override void Update(GameTime gameTime)
        {
            if (State == AnimationState.Playing)
            {
                IUpdateObject update = Current as IUpdateObject;

                if (update != null)
                    update.Update(gameTime);


                if (Current != null &&
                    Current.State != AnimationState.Playing)
                {
                    CurrentIndex++;
                    
                    if (CurrentIndex < animations.Count)
                        Current.Play();
                }

                if (CurrentIndex == animations.Count)
                {
                    Stop();
                    OnCompleted();
                }
            }
        }

        protected virtual void OnCompleted()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return animations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return animations.GetEnumerator();
        }
    }

    /// <summary>
    /// Contains several animation clips that are played one after another.
    /// The animation completes when the last animation has finished playing.
    /// </summary>
    public class SequentialAnimation : SequentialAnimationBase<IAnimation> 
    {
        /// <summary>
        /// Creates a new <c>SequentialAnimation</c>.
        /// </summary>
        public SequentialAnimation() { }

        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimation(IEnumerable<IAnimation> animations) : base(animations) { }
        
        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimation(params IAnimation[] animations) : base(animations) { }
    }
}