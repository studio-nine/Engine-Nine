namespace Nine.Graphics.PostEffects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

#if !WINDOWS_PHONE
    /// <summary>
    /// Defines a post processing effect that adapts to scene changes.
    /// </summary>
    public class AdaptationEffect : PostEffect
    {
        private RenderTarget2D lastFrame;
        private RenderTarget2D currentFrame;
        private AdaptionMaterial adoptionMaterial;
        private TextureMaterial textureMaterial;

        /// <summary>
        /// Get or sets the speed of the adaptation.
        /// </summary>
        public float Speed { get; set; }

        /// <summary>
        /// Creates a new instance of <c>AdaptationEffect</c>.
        /// </summary>
        public AdaptationEffect()
        {
            Speed = 1;
        }

        public override void GetActivePasses(IList<Pass> result)
        {
            // Enable this adaption effect even when Material is set to null.
            if (Enabled)
                result.Add(this);
        }

        public override RenderTarget2D PrepareRenderTarget(DrawingContext context, Texture2D input, SurfaceFormat? preferredFormat)
        {
            currentFrame = base.PrepareRenderTarget(context, input, preferredFormat);
            RenderTargetPool.Lock(currentFrame);
            return currentFrame;
        }

        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (Material == null)
                Material = adoptionMaterial = new AdaptionMaterial(context.graphics);
            else if (Material != adoptionMaterial)
                throw new InvalidOperationException();

            bool needLocalTexture = false;
            if (needLocalTexture = (currentFrame == null))
            {
                PrepareRenderTarget(context, InputTexture, null);
                currentFrame.Begin();
            }

            // Disable the adoption effect when we don't have a valid last frame texture
            // or when the adoption effect has been suspended for several frames.
#if SILVERLIGHT
            if (lastFrame == null || lastFrame.IsDisposed)
#else
            if (lastFrame == null || lastFrame.IsDisposed || lastFrame.IsContentLost)
#endif
            {
                CopyToScreen(context, drawables);
            }
            else
            {
                var graphics = context.graphics;
                graphics.Textures[0] = adoptionMaterial.texture;
                graphics.Textures[1] = lastFrame;
                graphics.SamplerStates[0] = graphics.SamplerStates[1] = SamplerState.PointClamp;
                adoptionMaterial.effect.Delta.SetValue((1 - (float)Math.Pow(0.98f, 30 * context.elapsedTime)) * Speed);

                base.Draw(context, drawables);

                graphics.SamplerStates[1] = context.SamplerState;
            }

            if (needLocalTexture)
            {
                currentFrame.End();

                InputTexture = currentFrame;
                CopyToScreen(context, drawables);
            }

            RenderTargetPool.Unlock(lastFrame);
            lastFrame = currentFrame;
            currentFrame = null;
        }

        private void CopyToScreen(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (textureMaterial == null)
                textureMaterial = new TextureMaterial(context.graphics) { SamplerState = SamplerState.PointClamp };
            Material = textureMaterial;
            base.Draw(context, drawables);
            Material = adoptionMaterial;
        }
    }
#endif
}

