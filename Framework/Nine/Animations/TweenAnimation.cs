namespace Nine.Animations
{
    using System;
    using System.Reflection;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;


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
    /// Implements a basic primitive animation that changes its value over time.
    /// Can also update the value of a named target property on an target object.
    /// </summary>
    [ContentSerializable]
    public class TweenAnimation<T> : TimelineAnimation, ISupportTarget where T : struct
    {
        private T from;
        private T to;
        private object target;
        private string targetProperty;
        private bool expressionChanged;
        private PropertyExpression<T> expression;
        private Interpolate<T> lerp;
        private Operator<T> add;
        private ICurve curve = Curves.Linear;

        /// <summary>
        /// Gets or sets where the tween starts.
        /// Specify null to use the current property value of the object being animated.
        /// </summary>
        public T? From { get; set; }

        /// <summary>
        /// Gets or sets where then tween ends.
        /// Specify null to control the end position using <c>By</c>.

        /// </summary>
        public T? To { get; set; }

        /// <summary>
        /// Gets or sets where the tween ends relative to start position.
        /// Specify null to control the end position using <c>To</c>.

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
        public ICurve Curve
        {
            get { return curve; }
            set { curve = value ?? Curves.Linear; }
        }

        /// <summary>
        /// Gets or sets the duration of this animation.
        /// </summary>
        public new TimeSpan Duration 
        {
            get { return base.TotalDuration; }
            set { base.TotalDuration = value; }
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
        /// Create a new instance of tweener.
        /// </summary>
        public TweenAnimation() : this(null, null) { }

        /// <summary>
        /// Create a new instance of tweener.
        /// </summary>
        public TweenAnimation(Interpolate<T> lerp, Operator<T> add)
        {
            this.lerp = lerp;

            if (lerp == null)
                SetDefaultLerp();

            if (this.lerp == null)
                throw new NotSupportedException(
                    "Type " + typeof(T).Name + " cannot be lerped by default. " +
                    "Please specify a interpolation method.");

            this.add = add;
            this.Easing = Easing.In;
            this.Curve = new LinearCurve();
            this.Repeat = 1;
            this.Duration = TimeSpan.FromSeconds(1);
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
                    expression = new PropertyExpression<T>(Target, TargetProperty);
                    expressionChanged = false;
                }
            }

            // Initialize from
            if (From.HasValue)
                from = From.Value;
            else if (expression != null)
                from = (T)expression.Value;
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

        /// <summary>
        /// Called when seek.
        /// </summary>
        protected override void OnSeek(TimeSpan currentPosition, TimeSpan previousPosition)
        {
            float percentage = 0;
            float position = (float)(Position.TotalSeconds / Duration.TotalSeconds);

            if (!float.IsNaN(position))
            {
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
            }

            Value = lerp(from, to, percentage);
            if (expression != null)
                expression.Value = Value;
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

            FieldInfo field = typeof(TweenAnimation<T>).GetField("lerp", BindingFlags.NonPublic | BindingFlags.Instance);

            if (typeof(T) == typeof(float))
                field.SetValue(this, (Interpolate<float>)MathHelper.Lerp);
            else if (typeof(T) == typeof(double))
                field.SetValue(this, (Interpolate<double>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(bool))
                field.SetValue(this, (Interpolate<bool>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(int))
                field.SetValue(this, (Interpolate<int>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(char))
                field.SetValue(this, (Interpolate<char>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(byte))
                field.SetValue(this, (Interpolate<byte>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(short))
                field.SetValue(this, (Interpolate<short>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(long))
                field.SetValue(this, (Interpolate<long>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(decimal))
                field.SetValue(this, (Interpolate<decimal>)LerpHelper.Lerp);
            else if (typeof(T) == typeof(Vector2))
                field.SetValue(this, (Interpolate<Vector2>)Vector2.Lerp);
            else if (typeof(T) == typeof(Vector3))
                field.SetValue(this, (Interpolate<Vector3>)Vector3.Lerp);
            else if (typeof(T) == typeof(Vector4))
                field.SetValue(this, (Interpolate<Vector4>)Vector4.Lerp);
            else if (typeof(T) == typeof(Matrix))
                field.SetValue(this, (Interpolate<Matrix>)LerpHelper.Slerp);
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

            FieldInfo field = typeof(TweenAnimation<T>).GetField("add", BindingFlags.NonPublic | BindingFlags.Instance);

            if (typeof(T) == typeof(float))
                field.SetValue(this, (Operator<float>)AddHelper.Add);
            else if (typeof(T) == typeof(double))
                field.SetValue(this, (Operator<double>)AddHelper.Add);
            else if (typeof(T) == typeof(int))
                field.SetValue(this, (Operator<int>)AddHelper.Add);
            else if (typeof(T) == typeof(Vector2))
                field.SetValue(this, (Operator<Vector2>)Vector2.Add);
            else if (typeof(T) == typeof(Vector3))
                field.SetValue(this, (Operator<Vector3>)Vector3.Add);
            else if (typeof(T) == typeof(Vector4))
                field.SetValue(this, (Operator<Vector4>)Vector4.Add);
            else if (typeof(T) == typeof(Matrix))
                field.SetValue(this, (Operator<Matrix>)Matrix.Multiply);
            else if (typeof(T) == typeof(Quaternion))
                field.SetValue(this, (Operator<Quaternion>)Quaternion.Multiply);
            else if (typeof(T) == typeof(Color))
                field.SetValue(this, (Operator<Color>)AddHelper.Add);
            else if (typeof(T) == typeof(Rectangle))
                field.SetValue(this, (Operator<Rectangle>)AddHelper.Add);
            else if (typeof(T) == typeof(Point))
                field.SetValue(this, (Operator<Point>)AddHelper.Add);
            else if (typeof(T) == typeof(Ray))
                field.SetValue(this, (Operator<Ray>)AddHelper.Add);
        }
    }

#if !TEXT_TEMPLATE
    partial class TweenAnimationReader<T> where T : struct { }
#endif
}