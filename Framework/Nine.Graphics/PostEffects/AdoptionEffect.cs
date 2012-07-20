#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
using System;
#endregion

namespace Nine.Graphics.PostEffects
{
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

        public Vector3 InitialColor { get; set; }

        /// <summary>
        /// Creates a new instance of <c>AdoptationEffect</c>.
        /// </summary>
        public AdoptionEffect()
        {
            Speed = 10;
        }

        public override void GetActivePasses(IList<Pass> result)
        {
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
            SamplerState previousSamplerState = context.GraphicsDevice.SamplerStates[1];

            try
            {
                if (needLocalTexture = (currentFrame == null))
                {
                    PrepareRenderTarget(context, InputTexture);
                    currentFrame.Begin();
                }

                context.GraphicsDevice.Textures[1] = lastFrame;
                context.GraphicsDevice.SamplerStates[1] = SamplerState.LinearClamp;

                adoptionMaterial.effect.delta.SetValue(context.ElapsedSeconds * Speed);
                base.Draw(context, drawables);
            }
            finally
            {
                if (needLocalTexture)
                {
                    currentFrame.End();

                    InputTexture = currentFrame;                    
                    if (basicMaterial == null)
                        basicMaterial = new BasicMaterial(context.GraphicsDevice);
                    Material = basicMaterial;
                    base.Draw(context, drawables);
                    Material = adoptionMaterial;
                }

                RenderTargetPool.Unlock(lastFrame);
                lastFrame = currentFrame;
                currentFrame = null;

                context.GraphicsDevice.SamplerStates[1] = previousSamplerState;
            }            
        }
    }
#endif
}

