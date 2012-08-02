namespace Nine.Graphics.PostEffects
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Microsoft.Xna.Framework.Content;
    using System.Collections.Generic;

    /// <summary>
    /// Represents a depth of field post processing effect.
    /// </summary>
    public class DepthOfFieldEffect : PostEffectGroup
    {        
        public float FocalPlane
        {
            get { return depthScale.FocalPlane; }
            set { depthScale.FocalPlane = combine.FocalPlane = value; }
        }

        public float FocalLength
        {
            get { return depthScale.FocalLength; }
            set { depthScale.FocalLength = combine.FocalLength = value; }
        }

        public float FocalDistance
        {
            get { return depthScale.FocalDistance; }
            set { depthScale.FocalDistance = combine.FocalDistance = value; }
        }

        public float BlurAmount
        {
            get { return blur.BlurAmount; }
            set { blur.BlurAmount = value; }
        }

        public float Quality
        {
            get { return quality; }
            set 
            {
                quality = value;
                scaleEffect.Enabled = !(depthScaleEffect.Enabled = blur.DepthBufferEnabled = (value > 0.5f));
            }
        }
        private float quality = 1;

        PostEffect scaleEffect;
        PostEffect depthScaleEffect;
        DepthOfFieldMaterial depthScale;
        DepthOfFieldMaterial combine;
        BlurEffect blur;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthOfFieldEffect"/> class.
        /// </summary>
        public DepthOfFieldEffect(GraphicsDevice graphics)
        {
            Material = combine = new DepthOfFieldMaterial(graphics);
            Passes.Add(new PostEffectChain(TextureUsage.Blur,
                depthScaleEffect = new PostEffect() { Material = depthScale = new DepthOfFieldMaterial(graphics) { IsDownScale = true }, RenderTargetScale = 0.5f },
                scaleEffect = new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 0.5f, Enabled = false },
                blur = new BlurEffect(graphics) { DepthBufferEnabled = true },
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