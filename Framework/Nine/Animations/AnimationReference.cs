namespace Nine.Animations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    
    /// <summary>
    /// Defines an animation that refers to an existing animation.
    /// </summary>
    public class AnimationReference : Animation, ISupportTarget
    {
        private IAnimation source;
        private object target;
        private string targetProperty;
        private bool expressionChanged;
        private PropertyExpression<IAnimation> expression;

        /// <summary>
        /// Gets the current animation clip been played
        /// </summary>
        [ContentSerializerIgnore]
        public IAnimation Source
        {
            get { return source; }
            set
            {
                if (source != value && (source = value) != null && State != source.State)
                {
                    if (State == AnimationState.Playing)
                        source.Play();
                    else if (State == AnimationState.Stopped)
                        source.Stop();
                    else if (State == AnimationState.Paused)
                        source.Pause();
                }
            }
        }
        
        /// <summary>
        /// Gets or sets the target object that this tweening will affect.
        /// This property is not required.
        /// </summary>
        [ContentSerializerIgnore]
        public object Target
        {
            get { return target; }
            set { if (target != value) { target = value; expressionChanged = true; } }
        }

        /// <summary>
        /// Gets or sets the property or field name of the target object.
        /// The property or field must be publicly visible.
        /// This property is not required.
        /// </summary>
        public string TargetProperty
        {
            get { return targetProperty; }
            set { if (targetProperty != value) { targetProperty = value; expressionChanged = true; } }
        }

        /// <summary>
        /// Creates a new <c>AnimationReference</c>.
        /// </summary>
        public AnimationReference()
        {

        }

        /// <summary>
        /// Creates a new <c>AnimationReference</c> with the specified animations.
        /// </summary>
        public AnimationReference(IAnimation source)
        {
            this.Source = source;
        }

        /// <summary>
        /// Plays the animation from start.
        /// </summary>
        protected override void OnStarted()
        {
            if (Target != null && !string.IsNullOrEmpty(TargetProperty))
            {
                if (expression == null || expressionChanged)
                {
                    expressionChanged = false;
                    expression = new PropertyExpression<IAnimation>(Target, TargetProperty);
                    Source = expression.Value;
                }
            }
            if (source != null)
                source.Play();
            base.OnStarted();
        }

        /// <summary>
        /// Stops the animation.
        /// </summary>
        protected override void OnStopped()
        {
            if (source != null)
                source.Stop();
            base.OnStopped();
        }

        /// <summary>
        /// Pauses the animation.
        /// </summary>
        protected override void OnPaused()
        {
            if (source != null)
                source.Pause();
            base.OnPaused();
        }

        /// <summary>
        /// Resumes the animation.
        /// </summary>
        protected override void OnResumed()
        {
            if (source != null)
                source.Resume();
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
                var update = source as Nine.IUpdateable;
                if (update != null)
                    update.Update(elapsedTime);
            }
        }
    }
}