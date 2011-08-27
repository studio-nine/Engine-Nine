#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
    /// Defines an basic screen effect that uses an effect chain.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ChainedScreenEffect : IScreenEffect, IUpdateable, IEffectTexture
    {
        /// <summary>
        /// Gets all the effects used by this pass.
        /// </summary>
        public ScreenEffectCollection Effects { get; private set; }

        /// <summary>
        /// Gets or sets whether this <c>ChainedScreenEffect</c> is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Creates a new instance of <c>ChainedScreenEffect</c>.
        /// </summary>
        public ChainedScreenEffect()
        {
            Enabled = true;
            Effects = new ScreenEffectCollection();
        }

        public virtual void Update(TimeSpan elapsedTime)
        {
            if (Enabled)
            {
                for (int i = 0; i < Effects.Count; i++)
                {
                    IUpdateable update = Effects[i] as IUpdateable;
                    if (update != null)
                        update.Update(elapsedTime);
                }
            }
        }

        public void SetTexture(TextureUsage usage, Texture texture)
        {
            for (int i = 0; i < Effects.Count; i++)
            {
                IEffectTexture update = Effects[i] as IEffectTexture;
                if (update != null)
                    update.SetTexture(usage, texture);
            }
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Texture2D Process(Texture2D input)
        {
            Texture2D intermediate = input;
            if (Enabled && Effects.Count > 0)
            {
                RenderTargetPool.AddRef(input as RenderTarget2D);

                for (int i = 0; i < Effects.Count; i++)
                {
                    intermediate = Effects[i].Process(intermediate);

                    if (i == 0)
                        RenderTargetPool.Release(input as RenderTarget2D);
                }
            }
            return intermediate;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ProcessAndDraw(Texture2D input)
        {
            if (Enabled)
            {
                if (Effects.Count <= 0)
                {
                    input.GraphicsDevice.DrawFullscreenQuad(input, SamplerState.PointClamp, BlendState.Opaque, Color.White, null);
                    return;
                }

                if (Effects.Count > 1)
                    RenderTargetPool.AddRef(input as RenderTarget2D);

                Texture2D intermediate = input;
                for (int i = 0; i < Effects.Count - 1; i++)
                {
                    intermediate = Effects[i].Process(intermediate);

                    if (i == 0)
                        RenderTargetPool.Release(input as RenderTarget2D);
                }

                if (Effects.Count > 0)
                    Effects[Effects.Count - 1].ProcessAndDraw(intermediate);
            }
        }
    }
}

