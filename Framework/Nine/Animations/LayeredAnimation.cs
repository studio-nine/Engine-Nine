#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Content;
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
        [ContentSerializerIgnore]
        public IList<IAnimation> Animations { get; private set; }

        [ContentSerializer(ElementName="Animations")]
        internal IList<object> AnimationsSerializer
        {
            get { throw new NotSupportedException(); }
            set { Animations.Clear(); Animations.AddRange(value.OfType<IAnimation>()); }
        }
        
        /// <summary>
        /// Gets or sets the key animation of this LayeredAnimation.
        /// A LayeredAnimation ends either when the last contained 
        /// animation stops or when the specifed KeyAnimation ends.
        /// </summary>
        [ContentSerializerIgnore]
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
            this.Repeat = 1;
            Animations = new List<IAnimation>();
            foreach (IAnimation animation in animations)
                Animations.Add(animation);
        }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            repeatCounter = 0;

            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Play();

            base.OnStarted();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected override void OnStopped()
        {
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Stop();

            base.OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected override void OnPaused()
        {
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Pause();

            base.OnPaused();
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected override void OnResumed()
        {
            for (int i = 0; i < Animations.Count; i++)
                Animations[i].Resume();

            base.OnResumed();
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public override void Update(TimeSpan elapsedTime)
        {
            if (State == AnimationState.Playing)
            {
                for (int i = 0; i < Animations.Count; i++)
                {
                    IUpdateable update = Animations[i] as IUpdateable;

                    if (update != null)
                        update.Update(elapsedTime);
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
        }

        /// <summary>
        /// Called when repeated.
        /// </summary>
        protected virtual void OnRepeated()
        {
            if (Repeated != null)
                Repeated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
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