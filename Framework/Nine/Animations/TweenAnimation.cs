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
#endregion

namespace Nine.Animations
{
    /// <summary>
    /// Defines in which direction will the transition be eased.
    /// </summary>
    public enum Easing
    {
        /// <summary>
        /// Eased when transiting in.
        /// </summary>
        In,

        /// <summary>
        /// Eased when transiting out.
        /// </summary>
        Out,

        /// <summary>
        /// Eased when both transiting in and out.
        /// </summary>
        InOut,
    }
    
    /// <summary>
    /// Defines a basic primitive animation that changes it value 
    /// base on time.
    /// </summary>
    public class TweenAnimation<T> : TimelineAnimation where T : struct
    {
        private T from;
        private T to;
        private MemberInfo member;
        private Interpolate<T> lerp;
        private Operation<T> add;

        /// <summary>
        /// Gets or sets where the tween starts.
        /// Specify null indicates the tweener will use the current
        /// property value of the object been animated.
        /// </summary>
        public T? From { get; set; }

        /// <summary>
        /// Gets or sets where then tween ends.
        /// Specify null when you want to control end position using <c>By</c>.
        /// </summary>
        public T? To { get; set; }

        /// <summary>
        /// Gets or sets where then tween ends relative to start position.
        /// Specify null when you want to control end position using <c>To</c>.
        /// </summary>
        public T? By { get; set; }

        /// <summary>
        /// Gets the current value of this tween animation.
        /// </summary>
        public T Value { get; private set; }

        /// <summary>
        /// Gets or sets the easing mode used by this tween animation.
        /// </summary>
        public Easing Easing { get; set; }

        /// <summary>
        /// Gets or sets the curve that controls the detailed motion of this
        /// tween animation.
        /// </summary>
        public ICurve Curve { get; set; }

        /// <summary>
        /// Gets or sets the duration of this animation.
        /// </summary>
        public new TimeSpan Duration { get; set; }

        /// <summary>
        /// Gets or sets the target object that this tweening will affect.
        /// This property is not required.
        /// </summary>
        public object Target { get; set; }

        /// <summary>
        /// Gets or sets the property or field name of the target object.
        /// The property or field must be publicly visible.
        /// This property is not required.
        /// </summary>
        public string TargetProperty { get; set; }

        /// <summary>
        /// Create a new instance of tweener.
        /// </summary>
        public TweenAnimation() : this(null, null) { }

        /// <summary>
        /// Create a new instance of tweener.
        /// </summary>
        public TweenAnimation(Interpolate<T> lerp, Operation<T> add)
        {
            this.lerp = lerp;

            if (lerp == null)
                SetDefaultLerp();

            if (this.lerp == null)
                throw new NotSupportedException(
                    "Type " + typeof(T).Name + " cannot be lerped by default. " +
                    "Please specify a interpolation method.");

            this.add = add;
            this.Duration = TimeSpan.FromSeconds(1);
            this.Easing = Easing.In;
            this.Curve = new LinearCurve();
        }

        protected sealed override TimeSpan GetDuration()
        {
            return Duration;
        }

        protected override void OnStarted()
        {
            // Extract a valid property target
            if (Target != null && !string.IsNullOrEmpty(TargetProperty))
            {
                member = Target.GetType().GetProperty(TargetProperty);

                if (member == null)
                    member = Target.GetType().GetField(TargetProperty);

                if (member == null)
                {
                    throw new ArgumentException(
                        "Type " + Target.GetType().Name +
                        " does not have a valid public property or field: " + TargetProperty);
                }
            }

            // Initialize from
            if (From.HasValue)
                from = From.Value;
            else if (member != null)
                from = GetValue();
            else
                throw new InvalidOperationException(
                    "When From is set to null, a valid Target and TargetProperty must be set");
            
            // Initialize to
            if (To.HasValue)
                to = To.Value;
            else if (By.HasValue)
                to = ReflectionAdd(from, By.Value);
            else
                throw new InvalidOperationException("Either To or By must be specified");

            base.OnStarted();
        }

        protected override void OnSeek(TimeSpan elapsedTime, TimeSpan previousPosition)
        {
            if (Curve == null)
                throw new ArgumentNullException("Curve cannot be null");

            float percentage = 0;
            float position = (float)(elapsedTime.TotalSeconds / Duration.TotalSeconds);

            switch (Easing)
            {
                case Easing.In:
                    percentage = Curve.Evaluate(position);
                    break;
                case Easing.Out:
                    percentage = 1.0f - Curve.Evaluate(1.0f - position);
                    break;
                case Easing.InOut:
                    percentage = position < 0.5f ? (0.5f - Curve.Evaluate(1.0f - position * 2) * 0.5f) :
                                                   (0.5f + Curve.Evaluate((position - 0.5f) * 2) * 0.5f);
                    break;
            }

            SetValue(Value = lerp(from, to, percentage));
        }

