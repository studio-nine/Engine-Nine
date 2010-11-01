#region Copyright 2009 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 (c) Engine Nine. All Rights Reserved.
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

namespace Nine
{
    public interface ICurve
    {
        float Evaluate(float position);
    }

    public class LinearCurve : ICurve
    {
        public float Evaluate(float position)
        {
            return position;
        }
    }

    public class ExponentialCurve : ICurve
    {
        public float Power { get; set; }
        
        public ExponentialCurve() { Power = 1.0f / 32; }

        public float Evaluate(float position)
        {
            return (float)((Math.Pow(Power, position) - 1) / (Power - 1));
        }
    }

    public class SinCurve : ICurve
    {
        public float Evaluate(float position)
        {
            return (float)Math.Sin((position * 2 - 1) * MathHelper.PiOver2) * 0.5f + 0.5f;
        }
    }

    public class SmoothCurve : ICurve
    {
        public float Evaluate(float position)
        {
            return MathHelper.SmoothStep(0, 1, position);
        }
    }

    public class ElasticCurve : ICurve
    {
        public float Strength { get; set; }

        public ElasticCurve() { Strength = 0.2f; }

        public float Evaluate(float position)
        {
            float a = 1.0f + Strength;
            float w = MathHelper.Pi - (float)Math.Asin(1.0f / a);

            return a * (float)Math.Sin(position * w);
        }
    }

    public class BounceCurve : ICurve
    {
        public float Strength { get; set; }

        public BounceCurve() { Strength = 0.2f; }

        public float Evaluate(float position)
        {
            float a = 1.0f + Strength;
            float w = MathHelper.Pi - (float)Math.Asin(1.0f / a);

            float y = a * (float)Math.Sin(position * w);

            return y < 1.0f ? y : y = 2.0f - y;
        }
    }

    public class CustomCurve : ICurve
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

        public CustomCurve(Curve curve)
        {
            Curve = curve;
        }

        public float Evaluate(float position)
        {
            return Curve != null ? 
                (Curve.Evaluate(minPosition + position * (maxPosition - minPosition)) - minValue) / (maxValue - minValue) : 0;
        }
    }
}