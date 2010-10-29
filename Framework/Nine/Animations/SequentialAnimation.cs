#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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
    public class SequentialAnimation : Animation, IEnumerable<IAnimation>
    {
        List<IAnimation> animations = new List<IAnimation>();

        /// <summary>
        /// Gets all the animations.
        /// </summary>
        public IList<IAnimation> Animations { get { return animations; } }

        /// <summary>
        /// Creates a new <c>SequentialAnimation</c>.
        /// </summary>
        public SequentialAnimation() { }

        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimation(IEnumerable<IAnimation> animations)
        {
            foreach (IAnimation animation in animations)
                this.animations.Add(animation);
        }
        
        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimation(params IAnimation[] animations)
        {
            foreach (IAnimation animation in animations)
                this.animations.Add(animation);
        }

        /// <summary>
        /// Gets the index of the current animation clip been played.
        /// </summary>
        public int Current { get; private set; }

        /// <summary>
        /// Gets the current animation clip been played
        /// </summary>
        public IAnimation CurrentAnimation 
        {
            get
            {
                if (Current >= 0 && Current < animations.Count)
                    return animations[Current];

                return null;
            }
        }

        /// <summary>
        /// Plays the specified animation clip.
        /// </summary>
        public void Play(IAnimation animation)
        {
            int index = animations.IndexOf(animation);

            if (index < 0)
                throw new ArgumentOutOfRangeException(
                    "Can not find the specified animation, make sure it has been added to the collection.");
            
            Play(index);
        }

        /// <summary>
        /// Plays the specified animation clip with the given index.
        /// </summary>
        public void Play(int index)
        {
            Play();
            Seek(index);
        }

        private void Seek(int index)
        {
            if (Current != index)
                CurrentAnimation.Stop();
            Current = index;
            CurrentAnimation.Play();
        }

        protected override void OnStarted()
        {
            base.OnStarted();
        }

        protected override void OnStopped()
        {
            CurrentAnimation.Stop();
            Current = 0;

            base.OnStopped();
        }

        protected override void OnPaused()
        {
            CurrentAnimation.Pause();

            base.OnPaused();
        }

        protected override void OnResumed()
        {
            CurrentAnimation.Resume();

            base.OnResumed();
        }

        public override event EventHandler Completed;

        public override void Update(GameTime gameTime)
        {
            if (State == AnimationState.Playing)
            {
                IUpdateObject update = CurrentAnimation as IUpdateObject;

                if (update != null)
                    update.Update(gameTime);


                if (CurrentAnimation != null &&
                    CurrentAnimation.State != AnimationState.Playing)
                    Current++;

                if (Current == animations.Count)
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

        public IEnumerator<IAnimation> GetEnumerator()
        {
            return animations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return animations.GetEnumerator();
        }
    }
}