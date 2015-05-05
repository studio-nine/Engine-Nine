namespace Nine.Animations
{
    using Microsoft.Xna.Framework.Content;
    using Nine.AttachedProperty;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    
    /// <summary>
    /// Contains several animation clips that are played one after another.
    /// The animation completes when the last animation has finished playing.
    /// </summary>
    [ContentProperty("Animations")]
    public class AnimationSequence : Animation, IEnumerable<IAnimation>
    {
        /// <summary>
        /// Gets all the animations in the animation sequence.
        /// </summary>
        public IList<IAnimation> Animations
        {
            get { return animations; }
        }
        private List<IAnimation> animations = new List<IAnimation>();

        /// <summary>
        /// Gets the index of the current animation clip been played.
        /// </summary>
        public int CurrentIndex
        {
            get { return currentIndex; }
        }
        private int currentIndex;

        /// <summary>
        /// Gets the current animation clip been played
        /// </summary>
        public IAnimation Current
        {
            get
            {
                if (currentIndex >= 0 && currentIndex < animations.Count)
                    return animations[currentIndex];
                return null;
            }
        }

        /// <summary>
        /// Gets or sets number of times this animation will be played.
        /// </summary>
        public int Repeat
        {
            get { return repeat; }
            set { repeat = value; }
        }
        private int repeat = 1;
        private int repeatCounter = 0;
        
        /// <summary>
        /// Occurs when this animation has reached the end and repeated.
        /// </summary>
        public event EventHandler Repeated;

        /// <summary>
        /// Creates a new <c>AnimationSequence</c>.
        /// </summary>
        public AnimationSequence()
        {

        }

        /// <summary>
        /// Creates a new <c>AnimationSequence</c> with the specified animations.
        /// </summary>
        public AnimationSequence(IEnumerable<IAnimation> animations)
        {
            this.animations.AddRange(animations);
        }
        
        /// <summary>
        /// Creates a new <c>AnimationSequence</c> with the specified animations.
        /// </summary>
        public AnimationSequence(params IAnimation[] animations)
        {
            this.animations.AddRange(animations);
        }

        /// <summary>
        /// Plays the specified animation clip.
        /// </summary>
        public void Play(IAnimation animation)
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

        /// <summary>
        /// Seeks the specified index.
        /// </summary>
        /// <param name="index">The index.</param>
        private void Seek(int index)
        {
            if (CurrentIndex != index && Current != null)
                Current.OnStopped();

            currentIndex = index;

            if (Current != null)
                Current.OnStarted();
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
                Current.OnStopped();
                currentIndex = 0;
            }

            base.OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected override void OnPaused()
        {
            if (Current != null)
                Current.OnPaused();

            base.OnPaused();
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected override void OnResumed()
        {
            if (Current != null)
                Current.OnResumed();

            base.OnResumed();
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        /// <param name="elapsedTime"></param>
        public override void Update(float elapsedTime)
        {
            if (State == AnimationState.Playing)
            {
                IUpdateable update = Current as IUpdateable;

                if (update != null)
                    update.Update(elapsedTime);

                if (Current != null && Current.State != AnimationState.Playing)
                {
                    currentIndex++;

                    if (CurrentIndex < Animations.Count)
                        Current.OnStarted();
                }

                if (CurrentIndex == Animations.Count)
                {
                    repeatCounter++;
                    if (repeatCounter == Repeat)
                    {
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
            var repeated = Repeated;
            if (repeated != null)
                repeated(this, EventArgs.Empty);
        }

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
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