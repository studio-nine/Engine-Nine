#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
using Nine.Graphics.ObjectModel;
using DirectionalLight = Nine.Graphics.ObjectModel.DirectionalLight;
#endregion

namespace Nine.Graphics.Materials
{
    /// <summary>    
    /// A post processing screen effect that blurs the whole screen.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class BlurMaterial
    {
        /// <summary>
        /// Gets or sets the amount of bluring.
        /// </summary>
        public float BlurAmount
        {
            get { return blurAmount; }
            set { blurAmount = value; UpdateSampleCount(); }
        }

        /// <summary>
        /// Gets or sets the direction of bluring in radians.
        /// </summary>
        public float Direction { get; set; }

        private float blurAmount;
        private int shaderIndex;
        private int sampleCount;
        private float[] sampleWeights;
        private Vector2[] sampleOffsets;

        partial void OnCreated()
        {
            BlurAmount = 2;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, BlurMaterial previousMaterial)
        {
            SetBlurEffectParameters(
                (float)Math.Cos(-Direction) / context.GraphicsDevice.Viewport.Width,
                (float)Math.Sin(-Direction) / context.GraphicsDevice.Viewport.Height);
        }
        
        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Create temporary arrays for computing our filter 
            if (sampleWeights == null || sampleWeights.Length < sampleCount)
            {
                sampleWeights = new float[sampleCount];
                sampleOffsets = new Vector2[sampleCount];
            }

            // The first sample always has a zero offset.
            sampleWeights[0] = ComputeGaussian(0);
            sampleOffsets[0] = new Vector2(0);

            // Maintain a sum of all the weighting values.
            float totalWeights = sampleWeights[0];

            // Add pairs of additional sample taps, positioned
            // along a line in both directions from the center.
            for (int i = 0; i < sampleCount / 2; i++)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;
                
                Vector2 delta = new Vector2(dx, dy) * (i + 1.0f);

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; i++)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter 
            effect.shaderIndex.SetValue(shaderIndex);
            effect.sampleOffsets.SetValue(sampleOffsets);
            effect.sampleWeights.SetValue(sampleWeights);
        }

        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        private float ComputeGaussian(float n)
        {
            float theta = Math.Max(BlurAmount, 0.0001f);

            // Gaussian filter http://en.wikipedia.org/wiki/Gaussian_filter
            return (float)((1.0 / Math.Sqrt(2 * Math.PI) / theta) * Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        /// <summary>
        /// Estimate the sample count for the given blur amount.
        /// </summary>
        private void UpdateSampleCount()
        {
            float theta = Math.Max(BlurAmount, 0.0001f);

            int MaxSampleCount = 15;

            // 2.146 = Math.Sqrt(-2 * Math.Log(0.1))
            shaderIndex = Math.Min((int)(blurAmount * 2.146f), (MaxSampleCount - 1) / 2);
            sampleCount = shaderIndex * 2 + 1;
        }
    }
}