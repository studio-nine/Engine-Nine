﻿#region Copyright 2009 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 (c) Nightin Games. All Rights Reserved.
//
//=============================================================================
#endregion


#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion


namespace Isles.Graphics
{
    #region GraphicsExtensions
    public static class GraphicsExtensions
    {
        public static void SetSpriteBlendMode(this RenderState renderState, SpriteBlendMode blend)
        {
            if (blend == SpriteBlendMode.None)
            {
                renderState.AlphaBlendEnable = false;
            }
            else if (blend == SpriteBlendMode.AlphaBlend)
            {
                renderState.AlphaBlendEnable = true;
                renderState.SourceBlend = Blend.SourceAlpha;
                renderState.DestinationBlend = Blend.InverseSourceAlpha;
            }
            else if (blend == SpriteBlendMode.Additive)
            {
                renderState.AlphaBlendEnable = true;
                renderState.SourceBlend = Blend.SourceAlpha;
                renderState.DestinationBlend = Blend.One;
            }
        }

        static SpriteBatch spriteBatch;

        public static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, Rectangle? destination, Rectangle? source, Color color, Effect effect)
        {
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphics);
            
            spriteBatch.Begin(SpriteBlendMode.None, SpriteSortMode.Immediate, SaveStateMode.None);

            if (effect != null)
            {
                effect.Begin();
                effect.CurrentTechnique.Passes[0].Begin();
            }

            if (destination == null)
            {
                destination = new Rectangle(graphics.Viewport.X,
                                            graphics.Viewport.Y,
                                            graphics.Viewport.Width,
                                            graphics.Viewport.Height);
            }

            spriteBatch.Draw(texture, destination.Value, source, color);

            if (effect != null)
            {
                effect.End();
                effect.CurrentTechnique.Passes[0].End();
            }

            spriteBatch.End();
        }
    }
    #endregion
}
