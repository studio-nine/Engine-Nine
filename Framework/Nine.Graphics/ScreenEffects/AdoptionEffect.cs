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
#if !WINDOWS_PHONE
    /// <summary>
    /// Defines a post processing effect that adopt scene changes.
    /// </summary>
    public class AdoptionEffect : BasicScreenEffect
    {
        private bool processing;
        private Texture2D lastFrameTexture;

        /// <summary>
        /// Get or sets the speed of the adoptation.
        /// </summary>
        public float Speed
        {
            get { return ((Adoption)Effect).Speed; }
            set { ((Adoption)Effect).Speed = value; }
        }

        /// <summary>
        /// Creates a new instance of <c>AdoptationEffect</c>.
        /// </summary>
        public AdoptionEffect(GraphicsDevice graphics)
        {   
            Effect = new Adoption(graphics);
            Speed = 10;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override Texture2D Process(Texture2D input)
        {
            if (!Enabled)
                return input;

            processing = true;            
            ((Adoption)Effect).LastFrameTexture = lastFrameTexture;
            Texture2D adoptedTexture = base.Process(input);
            processing = false;

            RenderTargetPool.Release(lastFrameTexture as RenderTarget2D);
            RenderTargetPool.AddRef(adoptedTexture as RenderTarget2D);

            return lastFrameTexture = adoptedTexture;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override void ProcessAndDraw(Texture2D input)
        {
            if (processing)
            {
                base.ProcessAndDraw(input);
                return;
            }

            Texture2D adoptedTexture = Process(input);
            RenderTargetPool.Release(lastFrameTexture as RenderTarget2D);
            RenderTargetPool.AddRef(adoptedTexture as RenderTarget2D);

            if (Enabled)
            {
                GraphicsDevice.DrawFullscreenQuad(lastFrameTexture = adoptedTexture, SamplerState.PointClamp, BlendState.Opaque, Color.White, null);
            }
        }
    }
#endif
}