        private T ReflectionAdd(T x, T y)
        {
            if (add == null)
                SetDefaultAdd();

            if (add == null)
                throw new NotSupportedException(
                    "Type " + typeof(T).Name + " cannot be added with an offset. " +
                    "Please specify an add operater in the constructor.");

            return add(x, y);
        }

        private void SetDefaultLerp()
        {
            // This is a generic limitaion,
            // we have to assign the lerp using reflection.
            FieldInfo field = GetType().GetField("lerp", BindingFlags.NonPublic | BindingFlags.Instance);

            if (typeof(T) == typeof(float))
                field.SetValue(this, (Interpolate<float>)MathHelper.Lerp);
            else if (typeof(T) == typeof(double))
                field.SetValue(this, (Interpolate<double>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(int))
                field.SetValue(this, (Interpolate<int>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(Vector2))
                field.SetValue(this, (Interpolate<Vector2>)Vector2.Lerp);
            else if (typeof(T) == typeof(Vector3))
                field.SetValue(this, (Interpolate<Vector3>)Vector3.Lerp);
            else if (typeof(T) == typeof(Vector4))
                field.SetValue(this, (Interpolate<Vector4>)Vector4.Lerp);
            else if (typeof(T) == typeof(Matrix))
                field.SetValue(this, (Interpolate<Matrix>)Matrix.Lerp);
            else if (typeof(T) == typeof(Quaternion))
                field.SetValue(this, (Interpolate<Quaternion>)Quaternion.Lerp);
            else if (typeof(T) == typeof(Color))
                field.SetValue(this, (Interpolate<Color>)Color.Lerp);
            else if (typeof(T) == typeof(Rectangle))
                field.SetValue(this, (Interpolate<Rectangle>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(Point))
                field.SetValue(this, (Interpolate<Point>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(Ray))
                field.SetValue(this, (Interpolate<Ray>)LerpHelper.Lerp);
        }

        private void SetDefaultAdd()
        {
            // This is a generic limitaion,
            // we have to assign the lerp using reflection.
            FieldInfo field = GetType().GetField("add", BindingFlags.NonPublic | BindingFlags.Instance);

            if (typeof(T) == typeof(float))
                field.SetValue(this, (Operation<float>)AddHelper.Add);
            else if (typeof(T) == typeof(double))
                field.SetValue(this, (Operation<double>)AddHelper.Add);
            else if (typeof(T) == typeof(int))
                field.SetValue(this, (Operation<int>)AddHelper.Add);
            else if (typeof(T) == typeof(Vector2))
                field.SetValue(this, (Operation<Vector2>)Vector2.Add);
            else if (typeof(T) == typeof(Vector3))
                field.SetValue(this, (Operation<Vector3>)Vector3.Add);
            else if (typeof(T) == typeof(Vector4))
                field.SetValue(this, (Operation<Vector4>)Vector4.Add);
            else if (typeof(T) == typeof(Matrix))
                field.SetValue(this, (Operation<Matrix>)Matrix.Multiply);
            else if (typeof(T) == typeof(Quaternion))
                field.SetValue(this, (Operation<Quaternion>)Quaternion.Multiply);
            else if (typeof(T) == typeof(Color))
                field.SetValue(this, (Operation<Color>)AddHelper.Add);
            else if (typeof(T) == typeof(Rectangle))
                field.SetValue(this, (Operation<Rectangle>)AddHelper.Add);
            else if (typeof(T) == typeof(Point))
                field.SetValue(this, (Operation<Point>)AddHelper.Add);
            else if (typeof(T) == typeof(Ray))
                field.SetValue(this, (Operation<Ray>)AddHelper.Add);
        }

        private T GetValue()
        {
            if (Target != null)
            {
                if (member is FieldInfo)
                {
                    return (T)(member as FieldInfo).GetValue(Target);
                }

                if (member is PropertyInfo)
                {
                    return (T)(member as PropertyInfo).GetValue(Target, null);
                }
            }

            return default(T);
        }

        private void SetValue(T value)
        {
            if (Target != null)
            {
                if (member is FieldInfo)
                {
                    (member as FieldInfo).SetValue(Target, value);
                }
                else if (member is PropertyInfo)
                {
                    (member as PropertyInfo).SetValue(Target, value, null);
                }
            }
        }
    }
}