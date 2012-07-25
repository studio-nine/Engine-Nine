namespace Nine.Graphics.PostEffects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.ObjectModel;

#if !WINDOWS_PHONE
    /// <summary>
    /// Defines a post processing effect that adopt scene changes.
    /// </summary>
    [ContentSerializable]
    public class AdoptionEffect : PostEffect
    {
        private RenderTarget2D lastFrame;
        private RenderTarget2D currentFrame;
        private AdoptionMaterial adoptionMaterial;        
        private BasicMaterial basicMaterial;

        /// <summary>
        /// Get or sets the speed of the adoptation.
        /// </summary>
        public float Speed { get; set; }
        
        /// <summary>
        /// Creates a new instance of <c>AdoptationEffect</c>.
        /// </summary>
        public AdoptionEffect()
        {
            Speed = 10;
            SamplerState = SamplerState.PointClamp;
        }

        public override void GetActivePasses(IList<Pass> result)
        {
            // Enable this adoption effect even when Material is set to null.
            if (Enabled)
                result.Add(this);
        }

        public override RenderTarget2D PrepareRenderTarget(DrawingContext context, Texture2D input)
        {
            currentFrame = base.PrepareRenderTarget(context, input);
            RenderTargetPool.Lock(currentFrame);
            return currentFrame;
        }
        
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (Material == null)
                Material = adoptionMaterial = new AdoptionMaterial(context.GraphicsDevice);
            else if (Material != adoptionMaterial)
                throw new InvalidOperationException();

            bool needLocalTexture = false;

            try
            {
                if (needLocalTexture = (currentFrame == null))
                {
                    PrepareRenderTarget(context, InputTexture);
                    currentFrame.Begin();
                }

                // Disable the adoption effect when we don't have a valid last frame texture
                // or when the adoption effect has been suspended for several frames.
                if (lastFrame == null || lastFrame.IsDisposed || lastFrame.IsContentLost)
                {
                    CopyToScreen(context, drawables);
                }
                else
                {
                    context.GraphicsDevice.Textures[1] = lastFrame;
                    context.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;

                    adoptionMaterial.effect.Delta.SetValue(context.ElapsedSeconds * Speed);
                    base.Draw(context, drawables);
                }
            }
            finally
            {
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
        }

        private void CopyToScreen(DrawingContext context, IList<IDrawableObject> drawables)
        {
            try
            {
                if (basicMaterial == null)
                    basicMaterial = new BasicMaterial(context.GraphicsDevice);
                Material = basicMaterial;
                base.Draw(context, drawables);
            }
            finally
            {
                Material = adoptionMaterial;
            }
        }
    }
#endif
}

