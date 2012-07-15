#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
using Nine.Graphics.Materials;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Graphics.PostEffects
{
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
    }
}