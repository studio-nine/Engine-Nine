#region File Description
//-----------------------------------------------------------------------------
// BloomComponent.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Xml.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Isles.Graphics.Filters
{
    #region BloomExtract
    internal sealed class BloomExtractFilter : Filter
    {
        private Effect effect;

        public float BloomThreshold { get; set; }


        protected override void LoadContent()
        {
            effect = InternalContents.BloomExtractEffect(GraphicsDevice);
        }

        protected override void Begin(Texture2D input)
        {
            effect.Parameters["BloomThreshold"].SetValue(BloomThreshold);

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
        }

        protected override void End()
        {
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
    #endregion


    #region BloomCombine
    internal sealed class BloomCombineFilter : Filter
    {
        private Effect effect;

        public float BloomIntensity { get; set; }
        public float BaseIntensity { get; set; }
        public float BloomSaturation { get; set; }
        public float BaseSaturation { get; set; }

        public Texture2D BaseTexture { get; set; }

                
        protected override void LoadContent()
        {
            effect = InternalContents.BloomCombineEffect(GraphicsDevice);
        }

        protected override void Begin(Texture2D input)
        {
            GraphicsDevice.Textures[1] = BaseTexture;

            effect.Parameters["BloomIntensity"].SetValue(BloomIntensity);
            effect.Parameters["BaseIntensity"].SetValue(BaseIntensity);
            effect.Parameters["BloomSaturation"].SetValue(BloomSaturation);
            effect.Parameters["BaseSaturation"].SetValue(BaseSaturation);

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
        }

        protected override void End()
        {
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
    #endregion


    #region BloomFilter
    public sealed class BloomFilter : Filter
    {
        // Controls how bright a pixel needs to be before it will bloom.
        // Zero makes everything bloom equally, while higher values select
        // only brighter colors. Somewhere between 0.25 and 0.5 is good.
        public float BloomThreshold { get; set; }


        // Controls how much blurring is applied to the bloom image.
        // The typical range is from 1 up to 10 or so.
        public float BlurAmount { get; set; }


        // Controls the amount of the bloom and base images that
        // will be mixed into the final scene. Range 0 to 1.
        public float BloomIntensity { get; set; }
        public float BaseIntensity { get; set; }


        // Independently control the color saturation of the bloom and
        // base images. Zero is totally desaturated, 1.0 leaves saturation
        // unchanged, while higher values increase the saturation level.
        public float BloomSaturation { get; set; }
        public float BaseSaturation { get; set; }


        private BloomExtractFilter extract = new BloomExtractFilter();       
        private BloomCombineFilter combine = new BloomCombineFilter();
        private BlurFilter blurX = new BlurFilter();
        private BlurFilter blurY = new BlurFilter();

        
        public BloomFilter()
        {
            BloomThreshold = 0.25f;
            BlurAmount = 4;
            BloomIntensity = 2;
            BaseIntensity = 1;
            BloomSaturation = 2;
            BaseSaturation = 0;


            blurX.RenderTargetScale = 0.5f;
            blurY.RenderTargetScale = 0.5f;
            
            blurX.DerivationX = 1.0f;
            blurX.DerivationY = 0;

            blurY.DerivationX = 0;
            blurY.DerivationY = 1.0f;
        }
        

        public override void Draw(GraphicsDevice graphics, Texture2D input, Rectangle destination, RenderTarget2D renderTarget)
        {
            blurX.BlurAmount = BlurAmount;
            blurY.BlurAmount = BlurAmount;
            
            extract.BloomThreshold = BloomThreshold;

            combine.BaseIntensity = BaseIntensity;
            combine.BaseSaturation = BaseSaturation;
            combine.BloomIntensity = BloomIntensity;
            combine.BloomSaturation = BloomSaturation;

            combine.BaseTexture = input;
            
            input = extract.Process(graphics, input);
            input = blurX.Process(graphics, input);
            input = blurY.Process(graphics, input);

            combine.Draw(graphics, input, destination, renderTarget);
        }


        protected override void LoadContent() { }
        protected override void Begin(Texture2D input) { }
        protected override void End() { }
    }
    #endregion
}

