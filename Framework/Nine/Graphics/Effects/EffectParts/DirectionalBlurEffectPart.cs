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
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    public class DirectionalBlurEffectPart : LinkedEffectPart, IEffectTexture
    {
        private uint dirtyMask = 0;
        
        private float blurAmount;
        private float step;
        private float direction;
        private Texture2D texture;

        private EffectParameter sampleOffsetsParameter;
        private EffectParameter sampleWeightsParameter;
        private EffectParameter textureParameter;

        private const uint blurDirtyMask = 1 << 0;
        private const uint textureDirtyMask = 1 << 1;
                
        [ContentSerializer(Optional = true)]
        public float BlurAmount
        {
            get { return blurAmount; }
            set { blurAmount = value; dirtyMask |= blurDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Step
        {
            get { return step; }
            set { step = value; dirtyMask |= blurDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Direction
        {
            get { return direction; }
            set { direction = value; dirtyMask |= blurDirtyMask; }
        }

        [ContentSerializerIgnore]
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; dirtyMask |= textureDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public int SampleCount { get; private set; }

        protected internal override void OnApply()
        {
            if ((dirtyMask & blurDirtyMask) != 0)
            {
                if (sampleOffsetsParameter == null)
                    sampleOffsetsParameter = GetParameter("SampleOffsets");
                if (sampleWeightsParameter == null)
                    sampleWeightsParameter = GetParameter("SampleWeights");

                SetBlurEffectParameters(
                    (float)Math.Cos(-Direction) / GraphicsDevice.Viewport.Width,
                    (float)Math.Sin(-Direction) / GraphicsDevice.Viewport.Height);

                dirtyMask &= ~blurDirtyMask;
            }

            if ((dirtyMask & textureDirtyMask) != 0)
            {
                if (textureParameter == null)
                    textureParameter = GetParameter("Texture");
                textureParameter.SetValue(texture);
                dirtyMask &= ~textureDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new DirectionalBlurEffectPart()
            {
                Direction = this.Direction,
                Step = this.Step,
                BlurAmount = this.BlurAmount,
                SampleCount = this.SampleCount,
                Texture = this.Texture,
            };
        }

        /// <summary>
        /// Computes sample weightings and texture coordinate offsets
        /// for one pass of a separable gaussian blur filter.
        /// </summary>
        void SetBlurEffectParameters(float dx, float dy)
        {
            // Look up how many samples our gaussian blur effect supports.
            //int sampleCount = weightsParameter.Elements.Count;
            int sampleCount = SampleCount;

            // Create temporary arrays for computing our filter 
            float[] sampleWeights = new float[sampleCount];
            Vector2[] sampleOffsets = new Vector2[sampleCount];

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

                // To get the maximum amount of blurring from a limited number of
                // pixel shader samples, we take advantage of the bilinear filtering
                // hardware inside the texture fetch unit. If we position our texture
                // coordinates exactly halfway between two texels, the filtering unit
                // will average them for us, giving two samples for the price of one.
                // This allows us to step in units of two texels per sample, rather
                // than just one at a time. The 1.5 offset kicks things off by
                // positioning us nicely in between two texels.
                float sampleOffset = i * 2 + 1.5f;

                Vector2 delta = new Vector2(dx, dy) * sampleOffset * Step;

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
            sampleOffsetsParameter.SetValue(sampleOffsets);
            sampleWeightsParameter.SetValue(sampleWeights);
        }


        /// <summary>
        /// Evaluates a single point on the gaussian falloff curve.
        /// Used for setting up the blur filter weightings.
        /// </summary>
        float ComputeGaussian(float n)
        {
            float theta = BlurAmount;

            return (float)((1.0 / Math.Sqrt(2 * Math.PI * theta)) * Math.Exp(-(n * n) / (2 * theta * theta)));
        }

        void IEffectTexture.SetTexture(string name, Texture texture)
        {

        }
    }

#endif
}
