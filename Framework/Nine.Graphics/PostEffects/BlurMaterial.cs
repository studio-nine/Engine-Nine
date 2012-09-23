namespace Nine.Graphics.Materials
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// A post processing screen effect that blurs the whole screen.
    /// </summary>
    [ContentSerializable]
    public partial class BlurMaterial
    {
        public const float MaxBlurAmount = 10;
        private const int MaxSampleCount = 15;

        /// <summary>
        /// Gets or sets the amount of blurring.
        /// </summary>
        public float BlurAmount
        {
            get { return blurAmount; }
            set { blurAmount = value; UpdateSampleCount(); }
        }

        /// <summary>
        /// Gets or sets a value indicating whether depth buffer will be sampled when blurring the scene.
        /// </summary>
        internal bool DepthBufferEnabled { get; set; }

        /// <summary>
        /// Gets or sets the direction of blurring in radians.
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

        partial void ApplyGlobalParameters(DrawingContext context)
        {
            // Need bilinear sampling to get the result correct.
            GraphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
        }

        partial void BeginApplyLocalParameters(DrawingContext context, BlurMaterial previousMaterial)
        {
            if ((GraphicsDevice.Textures[0] = texture) != null)
                SetBlurEffectParameters((float)Math.Cos(-Direction) / texture.Width, (float)Math.Sin(-Direction) / texture.Height);
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable Gaussian blur filter.
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
            for (int i = 0; i < sampleCount / 2; ++i)
            {
                // Store weights for the positive and negative taps.
                float weight = ComputeGaussian(i + 1);

                sampleWeights[i * 2 + 1] = weight;
                sampleWeights[i * 2 + 2] = weight;

                totalWeights += weight * 2;

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset;

                // Store texture coordinate offsets for the positive and negative taps.
                sampleOffsets[i * 2 + 1] = delta;
                sampleOffsets[i * 2 + 2] = -delta;
            }

            // Normalize the list of sample weightings, so they will always sum to one.
            for (int i = 0; i < sampleWeights.Length; ++i)
            {
                sampleWeights[i] /= totalWeights;
            }

            // Tell the effect about our new filter 
            effect.CurrentTechnique = DepthBufferEnabled ? effect.Techniques[shaderIndex + 8]
                                                         : effect.Techniques[shaderIndex];
            effect.sampleOffsets.SetValue(sampleOffsets);
            effect.sampleWeights.SetValue(sampleWeights);
        }

        /// <summary>
        /// Evaluates a single point on the Gaussian falloff curve.
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
            float minBlendWeight = 0.001f;
            float x = theta * (float)Math.Sqrt(-2 * Math.Log(minBlendWeight * theta * Math.Sqrt(2 * Math.PI)));

            // Divide by 2 because we sample 2 texels at a time
            shaderIndex = Math.Min((int)Math.Round(x / 2), (MaxSampleCount - 1) / 2);
            sampleCount = shaderIndex * 2 + 1;
        }
    }
}