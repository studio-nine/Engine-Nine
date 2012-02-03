#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Contains extension methods for drawing particle effect using SpriteBatch.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SpriteBatchExtensions
    {
        static SpriteBatch CurrentSpriteBatch;
        static ParticleEffect CurrentParticleEffect;

        public static void DrawParticleEffect(this SpriteBatch spriteBatch, ParticleEffect particleEffect)
        {
            if (particleEffect == null)
                throw new ArgumentNullException("particleEffect");

            CurrentParticleEffect = particleEffect;
            CurrentSpriteBatch = spriteBatch;

            // TODO: Use Vector2 for size (both width and height)?
            if (CurrentParticleEffect.Texture != null)
            {
                Vector2 scaleFactor = new Vector2(1.0f / CurrentParticleEffect.Texture.Width, 1.0f / CurrentParticleEffect.Texture.Height);
                Vector2 origin = new Vector2(CurrentParticleEffect.Texture.Width / 2, CurrentParticleEffect.Texture.Height / 2);

                if (CurrentParticleEffect.ParticleType == ParticleType.Billboard)
                {
                    CurrentParticleEffect.ForEach((ref Particle particle) =>
                    {
                        CurrentSpriteBatch.Draw(CurrentParticleEffect.Texture, 
                                         new Vector2(particle.Position.X, particle.Position.Y), 
                                         CurrentParticleEffect.SourceRectangle,
                                         particle.Color * particle.Alpha, particle.Rotation, origin,
                                         particle.Size * scaleFactor, SpriteEffects.None, 0);
                    });
                }
                else if (CurrentParticleEffect.ParticleType == ParticleType.ConstrainedBillboard)
                {
                    scaleFactor.X *= CurrentParticleEffect.Stretch;

                    CurrentParticleEffect.ForEach((ref Particle particle) =>
                    {
                        float rotation = (float)Math.Atan2(particle.Velocity.Y, particle.Velocity.X);

                        CurrentSpriteBatch.Draw(CurrentParticleEffect.Texture,
                                         new Vector2(particle.Position.X, particle.Position.Y),
                                         CurrentParticleEffect.SourceRectangle,
                                         particle.Color * particle.Alpha, rotation, origin,
                                         particle.Size * scaleFactor, SpriteEffects.None, 0);
                    });
                }
                else if (CurrentParticleEffect.ParticleType == ParticleType.ConstrainedBillboardUp)
                {
                    scaleFactor.X *= CurrentParticleEffect.Stretch;
                    float rotation = (float)Math.Atan2(CurrentParticleEffect.Up.Y, CurrentParticleEffect.Up.X);

                    CurrentParticleEffect.ForEach((ref Particle particle) =>
                    {

                        CurrentSpriteBatch.Draw(CurrentParticleEffect.Texture,
                                         new Vector2(particle.Position.X, particle.Position.Y),
                                         CurrentParticleEffect.SourceRectangle,
                                         particle.Color * particle.Alpha, rotation, origin,
                                         particle.Size * scaleFactor, SpriteEffects.None, 0);
                    });
                }
                else if (CurrentParticleEffect.ParticleType == ParticleType.RibbonTrail)
                {
                    throw new NotImplementedException();
                }
            }

            CurrentSpriteBatch = null;
            CurrentParticleEffect = null;

            for (int i = 0; i < particleEffect.ChildEffects.Count; i++)
                DrawParticleEffect(spriteBatch, particleEffect.ChildEffects[i]);

            for (int i = 0; i < particleEffect.EndingEffects.Count; i++)
                DrawParticleEffect(spriteBatch, particleEffect.EndingEffects[i]);
        }
    }
}
