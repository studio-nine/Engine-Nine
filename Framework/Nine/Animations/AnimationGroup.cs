namespace Nine.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;

    /// <summary>
    /// Contains several animation clips that are played concurrently.
    /// The animation completes when all of its containing animations
    /// had finished playing.
    /// </summary>
    [ContentProperty("Animations")]
    public class AnimationGroup : Animation, IEnumerable<IAnimation>
    {
        /// <summary>
        /// Gets all the animations in the animation group.
        /// </summary>
        public IList<IAnimation> Animations
        {
            get { return animations; }
        }
        private List<IAnimation> animations = new List<IAnimation>();

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
        /// Creates a new <c>AnimationGroup</c>.
        /// </summary>
        public AnimationGroup() 
        {
            animations = new List<IAnimation>();
        }

        /// <summary>
        /// Creates a new <c>AnimationGroup</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public AnimationGroup(IEnumerable<IAnimation> animations)
        {
            this.animations.AddRange(animations);
        }

        /// <summary>
        /// Creates a new <c>AnimationGroup</c> then fill each layer
        /// with the input animations.
        /// </summary>
        public AnimationGroup(params IAnimation[] animations)
        {
            this.animations.AddRange(animations);
        }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            repeatCounter = 0;
            for (int i = 0; i < animations.Count; ++i)
                animations[i].Play();
            base.OnStarted();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected override void OnStopped()
        {
            for (int i = 0; i < animations.Count; ++i)
                animations[i].Stop();
            base.OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected override void OnPaused()
        {
            for (int i = 0; i < animations.Count; ++i)
                animations[i].Pause();
            base.OnPaused();
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected override void OnResumed()
        {
            for (int i = 0; i < animations.Count; ++i)
                animations[i].Resume();
            base.OnResumed();
        }

        /// <summary>
        /// Updates the internal state of the object based on game time.
        /// </summary>
        public override void Update(TimeSpan elapsedTime)
        {
            if (State == AnimationState.Playing)
            {
                for (int i = 0; i < animations.Count; ++i)
                {
                    IUpdateable update = animations[i] as IUpdateable;
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
                    for (int i = 0; i < animations.Count; ++i)
                    {
                        if (animations[i].State != AnimationState.Stopped)
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
                        for (int i = 0; i < animations.Count; ++i)
                            animations[i].Play();
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