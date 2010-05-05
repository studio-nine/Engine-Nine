#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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

namespace Isles.Transitions
{
    #region ITweener<T>
    /// <summary>
    /// Generic interface for transitions
    /// </summary>
    public interface ITweener<T>
    {
        T Update(GameTime time);
    }
    #endregion

    #region LoopStyle
    /// <summary>
    /// Defines how a transition loops.
    /// </summary>
    public enum LoopStyle
    {
        /// <summary>
        /// The transition stops when it reaches the end.
        /// </summary>
        None,

        /// <summary>
        /// The transition loops from start and won't stop.
        /// </summary>
        Loop,

        /// <summary>
        /// The transition loops backward to start, creating a pulse before it stops.
        /// </summary>
        Pulse,

        /// <summary>
        /// The transition loops back and forth all the time.
        /// </summary>
        Yoyo,
    }
    #endregion

    #region Easing
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
    #endregion
    
    #region Tweener<T>
    public sealed class Tweener<T> : ITweener<T>, IAnimation
    {
        private float percentage;
        private double elapsedTime;
        private Interpolate<T> lerp;

        public T From { get; set; }
        public T To { get; set; }        
        public T Value { get; private set; }

        public LoopStyle Style { get; set; }
        public Easing Easing { get; set; }

        public ICurve Curve { get; set; }
        
        public float Speed { get; set; }
        public bool IsPlaying { get; private set; }
        public TimeSpan Duration { get; set; }

        #region IAnimation
        public void Play()
        {
            Value = From;
            elapsedTime = 0;
            IsPlaying = true;
        }

        public void Stop()
        {
            Value = From;
            elapsedTime = 0;
            IsPlaying = false;
        }

        public void Pause()
        {
            IsPlaying = false;
        }

        public void Resume()
        {
            IsPlaying = true;
        }

        public event EventHandler Complete;
        #endregion

        public Tweener() : this(null) { }

        public Tweener(Interpolate<T> lerp)
        {
            if (lerp == null)
                SetDefaultLerp();
            else
                this.lerp = lerp;

            this.IsPlaying = true;
            this.Speed = 1.0f;
            this.Duration = TimeSpan.FromSeconds(1);
            this.Style = LoopStyle.None;
            this.Easing = Easing.In;

            if (this.lerp == null)
                throw new NotSupportedException("Type " + typeof(T).Name + " cannot be lerped.");
        }

        private void SetDefaultLerp()
        {
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
        
        public T Update(GameTime time)
        {
            if (!IsPlaying)
                return Value;

            if (Curve == null)
                Curve = new Linear();

            elapsedTime += time.ElapsedGameTime.TotalSeconds * Speed;

            float position = (float)(elapsedTime / Duration.TotalSeconds);

            bool ended = position >= 1.0f;

            position = MathHelper.Clamp(position, 0, 1);

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

            Value = lerp(From, To, percentage);

            if (ended)
            {
                if (Style == LoopStyle.Yoyo || Style == LoopStyle.Pulse)
                {
                    T temp = From;
                    From = To;
                    To = temp;

                    elapsedTime -= Duration.TotalSeconds;

                    if (Style == LoopStyle.Pulse)
                        Style = LoopStyle.None;
                }
                else if (Style == LoopStyle.Loop)
                {
                    elapsedTime -= Duration.TotalSeconds;
                }
                else
                {
                    IsPlaying = false;

                    if (Complete != null)
                        Complete(this, EventArgs.Empty);
                }
            }

            return Value;
        }
    }
    #endregion
}