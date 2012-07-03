#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
    /// Represents a bloom post processing effect.
    /// </summary>
    [ContentSerializable]
    public class BloomEffect : ISceneObject
    {
        public float Threshold
        {
            get { return threshold.Threshold; }
            set { threshold.Threshold = value; }
        }

        public float BlurAmount
        {
            get { return blurH.BlurAmount; }
            set { blurH.BlurAmount = blurV.BlurAmount; }
        }

        PostEffectGroup postEffect;
        BlurMaterial blurH;
        BlurMaterial blurV;
        ThresholdMaterial threshold;

        /// <summary>
        /// Initializes a new instance of the <see cref="BloomEffect"/> class.
        /// </summary>
        public BloomEffect(GraphicsDevice graphics)
        {
            postEffect = new PostEffectGroup();
            postEffect.Passes.Add(new PostEffectChain());
            postEffect.Passes.Add(new PostEffectChain(BlendState.Additive,
                new PostEffect(graphics) { Material = new ScaleMaterial(graphics), RenderTargetScale = 0.5f },
                new PostEffect(graphics) { Material = threshold = new ThresholdMaterial(graphics) },
                new PostEffect(graphics) { Material = blurH = new BlurMaterial(graphics) },
                new PostEffect(graphics) { Material = blurV = new BlurMaterial(graphics) { Direction = MathHelper.PiOver2 } },
                new PostEffect(graphics) { Material = new ScaleMaterial(graphics), RenderTargetScale = 2.0f }
            ));
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