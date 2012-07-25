namespace Nine.Graphics.PostEffects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;

    /// <summary>
    /// Represents luminance chain used in high dynamic range (HDR) post processing effect.
    /// </summary>
    [ContentSerializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class LuminanceChain : PostEffectChain
    {
        /// <summary>
        /// Gets or sets the speed that determine how fast the eye adapts to the changes
        /// of the luminance in the scene, or specify zero to disable adaption completely.
        /// </summary>
        public float AdaptionSpeed
        {
            get { return adaptionEffect.Speed; }
            set { adaptionEffect.Speed = value; adaptionEffect.Enabled = value > 0; }
        }
        private AdaptionEffect adaptionEffect;

        /// <summary>
        /// Initializes a new instance of the <see cref="LuminanceChain"/> class.
        /// </summary>
        public LuminanceChain(GraphicsDevice graphics)
        {
            int scale = 4;
            int size = (int)Math.Max(graphics.Viewport.Width / scale, graphics.Viewport.Height / scale);
            size = Math.Max(1, UtilityExtensions.UpperPowerOfTwo(size));

            Effects.Add(new PostEffect { Material = new LuminanceMaterial(graphics), RenderTargetSize = Vector2.One * size, SurfaceFormat = SurfaceFormat.Vector2 });
            size = Math.Max(1, size / scale);

            while (size >= 1)
            {
                Effects.Add(new PostEffect { Material = new LuminanceMaterial(graphics) { IsDownScale = true }, RenderTargetSize = Vector2.One * size });
                size /= scale;
            }

            Effects.Add(adaptionEffect = new AdaptionEffect() { RenderTargetSize = Vector2.One });
            TextureUsage = TextureUsage.Luminance;
        }

        [ContentSerializerIgnore]
        public override IList<PostEffect> Effects
        {
            get { return base.Effects; }
        }
    }
}