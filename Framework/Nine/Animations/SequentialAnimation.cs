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
    /// Contains several animation clips that are played one after another.
    /// The animation completes when the last animation has finished playing.
    /// </summary>
    public class SequentialAnimation : Animation, IEnumerable<IAnimation>
    {
        /// <summary>
        /// Gets all the layers in the animation.
        /// </summary>
        [ContentSerializerIgnore]
        public IList<IAnimation> Animations { get; private set; }

        [ContentSerializer(ElementName = "Animations")]
        internal IList<object> AnimationsSerializer
        {
            get { throw new NotSupportedException(); }
            set { Animations.Clear(); Animations.AddRange(value.OfType<IAnimation>()); }
        }

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
        /// Creates a new <c>SequentialAnimation</c>.
        /// </summary>
        public SequentialAnimation()
        {
            Repeat = 1;
            Animations = new List<IAnimation>();
        }

        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimation(IEnumerable<IAnimation> animations)
        {
            Repeat = 1;
            Animations = new List<IAnimation>();
            foreach (IAnimation animation in animations)
                Animations.Add(animation);
        }
        
        /// <summary>
        /// Creates a new <c>SequentialAnimation</c> with the specified animations.
        /// </summary>
        public SequentialAnimation(params IAnimation[] animations)
        {
            Repeat = 1;
            Animations = new List<IAnimation>();
            foreach (IAnimation animation in animations)
                Animations.Add(animation);
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
                if (CurrentIndex >= 0 && CurrentIndex < Animations.Count)
                    return Animations[CurrentIndex];

                return null;
            }
        }

        /// <summary>
        /// Plays the specified animation clip.
        /// </summary>
        public void Play(IAnimation animation)
        {
            int index = Animations.IndexOf(animation);

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

        /// <summary>
        /// Seeks the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        private void Seek(int index)
        {
            if (CurrentIndex != index && Current != null)
                Current.Stop();

            CurrentIndex = index;

            if (Current != null)
                Current.Play();
        }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            repeatCounter = 0;

            Seek(0);

            base.OnStarted();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected override void OnStopped()
        {
            if (Current != null)
            {
                Current.Stop();
                CurrentIndex = 0;
            }

            base.OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected override void OnPaused()
        {
            if (Current != null)
                Current.Pause();

            base.OnPaused();
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected override void OnResumed()
        {
            if (Current != null)
                Current.Resume();

            base.OnResumed();
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public override void Update(TimeSpan elapsedTime)
        {
            if (State == AnimationState.Playing)
            {
                IUpdateable update = Current as IUpdateable;

                if (update != null)
                    update.Update(elapsedTime);


                if (Current != null &&
                    Current.State != AnimationState.Playing)
                {
                    CurrentIndex++;

                    if (CurrentIndex < Animations.Count)
                        Current.Play();
                }

                if (CurrentIndex == Animations.Count)
                {
                    repeatCounter++;
                    if (repeatCounter == Repeat)
                    {
                        Stop();
                        OnCompleted();
                    }
                    else
                    {
                        Seek(0);
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