namespace Nine.Graphics.PostEffects
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents a blur post processing effect.
    /// </summary>
    [ContentSerializable]
    public class BlurEffect : PostEffectChain
    {
        public float BlurAmount
        {
            get { return blurH.BlurAmount; }
            set { blurH.BlurAmount = blurV.BlurAmount = value; }
        }

        BlurMaterial blurH;
        BlurMaterial blurV;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlurEffect"/> class.
        /// </summary>
        public BlurEffect(GraphicsDevice graphics)
        {
            Effects.Add(new PostEffect(blurH = new BlurMaterial(graphics)));
            Effects.Add(new PostEffect(blurV = new BlurMaterial(graphics) { Direction = MathHelper.PiOver2 }));
        }

        [ContentSerializerIgnore]
        public override IList<PostEffect> Effects
        {
            get { return base.Effects; }
        }
    }
}