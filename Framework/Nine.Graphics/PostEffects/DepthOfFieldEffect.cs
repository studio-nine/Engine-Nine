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
    /// Represents a depth of field post processing effect.
    /// </summary>
    public class DepthOfFieldEffect : PostEffectGroup
    {        
        public float FocalLength
        {
            get { return material.FocalLength; }
            set { material.FocalLength = value; }
        }

        public float FocalPlane
        {
            get { return material.FocalPlane; }
            set { material.FocalPlane = value; }
        }

        public float FocalDistance
        {
            get { return material.FocalDistance; }
            set { material.FocalDistance = value; }
        }

        public float BlurAmount
        {
            get { return blurH.BlurAmount; }
            set { blurH.BlurAmount = blurV.BlurAmount = value; }
        }

        DepthOfFieldMaterial material;
        BlurMaterial blurH;
        BlurMaterial blurV;

        /// <summary>
        /// Initializes a new instance of the <see cref="DepthOfFieldEffect"/> class.
        /// </summary>
        public DepthOfFieldEffect(GraphicsDevice graphics)
        {
            Material = material = new DepthOfFieldMaterial(graphics);
            Passes.Add(new PostEffectChain());
            Passes.Add(new PostEffectChain(TextureUsage.Blur,
                new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 0.5f },
                new PostEffect() { Material = blurH = new BlurMaterial(graphics) },
                new PostEffect() { Material = blurV = new BlurMaterial(graphics) { Direction = MathHelper.PiOver2 } },
                new PostEffect() { Material = new ScaleMaterial(graphics), RenderTargetScale = 2.0f }
            ));
        }
    }
}