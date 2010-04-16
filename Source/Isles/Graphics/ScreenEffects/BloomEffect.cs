﻿#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Statements
using System;
using System.IO;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion


namespace Isles.Graphics.ScreenEffects
{
    /// <summary>
    /// A post processing screen effect that blooms the whole screen.
    /// </summary>
    public sealed class BloomEffect : IScreenEffect
    {
        private Threshold extract;
        private BlurEffect blur;
        private BloomCombine combine;
        private RenderToTextureEffect renderToTexture;

        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }
        
        /// <summary>
        /// Gets or sets the brightness threshold. 
        /// Pixels brighter then this value will be blured and bloomed.
        /// The specfied value should be ranged from 0.0f to 1.0f;
        /// </summary>
        public float Threshold
        {
            get { return extract.BrightnessThreshold; }
            set { extract.BrightnessThreshold = value; }
        }

        /// <summary>
        /// Gets or sets the saturation amount. 
        /// Pixels brighter then threshold will be saturation based on this value.
        /// The specfied value should be ranged from 0.0f to 1.0f;
        /// </summary>
        public float Saturation
        {
            get { return combine.BloomSaturation; }
            set { combine.BloomSaturation = value; }
        }

        /// <summary>
        /// Gets or sets the bloom intensity.
        /// </summary>
        public float Intensity
        {
            get { return combine.BloomIntensity; }
            set { combine.BloomIntensity = value; }
        }

        /// <summary>
        /// Gets or sets the blur amount. 
        /// Pixels brighter then threshold will be blured based on this value.
        /// </summary>
        public float Blur
        {
            get { return blur.BlurAmount; }
            set { blur.BlurAmount = value; }
        }


        /// <summary>
        /// Creates a new instance of bloom post processing.
        /// </summary>
        /// <param name="graphics">A GraphicsDevice instance.</param>
        public BloomEffect(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;

            extract = new Threshold(graphics);
            blur = new BlurEffect(graphics);
            combine = new BloomCombine(graphics);

            blur.BlurAmount = 2.0f;

            // Scale down render target
            renderToTexture = new RenderToTextureEffect(
                                    GraphicsDevice,
                                    GraphicsDevice.Viewport.Width / 2,
                                    GraphicsDevice.Viewport.Height / 2,
                                    GraphicsDevice.PresentationParameters.BackBufferFormat);
        }

        /// <summary>
        /// Draw the input texture onto the screen with a custom effect.
        /// </summary>
        /// <param name="texture">Input texture to be processed.</param>        
        public void Draw(Texture2D texture)
        {
            Texture2D intermediate;

            
            // Extract
            renderToTexture.Begin();

            GraphicsDevice.DrawSprite(texture, null, null, Color.White, extract);

            intermediate = renderToTexture.End();

            
            // Blur U
            blur.Direction = MathHelper.ToRadians(45);

            renderToTexture.Begin();

            GraphicsDevice.DrawSprite(intermediate, null, null, Color.White, blur);

            intermediate = renderToTexture.End();


            // Blur V
            blur.Direction = MathHelper.ToRadians(-45);

            renderToTexture.Begin();

            GraphicsDevice.DrawSprite(intermediate, null, null, Color.White, blur);

            intermediate = renderToTexture.End();
            

            // Combine
            GraphicsDevice.Textures[1] = intermediate;
            GraphicsDevice.DrawSprite(texture, null, null, Color.White, combine);
            GraphicsDevice.Textures[1] = null;
        }
    }
}
