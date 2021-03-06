﻿namespace Nine.Graphics.PostEffects
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents a bloom post processing effect.
    /// </summary>
    public class BloomEffect : PostEffectGroup
    {
        public float Threshold
        {
            get { return threshold.Threshold; }
            set { threshold.Threshold = value; }
        }

        public float BlurAmount
        {
            get { return blur.BlurAmount; }
            set { blur.BlurAmount = value; }
        }

        public float BloomIntensity
        {
            get { return bloom.BloomIntensity; }
            set { bloom.BloomIntensity = value; }
        }

        BlurEffect blur;
        ThresholdMaterial threshold;
        BloomMaterial bloom;

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomEffect"/> class.
        /// </summary>
        public BloomEffect(GraphicsDevice graphics)
        {
            Material = bloom = new BloomMaterial(graphics);
            Passes.Add(new PostEffectChain());
            Passes.Add(new PostEffectChain(TextureUsage.Bloom,
                new PostEffect() { Material = threshold = new ThresholdMaterial(graphics), RenderTargetScale = 0.5f, SurfaceFormat = SurfaceFormat.Color },
                blur = new BlurEffect(graphics),
                new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 2.0f }
            ));
        }

        [Nine.Serialization.NotBinarySerializable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new IList<PostEffectChain> Passes
        {
            // Prevent content serializer from loading passes.
            get { return base.Passes; }
        }

        [Nine.Serialization.NotBinarySerializable]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Material Material
        {
            get { return base.Material; }
            set { base.Material = value; }
        }
    }
}