#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Linq;
using System.ComponentModel;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.Primitives;
using Nine.Animations;
using Nine.Graphics.Effects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    public class SkyBox : Drawable
    {
        /// <summary>
        /// Gets or sets the skybox texture.
        /// </summary>
        [ContentSerializer]
        public TextureCube Texture { get; internal set; }

        private Effect effect;

        public override void Draw(GraphicsContext context)
        {
#if !WINDOWS_PHONE
            // Keep track of the effect to avoid the effect been constantly garbage collected
            // since graphics resources uses weak references now.
            effect = effect ?? GraphicsResources<SkyBoxEffect>.GetInstance(context.GraphicsDevice);

            context.ModelBatch.DrawSkyBox(Texture);
#endif
        }
    }
}