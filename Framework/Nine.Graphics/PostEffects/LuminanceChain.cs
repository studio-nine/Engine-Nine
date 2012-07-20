namespace Nine.Graphics.PostEffects
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
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
        /// Initializes a new instance of the <see cref="LuminanceChain"/> class.
        /// </summary>
        public LuminanceChain(GraphicsDevice graphics)
        {
            int scale = 2;
            int size = (int)MathHelper.Min(graphics.Viewport.Width / scale, graphics.Viewport.Height / scale);
            size = UtilityExtensions.UpperPowerOfTwo(size);

            Effects.Add(new PostEffect()
            {
                Material = new LuminanceMaterial(graphics),
                RenderTargetSize = Vector2.One * size,
                SurfaceFormat = SurfaceFormat.Vector2,
            });
            size = Math.Max(1, size / scale);

            while (size >= 1)
            {
                Effects.Add(new PostEffect()
                {
                    Material = new ScaleMaterial(graphics),
                    RenderTargetSize = Vector2.One * size,
                });
                size /= scale;
            }
            /*
            if (adoption.HasValue)
            {
                luminancePass.Effects.Add(new AdoptionEffect(graphics)
                {
                    Speed = adoption.Value,
                    RenderTargetSize = Vector2.One,
                });
            }
            */

            TextureUsage = TextureUsage.Luminance;
        }
    }
}