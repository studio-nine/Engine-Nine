#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Transitions
{
    /// <summary>
    /// Interface for transitions.
    /// </summary>
    public interface ITransition : IUpdateObject
    {
        object Value { get; }
    }


    /// <summary>
    /// Generic interface for transitions
    /// </summary>
    public interface ITransition<T> : ITransition
    {
        T Value { get; }
    }


    public enum TransitionEffect
    {
        None,
        Loop,
        Pulse,
        Yoyo,
    }

    public delegate T Interpolate<T>(T x, T y, float amount);

    public abstract class Transition<T> : ITransition<T>, IAnimation
    {
        public T Start { get; set; }
        public T End { get; set; }
        public T Value { get; set; }

        private float Percentage;

        private TimeSpan ElapsedTime;

        public Interpolate<T> Lerp { get; set; }

        public TransitionEffect Effect { get; set; }
        
        object ITransition.Value
        {
            get { return Value; }
        }

        public Transition()
        {
            IsPlaying = true;
            Speed = 1.0f;
            Duration = TimeSpan.FromSeconds(1);
            Effect = TransitionEffect.None;
        }

        public Transition(T start, T end)
        {
            Start = start;
            End = end;
            IsPlaying = true;
            Speed = 1.0f;
            Duration = TimeSpan.FromSeconds(1);
            Effect = TransitionEffect.None;
        }

        public Transition(T start, T end, TimeSpan duration, TransitionEffect effect)
        {
            Start = start;
            End = end;
            IsPlaying = true;
            Speed = 1.0f;
            Duration = duration;
            Effect = effect;
        }

        public void Update(GameTime time)
        {
            if (IsPlaying)
            {
                ElapsedTime += time.ElapsedGameTime;

                TimeSpan elapsed = ElapsedTime;

                if (elapsed > TimeSpan.Zero)
                {
                    float position = (float)(Speed * elapsed.TotalMilliseconds / Duration.TotalMilliseconds);

                    Percentage = Evaluate(position);

                    if (Lerp != null)
                        Value = Lerp(Start, End, Percentage);
                    else
                        Value = (T)Interpolate(Start, End, Percentage);

                    if (position > 1.0f)
                    {
                        if (Effect == TransitionEffect.Yoyo || Effect == TransitionEffect.Pulse)
                        {
                            T temp = Start;
                            Start = End;
                            End = temp;

                            ElapsedTime -= Duration;

                            if (Effect == TransitionEffect.Pulse)
                                Effect = TransitionEffect.None;
                        }
                        else if (Effect == TransitionEffect.Loop)
                        {
                            ElapsedTime -= Duration;
                        }
                        else
                        {
                            if (Complete != null)
                                Complete(this, EventArgs.Empty);

                            IsPlaying = false;
                        }
                    }
                }
            }
        }

        protected abstract float Evaluate(float position);


        private static object Interpolate(object x, object y, float amount)
        {
            if (x is float || x is double || x is int)
                return MathHelper.Lerp((float)x, (float)y, amount);

            if (x is Vector2)
                return Vector2.Lerp((Vector2)x, (Vector2)y, amount);

            if (x is Vector3)
                return Vector3.Lerp((Vector3)x, (Vector3)y, amount);

            if (x is Vector4)
                return Vector4.Lerp((Vector4)x, (Vector4)y, amount);

            if (x is Matrix)
                return Matrix.Lerp((Matrix)x, (Matrix)y, amount);

            if (x is Quaternion)
                return Quaternion.Lerp((Quaternion)x, (Quaternion)y, amount);

            return null;
        }


        public float Speed { get; set; }
        private bool IsPlaying;
        public TimeSpan Duration { get; set; }

        public void Play()
        {
            Value = Start;
            ElapsedTime = TimeSpan.Zero;
            IsPlaying = true;
        }

        public event EventHandler Complete;
    }
}