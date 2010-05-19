#region Copyright 2010 (c) Nightin Games
//=============================================================================
//
//  Copyright 2010 (c) Nightin Games. All Rights Reserved.
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

namespace Nine.Graphics.ParticleEffects
{
    internal sealed class ParticleEffectBatch
    {
        #region Key
        internal struct Key
        {
            public Texture2D Texture;
            public EffectTechnique Technique;
        }
        #endregion


        private Matrix view;
        private Matrix projection;
        private bool hasBegin = false;
        private Batch<Key, ParticleEffect> batch;
        private ParticleSystemEffect effect;
        private GameTime time;


        public GraphicsDevice GraphicsDevice { get; private set; }
        public bool IsDisposed { get; private set; }


        public ParticleEffectBatch(GraphicsDevice graphics, int capacity)
        {
            if (capacity < 4)
                throw new ArgumentException("Capacity should be at least 4");

            GraphicsDevice = graphics;

            effect = new ParticleSystemEffect(graphics);

            batch = new Batch<Key, ParticleEffect>(capacity);
        }


        public void Begin(Matrix view, Matrix projection)
        {
            if (IsDisposed)
                throw new ObjectDisposedException("ParticleEffectBatch");

            hasBegin = true;

            this.view = view;
            this.projection = projection;

            batch.Clear();
        }


        public void Draw(ParticleEffect particleEffect, GameTime time)
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (IsDisposed)
                throw new ObjectDisposedException("ParticleEffectBatch");

            if (particleEffect == null)
                throw new ArgumentNullException();

            if (particleEffect.Texture == null)
                throw new ArgumentException("Particle effect do not have a valid texture");

            
            this.time = time;


            Key key;

            key.Texture = particleEffect.Texture;

            // Choose the appropriate effect technique. If these particles will never
            // rotate, we can use a simpler pixel shader that requires less GPU power.
            if ((particleEffect.RotateSpeed.Min == 0) && (particleEffect.RotateSpeed.Max == 0))
                key.Technique = effect.Techniques["NonRotatingParticles"];
            else
                key.Technique = effect.Techniques["RotatingParticles"];

            batch.Add(key, particleEffect);
        }


        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException("Begin must be called before end and draw calls");

            if (IsDisposed)
                throw new ObjectDisposedException("ParticleEffectBatch");


            hasBegin = false;

            if (batch.Count <= 0)
                return;


            // Look up shortcuts for parameters that change every frame.
            effect.View = view;
            effect.Projection = projection;
            effect.CurrentTime = (float)time.TotalGameTime.TotalSeconds;

            foreach (BatchItem<Key, ParticleEffect> batchItem in batch.Batches)
            {
                effect.Texture = batchItem.Key.Texture;

                for (int i = 0; i < batchItem.Count; i++)
                {
                    ParticleEffect particleEffect = batchItem.Values[i];

                    effect.Duration = new Vector2((float)particleEffect.Duration.Max.TotalSeconds,
                                                  (float)particleEffect.Duration.Min.TotalSeconds);
                    effect.Gravity = particleEffect.Gravity;
                    effect.EndVelocity = particleEffect.EndVelocity;
                    effect.MinStartColor = particleEffect.StartColor.Min.ToVector4();
                    effect.MaxStartColor = particleEffect.StartColor.Max.ToVector4();
                    effect.MinEndColor = particleEffect.EndColor.Min.ToVector4();
                    effect.MaxEndColor = particleEffect.EndColor.Max.ToVector4();
                    effect.RotateSpeed = new Vector2(particleEffect.RotateSpeed.Min,
                                                     particleEffect.RotateSpeed.Max);
                    effect.StartSize = new Vector2(particleEffect.StartSize.Min,
                                                   particleEffect.StartSize.Max);
                    effect.EndSize = new Vector2(particleEffect.EndSize.Min,
                                                 particleEffect.EndSize.Max);

                    effect.CurrentTechnique = batchItem.Key.Technique;
                    batchItem.Key.Technique.Passes[0].Apply();

                    particleEffect.Draw(GraphicsDevice, time);
                }
            }
        }
    }
}
