namespace Nine.Graphics.PostEffects
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents a high dynamic range (HDR) post processing effect.
    /// </summary>
    public class HighDynamicRangeEffect : PostEffectGroup
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

        public bool AdaptationEnabled
        {
            get { return luminanceChain.AdaptationEnabled; }
            set { luminanceChain.AdaptationEnabled = value; }
        }

        public float AdaptationSpeed
        {
            get { return luminanceChain.AdaptationSpeed; }
            set { luminanceChain.AdaptationSpeed = value; }
        }

        public float Exposure
        {
            get { return toneMapping.Exposure; }
            set { toneMapping.Exposure = value; }
        }

        public float BloomIntensity
        {
            get { return toneMapping.BloomIntensity; }
            set { toneMapping.BloomIntensity = value; }
        }

        BlurEffect blur;
        ThresholdMaterial threshold;
        ToneMappingMaterial toneMapping;
        LuminanceChain luminanceChain;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighDynamicRangeEffect"/> class.
        /// </summary>
        public HighDynamicRangeEffect(GraphicsDevice graphics)
        {
            Material = toneMapping = new ToneMappingMaterial(graphics);
            Passes.Add(new PostEffectChain(Nine.Graphics.TextureUsage.Bloom,
                new PostEffect() { Material = threshold = new ThresholdMaterial(graphics), RenderTargetScale = 0.5f, SurfaceFormat = SurfaceFormat.Color },
                blur = new BlurEffect(graphics),
                new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 2.0f }
            ));
            Passes.Add(luminanceChain = new LuminanceChain(graphics));
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets the preferred surface format for the input texture.
        /// </summary>
        public override SurfaceFormat? InputFormat
        {
            get { return SurfaceFormat.HdrBlendable; }
        }
#endif

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