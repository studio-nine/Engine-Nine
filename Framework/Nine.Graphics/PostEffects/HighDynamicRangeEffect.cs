namespace Nine.Graphics.PostEffects
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents a high dynamic range (HDR) post processing effect.
    /// </summary>
    [ContentSerializable]
    public class HighDynamicRangeEffect : PostEffectGroup
    {
        public float Threshold
        {
            get { return threshold.Threshold; }
            set { threshold.Threshold = value; }
        }

        public float BlurAmount
        {
            get { return blurH.BlurAmount; }
            set { blurH.BlurAmount = blurV.BlurAmount = value; }
        }

        public float AdoptionSpeed
        {
            get { return luminanceChain.AdoptionSpeed; }
            set { luminanceChain.AdoptionSpeed = value; }
        }

        public float Exposure
        {
            get { return toneMapping.Exposure; }
            set { toneMapping.Exposure = value; }
        }

        public float MaxLuminance
        {
            get { return toneMapping.MaxLuminance; }
            set { toneMapping.MaxLuminance = value; }
        }

        BlurMaterial blurH;
        BlurMaterial blurV;
        ThresholdMaterial threshold;
        ToneMappingMaterial toneMapping;
        LuminanceChain luminanceChain;

        /// <summary>
        /// Initializes a new instance of the <see cref="HighDynamicRangeEffect"/> class.
        /// </summary>
        public HighDynamicRangeEffect(GraphicsDevice graphics)
        {
            Material = toneMapping = new ToneMappingMaterial(graphics);
            Passes.Add(new PostEffectChain(TextureUsage.Bloom,
                new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 0.5f },
                new PostEffect() { Material = threshold = new ThresholdMaterial(graphics) },
                new PostEffect() { Material = blurH = new BlurMaterial(graphics) },
                new PostEffect() { Material = blurV = new BlurMaterial(graphics) { Direction = MathHelper.PiOver2 } },
                new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 2.0f }
            ));
            Passes.Add(luminanceChain = new LuminanceChain(graphics));
        }
    }
}