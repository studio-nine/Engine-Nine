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
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Animations
{
    /// <summary>
    /// Defines an animation that modifies the transform of the target object.
    /// </summary>
    [ContentSerializable]
    public class TransformAnimation : Animation, ISupportTarget
    {
        private Matrix value;
        private object target;
        private string targetProperty;
        private bool expressionChanged;
        private PropertyExpression<Matrix> expression;

        private TweenAnimation<float> scaleX;
        private TweenAnimation<float> scaleY;
        private TweenAnimation<float> scaleZ;
        private TweenAnimation<float> rotationX;
        private TweenAnimation<float> rotationY;
        private TweenAnimation<float> rotationZ;
        private TweenAnimation<float> translationX;
        private TweenAnimation<float> translationY;
        private TweenAnimation<float> translationZ;
        private LayeredAnimation layeredAnimation;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransformAnimation"/> class.
        /// </summary>
        public TransformAnimation()
        {
            value = Matrix.Identity;
            layeredAnimation = new LayeredAnimation();
            layeredAnimation.Completed += (sender, e) => { OnCompleted(); };
        }

        /// <summary>
        /// Gets the current value of this transform animation.
        /// </summary>
        public Matrix Value { get { return value; } }

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
        /// Gets or sets the animation track that affects the X scale of the target object.
        /// </summary>
        public TweenAnimation<float> ScaleX
        {
            get { return scaleX; }
            set
            {
                if (scaleX != value)
                {
                    if (scaleX != null)
                        layeredAnimation.Animations.Remove(scaleX);
                    scaleX = value;
                    if (scaleX != null)
                    {
                        scaleX.Target = null;
                        scaleX.TargetProperty = null;
                        layeredAnimation.Animations.Add(scaleX);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the animation track that affects the Y scale of the target object.
        /// </summary>
        public TweenAnimation<float> ScaleY
        {
            get { return scaleY; }
            set
            {
                if (scaleY != value)
                {
                    if (scaleY != null)
                        layeredAnimation.Animations.Remove(scaleY);
                    scaleY = value;
                    if (scaleY != null)
                    {
                        scaleY.Target = null;
                        scaleY.TargetProperty = null;
                        layeredAnimation.Animations.Add(scaleY);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the animation track that affects the Z scale of the target object.
        /// </summary>
        public TweenAnimation<float> ScaleZ
        {
            get { return scaleZ; }
            set
            {
                if (scaleZ != value)
                {
                    if (scaleZ != null)
                        layeredAnimation.Animations.Remove(scaleZ);
                    scaleZ = value;
                    if (scaleZ != null)
                    {
                        scaleZ.Target = null;
                        scaleZ.TargetProperty = null;
                        layeredAnimation.Animations.Add(scaleZ);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the rotation order.
        /// </summary>
        public RotationOrder RotationOrder { get; set; }

        /// <summary>
        /// Gets or sets the animation track that affects the X rotation of the target object.
        /// </summary>
        public TweenAnimation<float> RotationX
        {
            get { return rotationX; }
            set
            {
                if (rotationX != value)
                {
                    if (rotationX != null)
                        layeredAnimation.Animations.Remove(rotationX);
                    rotationX = value;
                    if (rotationX != null)
                    {
                        rotationX.Target = null;
                        rotationX.TargetProperty = null;
                        layeredAnimation.Animations.Add(rotationX);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the animation track that affects the Y rotation of the target object.
        /// </summary>
        public TweenAnimation<float> RotationY
        {
            get { return rotationY; }
            set
            {
                if (rotationY != value)
                {
                    if (rotationY != null)
                        layeredAnimation.Animations.Remove(rotationY);
                    rotationY = value;
                    if (rotationY != null)
                    {
                        rotationY.Target = null;
                        rotationY.TargetProperty = null;
                        layeredAnimation.Animations.Add(rotationY);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the animation track that affects the Z rotation of the target object.
        /// </summary>
        public TweenAnimation<float> RotationZ
        {
            get { return rotationZ; }
            set
            {
                if (rotationZ != value)
                {
                    if (rotationZ != null)
                        layeredAnimation.Animations.Remove(rotationZ);
                    rotationZ = value;
                    if (rotationZ != null)
                    {
                        rotationZ.Target = null;
                        rotationZ.TargetProperty = null;
                        layeredAnimation.Animations.Add(rotationZ);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the animation track that affects the X position of the target object.
        /// </summary>
        public TweenAnimation<float> X
        {
            get { return translationX; }
            set
            {
                if (translationX != value)
                {
                    if (translationX != null)
                        layeredAnimation.Animations.Remove(translationX);
                    translationX = value;
                    if (translationX != null)
                    {
                        translationX.Target = null;
                        translationX.TargetProperty = null;
                        layeredAnimation.Animations.Add(translationX);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the animation track that affects the Y position of the target object.
        /// </summary>
        public TweenAnimation<float> Y
        {
            get { return translationY; }
            set
            {
                if (translationY != value)
                {
                    if (translationY != null)
                        layeredAnimation.Animations.Remove(translationY);
                    translationY = value;
                    if (translationY != null)
                    {
                        translationY.Target = null;
                        translationY.TargetProperty = null;
                        layeredAnimation.Animations.Add(translationY);
                    }
                }
            }
        }

        /// <summary>
        /// Gets or sets the animation track that affects the Z position of the target object.
        /// </summary>
        public TweenAnimation<float> Z
        {
            get { return translationZ; }
            set
            {
                if (translationZ != value)
                {
                    if (translationZ != null)
                        layeredAnimation.Animations.Remove(translationZ);
                    translationZ = value;
                    if (translationZ != null)
                    {
                        translationZ.Target = null;
                        translationZ.TargetProperty = null;
                        layeredAnimation.Animations.Add(translationZ);
                    }
                }
            }
        }

        protected override void OnStarted()
        {
            if (Target != null && !string.IsNullOrEmpty(TargetProperty))
            {
                if (expression == null || expressionChanged)
                    expression = new PropertyExpression<Matrix>(Target, TargetProperty);
            }

            layeredAnimation.Play();
            base.OnStarted();
        }

        protected override void OnPaused()
        {
            layeredAnimation.Pause();
            base.OnPaused();
        }

        protected override void OnResumed()
        {
            layeredAnimation.Resume();
            base.OnResumed();
        }

        protected override void OnStopped()
        {
            layeredAnimation.Stop();
            base.OnStopped();
        }

        public override void Update(TimeSpan elapsedTime)
        {
            layeredAnimation.Update(elapsedTime);

            value = Matrix.Identity;
            if (scaleX != null)
                value.M11 = scaleX.Value;
            if (scaleY != null)
                value.M22 = scaleX.Value;
            if (scaleZ != null)
                value.M33 = scaleX.Value;

            Matrix temp;
            if (RotationOrder == RotationOrder.Zxy)
            {
                Matrix.CreateFromYawPitchRoll(rotationY != null ? rotationY.Value : 0,
                                              rotationX != null ? rotationX.Value : 0,
                                              rotationZ != null ? rotationZ.Value : 0, out temp);
            }
            else
            {
                Matrix temp2;
                Matrix.CreateRotationY(rotationY != null ? rotationY.Value : 0, out temp);                
                Matrix.CreateRotationY(rotationX != null ? rotationX.Value : 0, out temp2);
                Matrix.Multiply(ref temp, ref temp2, out temp);
                Matrix.CreateRotationZ(rotationZ != null ? rotationZ.Value : 0, out temp2);
                Matrix.Multiply(ref temp, ref temp2, out temp);
            }
            Matrix.Multiply(ref value, ref temp, out value);

            if (translationX != null)
                value.M41 = translationX.Value;
            if (translationY != null)
                value.M42 = translationY.Value;
            if (translationZ != null)
                value.M43 = translationZ.Value;

            if (expression != null)
            {
                expression.Value = Value;
            }
        }
    }
}