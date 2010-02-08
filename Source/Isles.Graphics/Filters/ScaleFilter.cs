#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//  Written By  : Mahdi Khodadadi Fard.
//  Date        : 2010-Feb-08
//  Edit        : -
//=============================================================================
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
    /// <summary>
    /// Scales the input texture 1/4 size of the original input texture. This class is used by HDR Pipeline.
    /// </summary>
    public sealed class ScaleFilter : Filter
    {
        private bool supportsFilterFP16 = false;
        private Effect effect;

        /// <summary>
        /// If set to true and hardware supports shader scaling, it will use shader to scale the input texture. Otherwise it will use hardware scaling.
        /// </summary>
        public bool PreferShaderScaling
        {
            get;
            set;
        }

        /// <summary>
        /// Indicates whether is class is being used for HDR rendering. This property should be internal. No exteral access is needed.
        /// </summary>
        internal bool ScaleForLuminanceMeasurement
        {
            get;
            set;
        }


        public ScaleFilter()
        {
            ScaleForLuminanceMeasurement = false;
            PreferShaderScaling = false;
        }

        protected override void LoadContent()
        {
            effect = ScaleFilter_Code.CreateEffect(GraphicsDevice);

            // Check to see if supports filter fp16
            supportsFilterFP16 = GraphicsAdapter.DefaultAdapter.CheckDeviceFormat(DeviceType.Hardware,
                                                                                SurfaceFormat.Color,
                                                                                TextureUsage.None,
                                                                                QueryUsages.Filter,
                                                                                ResourceType.Texture2D,
                                                                                SurfaceFormat.HalfVector4); 
        }
        protected override void Begin(Texture2D input)
        {
            effect.Parameters["SourceTexture0"].SetValue(input);
            effect.Parameters["g_vSourceDimensions"].SetValue(new Vector2(input.Width, input.Height));

            if (ScaleForLuminanceMeasurement)
            {
                effect.CurrentTechnique = effect.Techniques["ScaleShaderLuminance"];
            }
            else
            {
                if (supportsFilterFP16 && PreferShaderScaling)
                {
                    effect.CurrentTechnique = effect.Techniques["ScaleShader"];
                }
                else
                {
                    effect.CurrentTechnique = effect.Techniques["ScaleHardware"];
                }
            }

            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
        }

        /// <summary>
        /// For a correct result, 'renderTarget' parameter should be 1/4 size of the input texture.
        /// </summary>
        /// <param name="graphics"></param>
        /// <param name="input"></param>
        /// <param name="destination"></param>
        /// <param name="renderTarget"></param>
        public override void Draw(GraphicsDevice graphics, Texture2D input, Rectangle destination, RenderTarget2D renderTarget)
        {
            Vector2 destDimensions = new Vector2();

            if (renderTarget == null)
            {
                destDimensions.X = graphics.PresentationParameters.BackBufferWidth;
                destDimensions.Y = graphics.PresentationParameters.BackBufferHeight;
            }
            else
            {
                destDimensions.X = renderTarget.Width;
                destDimensions.Y = renderTarget.Height;
            }

            effect.Parameters["g_vDestinationDimensions"].SetValue(destDimensions);

            base.Draw(graphics, input, destination, renderTarget);
        }

        protected override void End()
        {
            effect.CurrentTechnique.Passes[0].End();
            effect.End();
        }
    }
}

