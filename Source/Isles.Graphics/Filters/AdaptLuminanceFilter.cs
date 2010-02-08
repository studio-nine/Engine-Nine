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
    /// Used for transition from the previous frame luminance texture to current frame luminance texture, based on the elapsed time.
    /// Yes, this class is internal. No external access is needed.
    /// </summary>
    internal sealed class AdaptLuminanceFilter : Filter
    {
        private Effect effect;

        public Texture2D PreviousFrameLuminanceTexture
        {
            get;
            set;
        }

        public AdaptLuminanceFilter()
        {
        }

        protected override void LoadContent()
        {
            effect = AdaptLuminanceFilter_Code.CreateEffect(GraphicsDevice);
        }
        protected override void Begin(Texture2D input)
        {
            effect.Begin();
            effect.CurrentTechnique.Passes[0].Begin();
        }

        public override void Draw(GraphicsDevice graphics, Texture2D input, Rectangle destination, RenderTarget2D renderTarget)
        {
            effect.Parameters["SourceTexture0"].SetValue(input);
            effect.Parameters["SourceTexture1"].SetValue(PreviousFrameLuminanceTexture);
            effect.Parameters["g_vSourceDimensions"].SetValue(new Vector2(input.Width, input.Height));

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

