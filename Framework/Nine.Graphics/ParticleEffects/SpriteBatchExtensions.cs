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
using System.Text;
using System.IO;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Contains extension methods for drawing particle effect using SpriteBatch.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static class SpriteBatchExtensions
    {
        public static void DrawParticleEffect(this SpriteBatch spriteBatch, ParticleEffect particleEffect)
        {
            // TODO: Use Vector2 for size (both width and height)?
            if (particleEffect.Texture != null)
            {
                Vector2 scaleFactor = new Vector2(1.0f / particleEffect.Texture.Width, 1.0f / particleEffect.Texture.Height);
                Vector2 origin = new Vector2(particleEffect.Texture.Width / 2, particleEffect.Texture.Height / 2);

                if (particleEffect.ParticleType == ParticleType.Billboard)
                {
                    particleEffect.ForEach((ref Particle particle) =>
                    {
                        spriteBatch.Draw(particleEffect.Texture, 
                                         new Vector2(particle.Position.X, particle.Position.Y), 
                                         particleEffect.SourceRectangle,
                                         particle.Color * particle.Alpha, particle.Rotation, origin,
                                         particle.Size * scaleFactor, SpriteEffects.None, 0);
                    });
                }
                else if (particleEffect.ParticleType == ParticleType.ConstrainedBillboard)
                {
                    scaleFactor.X *= particleEffect.Stretch;

                    particleEffect.ForEach((ref Particle particle) =>
                    {
                        float rotation = (float)Math.Atan2(particle.Velocity.Y, particle.Velocity.X);

                        spriteBatch.Draw(particleEffect.Texture,
                                         new Vector2(particle.Position.X, particle.Position.Y),
                                         particleEffect.SourceRectangle,
                                         particle.Color * particle.Alpha, rotation, origin,
                                         particle.Size * scaleFactor, SpriteEffects.None, 0);
                    });
                }
                else if (particleEffect.ParticleType == ParticleType.ConstrainedBillboardUp)
                {
                    scaleFactor.X *= particleEffect.Stretch;
                    float rotation = (float)Math.Atan2(particleEffect.Up.Y, particleEffect.Up.X);

                    particleEffect.ForEach((ref Particle particle) =>
                    {

                        spriteBatch.Draw(particleEffect.Texture,
                                         new Vector2(particle.Position.X, particle.Position.Y),
                                         particleEffect.SourceRectangle,
                                         particle.Color * particle.Alpha, rotation, origin,
                                         particle.Size * scaleFactor, SpriteEffects.None, 0);
                    });
                }
                else if (particleEffect.ParticleType == ParticleType.RibbonTrail)
                {
                    throw new NotImplementedException();
                }
            }

            foreach (var childEffect in particleEffect.ChildEffects)
                DrawParticleEffect(spriteBatch, childEffect);

            foreach (var endingEffect in particleEffect.EndingEffects)
                DrawParticleEffect(spriteBatch, endingEffect);
        }
    }
}
