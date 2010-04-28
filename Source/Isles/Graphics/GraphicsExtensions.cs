#region Copyright 2009 (c) Nightin Games
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
        static SpriteBatch spriteBatch;

        public static void DrawSprite(this GraphicsDevice graphics, Texture2D texture, Rectangle? destination, Rectangle? source, Color color, Effect effect)
        {
            if (spriteBatch == null)
                spriteBatch = new SpriteBatch(graphics);

            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.Opaque);
                        
            if (effect != null)
            {
                effect.CurrentTechnique.Passes[0].Apply();     
            }


            if (destination == null)
            {
                destination = new Rectangle(graphics.Viewport.X,
                                            graphics.Viewport.Y,
                                            graphics.Viewport.Width,
                                            graphics.Viewport.Height);
            }


            spriteBatch.Draw(texture, destination.Value, source, color);

            spriteBatch.End();
        }
    }
    #endregion
}
