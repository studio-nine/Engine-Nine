#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ScreenEffects
{
    /// <summary>
    /// Represents post processing effects.
    /// </summary>
    public class ScreenEffect : ScreenEffectPass
    {
        int renderTarget = 0;

        internal ScreenEffect() { }

        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        /// <param name="graphics">A GraphicsDevice instance.</param>
        public ScreenEffect(GraphicsDevice graphics) : base(graphics)
        {
            if (graphics.PresentationParameters.RenderTargetUsage != RenderTargetUsage.PreserveContents)
                throw new NotSupportedException("ScreenEffect requires RenderTargetUsage to be RenderTargetUsage.PreserveContents.");

            BlendState = BlendState.Opaque;
        }

        /// <summary>
        /// Begins the rendering of the scene to be post processed.
        /// </summary>
        public void Begin()
        {
            if (Effects.Count <= 0 && Passes.Count <= 0)
                return;

            renderTarget = Pool.MoveNext();
            Pool.Begin(renderTarget);
        }

        /// <summary>
        /// Ends the rendering of the scene, applying all the post processing effects.
        /// </summary>
        public void End()
        {
            if (Effects.Count <= 0 && Passes.Count <= 0)
                return;

            Texture2D backbuffer = Pool.End(renderTarget);

            Pool.Lock(renderTarget);
            Draw(backbuffer);
            Pool.Unlock(renderTarget);
        }

        /// <summary>
        /// Creates a bloom post processing effect.
        /// </summary>
        public static ScreenEffect CreateBloom(GraphicsDevice graphics, float threshold, float saturation, float blurAmount)
        {
            ScreenEffect screenEffect = new ScreenEffect(graphics);

            ScreenEffectPass basePass = new ScreenEffectPass(graphics);
            basePass.BlendState = BlendState.Opaque;

            ScreenEffectPass brightPass = new ScreenEffectPass(graphics);
            brightPass.Effects.Add(new ThresholdEffect(graphics));
            brightPass.Effects.Add(new ColorMatrixEffect(graphics) { Transform = ColorMatrix.CreateSaturation(saturation) });
            brightPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction =  MathHelper.PiOver4 });
            brightPass.Effects.Add(new BlurEffect(graphics) { BlurAmount = blurAmount, Direction = -MathHelper.PiOver4 });
            brightPass.BlendState = BlendState.Additive;
            brightPass.DownScaleEnabled = true;
            brightPass.FilteringEnabled = false;

            screenEffect.Passes.Add(basePass);
            screenEffect.Passes.Add(brightPass);

            return screenEffect;
        }
    }

    class ScreenEffectReader : ContentTypeReader<ScreenEffect>
    {
        protected override ScreenEffect Read(ContentReader input, ScreenEffect existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new ScreenEffect(input.ContentManager.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice);
            ScreenEffectPassReader.InternalRead(input, existingInstance);
            return existingInstance;
        }
    }
}

