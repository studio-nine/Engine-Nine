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
    public sealed class LinearTransition<T> : Transition<T>
    {
        public LinearTransition() { }
        public LinearTransition(T start, T end) : base(start, end) { }
        public LinearTransition(T start, T end, TimeSpan duration, TransitionEffect effect) : base(start, end, duration, effect) { }

        protected override float Evaluate(float position)
        {
            return position;
        }
    }

    public sealed class ExponentialTransition<T> : Transition<T>
    {
        public float Power { get; set; }
        
        public ExponentialTransition() { Power = MathHelper.E; }
        public ExponentialTransition(T start, T end, float power) : base(start, end) { Power = power; }
        public ExponentialTransition(T start, T end, float power, TimeSpan duration, TransitionEffect effect) : base(start, end, duration, effect) { Power = power; }
        
        protected override float Evaluate(float position)
        {
            return (float)((Math.Pow(Power, position) - 1) / (Power - 1));
        }
    }

    public sealed class SinTransition<T> : Transition<T>
    {
        public SinTransition() { }
        public SinTransition(T start, T end) : base(start, end) { }
        public SinTransition(T start, T end, TimeSpan duration, TransitionEffect effect) : base(start, end, duration, effect) { }

        protected override float Evaluate(float position)
        {
            return (float)Math.Sin((position * 2 - 1) * MathHelper.PiOver2) * 0.5f + 0.5f;
        }
    }

    public sealed class SmoothTransition<T> : Transition<T>
    {
        public SmoothTransition() { }
        public SmoothTransition(T start, T end) : base(start, end) { }
        public SmoothTransition(T start, T end, TimeSpan duration, TransitionEffect effect) : base(start, end, duration, effect) { }

        protected override float Evaluate(float position)
        {
            return MathHelper.SmoothStep(0, 1, position);
        }
    }

    public sealed class CurveTransition<T> : Transition<T>
    {
        private float minPosition;
        private float maxPosition;
        private float minValue;
        private float maxValue;

        private Curve curve;

        public Curve Curve
        {
            get { return curve; }
            set
            {
                curve = value;

                if (curve.Keys.Count > 0)
                {
                    minPosition = float.MaxValue;
                    maxPosition = float.MinValue;
                    minValue = curve.Keys[0].Value;
                    maxValue = curve.Keys[curve.Keys.Count - 1].Value;

                    foreach (CurveKey key in curve.Keys)
                    {
                        if (key.Position < minPosition)
                            minPosition = key.Position;
                        if (key.Position > maxPosition)
                            maxPosition = key.Position;
                    }
                }
            }
        }

        public CurveTransition() { }
        public CurveTransition(T start, T end, Curve curve) : base(start, end) { Curve = curve; }
        public CurveTransition(T start, T end, Curve curve, TimeSpan duration, TransitionEffect effect) : base(start, end, duration, effect) { Curve = curve; }
                
        protected override float Evaluate(float position)
        {
            return Curve != null ? 
                (Curve.Evaluate(minPosition + position * (maxPosition - minPosition)) - minValue) / (maxValue - minValue) : 0;
        }
    }
}