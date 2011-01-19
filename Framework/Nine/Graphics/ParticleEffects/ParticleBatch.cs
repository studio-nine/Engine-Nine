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
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ParticleEffects
{
    /// <summary>
    /// Enables a group of particle effects to be drawn in batches.
    /// </summary>
    public class ParticleBatch : IDisposable
    {
        private PrimitiveBatch primitiveBatch;

        /// <summary>
        /// Gets the underlying graphics device used by this ModelBatch.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get { return primitiveBatch.GraphicsDevice; } }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }
        
#if WINDOWS_PHONE
        public ParticleBatch(GraphicsDevice graphics) : this(graphics, 2048)
#else
        public ParticleBatch(GraphicsDevice graphics) : this(graphics, 8192)
#endif
        {
        }

        public ParticleBatch(GraphicsDevice graphics, int capacity)
        {
            primitiveBatch = new PrimitiveBatch(graphics, capacity);
        }

        public void Begin()
        {
        }

        public void Begin(Matrix view, Matrix projection)
        {
            primitiveBatch.Begin(PrimitiveSortMode.Deferred, view, projection);
        }

        public void Draw(ParticleEffect particleEffect)
        {

        }

        public void DrawBillboard(ParticleEffect particleEffect)
        {
            for (int currentParticle = particleEffect.firstParticle; 
                     currentParticle != particleEffect.lastParticle;
                     currentParticle = (currentParticle + 1) % particleEffect.maxParticles)
            {
                if (particleEffect.particles[currentParticle].Age <= 1)
                {
                    Matrix textureTransform = TextureTransform.CreateSourceRectange(particleEffect.Texture, particleEffect.SourceRectangle);                         

                    primitiveBatch.DrawSprite(particleEffect.Texture,
                                              particleEffect.particles[currentParticle].Position,
                                              particleEffect.particles[currentParticle].Size, 
                                              particleEffect.particles[currentParticle].Size,
                                              particleEffect.particles[currentParticle].Rotation, Vector3.UnitZ, textureTransform, null,
                                              particleEffect.particles[currentParticle].Color);
                }
            }
        }

        public void DrawConstrainedBillboard(ParticleEffect particleEffect)
        {
            for (int currentParticle = particleEffect.firstParticle;
                     currentParticle != particleEffect.lastParticle;
                     currentParticle = (currentParticle + 1) % particleEffect.maxParticles)
            {
                if (particleEffect.particles[currentParticle].Age <= 1)
                {
                    Matrix textureTransform = TextureTransform.CreateSourceRectange(particleEffect.Texture, particleEffect.SourceRectangle);
                    Vector3 forward = Vector3.Normalize(particleEffect.particles[currentParticle].Velocity);
                    forward *= particleEffect.particles[currentParticle].Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

                    primitiveBatch.DrawLine(particleEffect.Texture,
                                            particleEffect.particles[currentParticle].Position,
                                            particleEffect.particles[currentParticle].Position + forward,
                                            particleEffect.particles[currentParticle].Size,
                                            textureTransform, null,
                                            particleEffect.particles[currentParticle].Color);
                }
            }
        }

        public void DrawConstrainedBillboard(ParticleEffect particleEffect, Vector3 axis)
        {
            for (int currentParticle = particleEffect.firstParticle;
                     currentParticle != particleEffect.lastParticle;
                     currentParticle = (currentParticle + 1) % particleEffect.maxParticles)
            {
                if (particleEffect.particles[currentParticle].Age <= 1)
                {
                    Matrix textureTransform = TextureTransform.CreateSourceRectange(particleEffect.Texture, particleEffect.SourceRectangle);
                    Vector3 forward = axis * particleEffect.particles[currentParticle].Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

                    primitiveBatch.DrawLine(particleEffect.Texture,
                                            particleEffect.particles[currentParticle].Position,
                                            particleEffect.particles[currentParticle].Position + forward,
                                            particleEffect.particles[currentParticle].Size,
                                            textureTransform, null,
                                            particleEffect.particles[currentParticle].Color);
                }
            }
        }

        public void End()
        {
            primitiveBatch.End();
        }

        public void Dispose()
        {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                primitiveBatch.Dispose();
            }
        }

        ~ParticleBatch()
        {
            Dispose(false);
        }
    }
}
