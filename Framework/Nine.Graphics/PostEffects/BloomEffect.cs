namespace Nine.Graphics.PostEffects
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents a bloom post processing effect.
    /// </summary>
    [ContentSerializable]
    public class BloomEffect : PostEffectGroup
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

        BlurMaterial blurH;
        BlurMaterial blurV;
        ThresholdMaterial threshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomEffect"/> class.
        /// </summary>
        public BloomEffect(GraphicsDevice graphics)
        {
            Passes.Add(new PostEffectChain());
            Passes.Add(new PostEffectChain(BlendState.Additive,
                new PostEffect() { Material = threshold = new ThresholdMaterial(graphics), RenderTargetScale = 0.5f, SurfaceFormat = SurfaceFormat.Color },
                new PostEffect() { Material = blurH = new BlurMaterial(graphics) },
                new PostEffect() { Material = blurV = new BlurMaterial(graphics) { Direction = MathHelper.PiOver2 } },
                new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 2.0f }
            ));
        }

        [ContentSerializerIgnore]
        public override IList<PostEffectChain> Passes
        {
            // Prevent content serializer from loading passes.
            get { return base.Passes; }
        }

        [ContentSerializerIgnore]
        public override Material Material
        {
            get { return base.Material; }
            set { base.Material = value; }
        }
    }
}