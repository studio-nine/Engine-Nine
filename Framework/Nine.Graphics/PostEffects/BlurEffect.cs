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
    public class BlurEffect : ISceneObject
    {
        public float BlurAmount
        {
            get { return blurH.BlurAmount; }
            set { blurH.BlurAmount = blurV.BlurAmount = value; }
        }

        PostEffectChain postEffect;
        BlurMaterial blurH;
        BlurMaterial blurV;

        /// <summary>
        /// Initializes a new instance of the <see cref="BlurEffect"/> class.
        /// </summary>
        public BlurEffect(GraphicsDevice graphics)
        {
            postEffect = new PostEffectChain();
            postEffect.Effects.Add(new PostEffect(blurH = new BlurMaterial(graphics)));
            postEffect.Effects.Add(new PostEffect(blurV = new BlurMaterial(graphics) { Direction = MathHelper.PiOver2 }));
        }

        void ISceneObject.OnAdded(DrawingContext context)
        {
            ((ISceneObject)postEffect).OnAdded(context);
        }

        void ISceneObject.OnRemoved(DrawingContext context)
        {
            ((ISceneObject)postEffect).OnRemoved(context);
        }
    }
}