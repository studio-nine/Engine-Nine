#region Copyright 2009 - 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Nightin Games. All Rights Reserved.
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
#endregion


namespace Isles.Graphics.ParticleEffects
{
    public sealed class ParticleBatch : IDisposable
    {
        PointSpriteBatch pointSprite;
        LineSpriteBatch lineSprite;
        ParticleEffectBatch particleEffect;


        public GraphicsDevice GraphicsDevice { get; private set; }
        public bool IsDisposed { get; private set; }


        public ParticleBatch(GraphicsDevice graphics, int capacity)
        {
            GraphicsDevice = graphics;

            pointSprite = new PointSpriteBatch(graphics, capacity);
            lineSprite = new LineSpriteBatch(graphics, capacity);
            particleEffect = new ParticleEffectBatch(graphics, capacity);
        }

        public void Begin(Matrix view, Matrix projection)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("Particle Batch");

            pointSprite.Begin(view, projection);
            lineSprite.Begin(view, projection);
            particleEffect.Begin(view, projection);
        }

        public void Draw(Texture2D texture, Vector3 position, float size, float rotation, Color color)
        {
            pointSprite.Draw(texture, position, size, rotation, color);
        }

        public void DrawLine(Texture2D texture, Vector3 start, Vector3 end, float width, Color color)
        {
            DrawLine(texture, start, end, width, Vector2.One, Vector2.Zero, color);
        }

        public void DrawLine(Texture2D texture, Vector3 start, Vector3 end, float width, Vector2 textureScale, Vector2 textureOffset, Color color)
        {
            lineSprite.Draw(texture, start, end, width, textureScale, textureOffset, color);
        }

        public void DrawLine(Texture2D texture, Vector3[] lineStrip, float width, Color color)
        {
            DrawLine(texture, lineStrip, width, Vector2.One, Vector2.Zero, color);
        }

        public void DrawLine(Texture2D texture, Vector3[] lineStrip, float width, Vector2 textureScale, Vector2 textureOffset, Color color)
        {
            lineSprite.Draw(texture, lineStrip, width, textureScale, textureOffset, color);
        }

        public void Draw(ParticleEffect effect, GameTime time)
        {
            particleEffect.Draw(effect, time);
        }

        public void End() 
        {
            pointSprite.End();
            lineSprite.End();
            particleEffect.End();
        }

        public void Dispose()
        {
            if (pointSprite != null)
                pointSprite.Dispose();
            if (lineSprite != null)
                lineSprite.Dispose();

            IsDisposed = true;
        }
    }
}
