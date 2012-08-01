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
            get { return material.FocalPlane; }
            set { material.FocalPlane = value; }
        }

        public float FocalLength
        {
            get { return material.FocalLength; }
            set { material.FocalLength = value; }
        }

        public float FocalDistance
        {
            get { return material.FocalDistance; }
            set { material.FocalDistance = value; }
        }

        public float BlurAmount
        {
            get { return blur.BlurAmount; }
            set { blur.BlurAmount = value; }
        }

        public float Quality
        {
            get { return quality; }
            set { quality = value; blur.DepthBufferEnabled = (value > 0.5f); }
        }
        private float quality = 1;

        DepthOfFieldMaterial material;
        BlurEffect blur;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthOfFieldEffect"/> class.
        /// </summary>
        public DepthOfFieldEffect(GraphicsDevice graphics)
        {
            Material = new DepthOfFieldMaterial(graphics);
            Passes.Add(new PostEffectChain(TextureUsage.Blur,
                new PostEffect() { Material = material = new DepthOfFieldMaterial(graphics) { IsDownScale = true }, RenderTargetScale = 0.5f },
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