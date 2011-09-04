#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics.ObjectModel
{
    /// <summary>
    /// Defines commonly used statistics of the renderer.
    /// </summary>
    public class GraphicsStatistics
    {
        public int VisibleLightCount { get; internal set; }
        public int VisibleObjectCount { get; internal set; }
        public int VisibleDrawableCount { get; internal set; }

        public int RenderTargetCount { get { return RenderTargetPool.TotalRenderTargets; } }

        public int VertexCount { get; internal set; }
        public int PrimitiveCount { get; internal set; }

        internal GraphicsStatistics()
        {
            Reset();
        }

        internal void Reset()
        {
            VisibleLightCount = 0;
            VisibleDrawableCount = 0;
            VisibleObjectCount = 0;
            VertexCount = 0;
            PrimitiveCount = 0;
        }

        internal void Draw(SpriteBatch spriteBatch, SpriteFont font, Color color)
        {
            var height = font.MeasureString("X").Y;
            Vector2 position = new Vector2(50, 50);

            foreach (var property in GetType().GetProperties())
            {
                string text = string.Format("{0}: {1}", property.Name, property.GetValue(this, null).ToString());
                spriteBatch.DrawString(font, text, position + Vector2.One, Color.Black);
                spriteBatch.DrawString(font, text, position, color);
                position += new Vector2(0, height + 5);
            }
        }
    }
}