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
    public enum TransitionEffect
    {
        None,
        Loop,
        Pulse,
        Yoyo,
    }


    public abstract class Transition<T> : ITransition<T>, IAnimation
    {
        public T Start { get; set; }
        public T End { get; set; }
        public T Value { get; set; }

        public float Percentage { get; private set; }

        public TimeSpan ElapsedTime { get; private set; }
        public TimeSpan Delay { get; set; }

        public delegate T Lerp(T x, T y, float amount);

        public Lerp LerpFunction { get; set; }

        public TransitionEffect Effect { get; set; }


        public Transition()
        {
            IsPlaying = true;
            Speed = 1.0f;
            Duration = TimeSpan.FromSeconds(1);
            Delay = TimeSpan.Zero;
            Effect = TransitionEffect.None;
        }

        public virtual T Update(GameTime time)
        {
            if (IsPlaying)
            {
                ElapsedTime += time.ElapsedGameTime;

                TimeSpan elapsed = ElapsedTime - Delay;

                if (elapsed > TimeSpan.Zero)
                {
                    float position = (float)(Speed * elapsed.TotalMilliseconds / Duration.TotalMilliseconds);

                    Percentage = Evaluate(position);

                    if (LerpFunction != null)
                        Value = LerpFunction(Start, End, Percentage);
                    else
                        Value = (T)Interpolate(Start, End, Percentage);

                    if (position > 1.0f)
                    {
                        if (Complete != null)
                            Complete(this, EventArgs.Empty);

                        if (Effect == TransitionEffect.Yoyo || Effect == TransitionEffect.Pulse)
                        {
                            T temp = Start;
                            Start = End;
                            End = temp;

                            ElapsedTime -= (Duration + Delay);

                            if (Effect == TransitionEffect.Pulse)
                                Effect = TransitionEffect.None;
                        }
                        else if (Effect == TransitionEffect.Loop)
                        {
                            ElapsedTime -= (Duration + Delay);
                        }
                        else
                        {
                            Stop();
                        }
                    }
                }
            }

            return Value;
        }

        public abstract float Evaluate(float position);


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

        #region IAnimation Members

        public float Speed { get; set; }
        public bool IsPlaying { get; set; }
        public TimeSpan Duration { get; set; }

        public void Play()
        {
            Value = Start;
            ElapsedTime = TimeSpan.Zero;
            IsPlaying = true;
        }

        public void Stop()
        {
            IsPlaying = false;
        }

        public void Resume()
        {
            IsPlaying = true;
        }

        public event EventHandler Complete;

        #endregion
    }
}