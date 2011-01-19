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
    public class LayeredAnimation : Animation, IEnumerable<IAnimation>
    {
        /// <summary>
        /// Gets all the layers in the animation.
        /// </summary>
        public IList<IAnimation> Animations { get; private set; }
        
        /// <summary>
        /// Gets or sets the key animation of this LayeredAnimation.
        /// A LayeredAnimation ends either when the last contained 
        /// animation stops or when the specifed KeyAnimation ends.
        /// </summary>
        public IAnimation KeyAnimation
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

        private IAnimation keyAnimation;

        /// <summary>
        /// Gets or sets number of times this animation will be played.
        /// </summary>
        public int Repeat { get; set; }
        private int repeatCounter = 0;

        /// <summary>
        /// Occurs when this animation has reached the end and repeated.
        /// </summary>
        public event EventHandler Repeated;

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c>.
        /// </summary>
        public LayeredAnimation() 
        {
            Repeat = 1;
            Animations = new List<IAnimation>();
        }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public LayeredAnimation(IEnumerable<IAnimation> animations)
        {
            Repeat = 1;
            Animations = new List<IAnimation>();
            foreach (IAnimation animation in animations)
                Animations.Add(animation);
        }

        /// <summary>
        /// Creates a new <c>LayeredAnimation</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public LayeredAnimation(params IAnimation[] animations)
        {
            Repeat = 1;
            Animations = new List<IAnimation>();
            foreach (IAnimation animation in animations)
                Animations.Add(animation);
        }
        
        protected override void OnStarted()
        {
            repeatCounter = 0;

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
                    repeatCounter++;
                    if (repeatCounter == Repeat)
                    {
                        Stop();
                        OnCompleted();
                    }
                    else
                    {
                        for (int i = 0; i < Animations.Count; i++)
                            Animations[i].Play();
                        OnRepeated();
                    }
                }
            }
 	    
            base.Update(gameTime);
        }

        protected virtual void OnCompleted()
        {
            if (Completed != null)
                Completed(this, EventArgs.Empty);
        }

        protected virtual void OnRepeated()
        {
            if (Repeated != null)
                Repeated(this, EventArgs.Empty);
        }

        public IEnumerator<IAnimation> GetEnumerator()
        {
            return Animations.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return Animations.GetEnumerator();
        }
    }
}