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
        /// <summary>
        /// Gets the underlying graphics device used by this ModelBatch.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get { return primitiveBatch.GraphicsDevice; } }

        /// <summary>
        /// Gets or sets user data.
        /// </summary>
        public object Tag { get; set; }

        private int batchCount;
        private bool hasBegin = false;
        private AlphaTestEffect alphaTestEffect;
        private PrimitiveBatch primitiveBatch;
        private List<ParticleBatchItem> batches;
        private ParticleBatchSortComparer comparer = new ParticleBatchSortComparer();

        private SamplerState samplerState;
        private RasterizerState rasterizerState;

        private Matrix view;
        private Matrix projection;

        private ParticleBatchItem item;
        private ParticleEffect particleEffect;
        private Matrix? textureTransform = null;

        private ParticleAction cachedDrawBillboard;
        private ParticleAction cachedDrawConstrainedBillboard;
        private ParticleAction cachedDrawConstrainedBillboardUp;
        private ParticleAction cachedDrawRibbonTrail;

        internal int VertexCount { get; private set; }
        internal int PrimitiveCount { get; private set; }
        
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
            batches = new List<ParticleBatchItem>();
            alphaTestEffect = new AlphaTestEffect(graphics);
            alphaTestEffect.VertexColorEnabled = true;

            cachedDrawBillboard = new ParticleAction(DrawParticleBillboard);
            cachedDrawConstrainedBillboard = new ParticleAction(DrawParticleConstrainedBillboard);
            cachedDrawConstrainedBillboardUp = new ParticleAction(DrawParticleConstrainedBillboardUp);
            cachedDrawRibbonTrail = new ParticleAction(DrawParticleRibbonTrail);
        }

        public void Begin()
        {
            Begin(Matrix.Identity, Matrix.Identity);
        }

        public void Begin(Matrix view, Matrix projection)
        {
            Begin(view, projection, null, null);
        }

        public void Begin(Matrix view, Matrix projection, SamplerState samplerState, RasterizerState rasterizerState)
        {
            if (hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            this.view = view;
            this.projection = projection;
            this.samplerState = samplerState;
            this.rasterizerState = rasterizerState;
            this.hasBegin = true;
            this.VertexCount = 0;
            this.PrimitiveCount = 0;
        }

        public void Draw(ParticleEffect particleEffect)
        {   
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            if (particleEffect.Texture != null)
            {
                if (batches.Count <= batchCount)
                {
                    batchCount++;
                    batches.Add(new ParticleBatchItem()
                    {
                        Enabled = true,
                        Type = particleEffect.ParticleType,
                        ParticleEffect = particleEffect,
                        Axis = particleEffect.Up
                    });
                }
                else
                {
                    var item = batches[batchCount++];
                    item.Enabled = true;
                    item.Type = particleEffect.ParticleType;
                    item.ParticleEffect = particleEffect;
                    item.Axis = particleEffect.Up;
                }
            }

            for (int i = 0; i < particleEffect.ChildEffects.Count; i++)
                Draw(particleEffect.ChildEffects[i]);

            for (int i = 0; i < particleEffect.EndingEffects.Count; i++)
                Draw(particleEffect.EndingEffects[i]);
        }

        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            if (batchCount > 0)
            {
                batches.Sort(0, batchCount, comparer);
                DrawBatches(DepthStencilState.Default, alphaTestEffect, true);
                DrawBatches(DepthStencilState.DepthRead, null, false);

                for (int i = 0; i < batchCount; i++)
                {
                    var batch = batches[i];
                    batch.Enabled = false;
                    batch.ParticleEffect = null;
                }
                batchCount = 0;
            }

            hasBegin = false;

            PrimitiveCount += primitiveBatch.PrimitiveCount;
            VertexCount += primitiveBatch.VertexCount;
        }

        private void DrawBatches(DepthStencilState depthStencilState, Effect effect, bool depthSort)
        {
            var blendState = batches[0].ParticleEffect.BlendState;

            primitiveBatch.Begin(PrimitiveSortMode.Deferred, view, projection, blendState, samplerState, depthStencilState, rasterizerState, effect);
            for (int i = 0; i < batchCount; i++)
            {
                if (depthSort)
                {
                    if (!batches[i].ParticleEffect.DepthSortEnabled)
                        continue;
                    alphaTestEffect.ReferenceAlpha = batches[i].ParticleEffect.ReferenceAlpha;
                }
                if (batches[i].ParticleEffect.BlendState != blendState)
                {
                    primitiveBatch.End();
                    blendState = batches[i].ParticleEffect.BlendState;
                    primitiveBatch.Begin(PrimitiveSortMode.Deferred, view, projection, blendState, samplerState, depthStencilState, rasterizerState, effect);
                }
                Draw(batches[i]);
            }
            primitiveBatch.End();
        }

        private void Draw(ParticleBatchItem item)
        {
            if (!item.Enabled)
                throw new InvalidOperationException();

            this.item = item;
            this.particleEffect = item.ParticleEffect;
            this.textureTransform = null;
            if (particleEffect.SourceRectangle.HasValue)
                textureTransform = TextureTransform.CreateSourceRectange(particleEffect.Texture, particleEffect.SourceRectangle);

            if (item.Type == ParticleType.Billboard)
            {
                particleEffect.ForEach(cachedDrawBillboard);
            }
            else if (item.Type == ParticleType.ConstrainedBillboard)
            {
                particleEffect.ForEach(cachedDrawConstrainedBillboard);
            }
            else if (item.Type == ParticleType.ConstrainedBillboardUp)
            {
                particleEffect.ForEach(cachedDrawConstrainedBillboardUp);
            }
            else if (item.Type == ParticleType.RibbonTrail)
            {
                particleEffect.ForEach(cachedDrawRibbonTrail);
            }

            this.particleEffect = null;
            this.item = null;
        }

        private void DrawParticleBillboard(ref Particle particle)
        {
            primitiveBatch.DrawBillboard(particleEffect.Texture,
                                         particle.Position,
                                         particle.Size,
                                         particle.Size,
                                         particle.Rotation, Vector3.UnitZ, textureTransform, null,
                                         particle.Color * particle.Alpha);
        }

        private void DrawParticleConstrainedBillboard(ref Particle particle)
        {
            Vector3 forward = Vector3.Normalize(particle.Velocity);
            forward *= 0.5f * particle.Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

            primitiveBatch.DrawConstrainedBillboard(particleEffect.Texture,
                                        particle.Position - forward,
                                        particle.Position + forward,
                                        particle.Size,
                                        textureTransform, null,
                                        particle.Color * particle.Alpha);
        }

        private void DrawParticleConstrainedBillboardUp(ref Particle particle)
        {
            Vector3 forward = 0.5f * item.Axis * particle.Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

            primitiveBatch.DrawConstrainedBillboard(particleEffect.Texture,
                                        particle.Position - forward,
                                        particle.Position + forward,
                                        particle.Size,
                                        textureTransform, null,
                                        particle.Color * particle.Alpha);
        }

        private void DrawParticleRibbonTrail(ref Particle particle)
        {
            Vector3 forward = 0.5f * item.Axis * particle.Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

            primitiveBatch.DrawConstrainedBillboard(particleEffect.Texture,
                                        particle.Position - forward,
                                        particle.Position + forward,
                                        particle.Size,
                                        textureTransform, null,
                                        particle.Color * particle.Alpha);
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

    class ParticleBatchItem
    {
        public bool Enabled;
        public ParticleType Type;
        public ParticleEffect ParticleEffect;
        public Vector3 Axis;
    }

    class ParticleBatchSortComparer : IComparer<ParticleBatchItem>
    {
        public int Compare(ParticleBatchItem x, ParticleBatchItem y)
        {
            return Compare(x.ParticleEffect.BlendState, y.ParticleEffect.BlendState);
        }

        private int Compare(BlendState x, BlendState y)
        {
            int result = 0;
            if ((result = ((int)x.AlphaBlendFunction).CompareTo((int)y.AlphaBlendFunction)) != 0)
                return result;
            if ((result = ((int)x.AlphaDestinationBlend).CompareTo((int)y.AlphaDestinationBlend)) != 0)
                return result;
            if ((result = ((int)x.AlphaSourceBlend).CompareTo((int)y.AlphaSourceBlend)) != 0)
                return result;
            if ((result = x.BlendFactor.PackedValue.CompareTo(y.BlendFactor.PackedValue)) != 0)
                return result;
            if ((result = ((int)x.ColorBlendFunction).CompareTo((int)y.ColorBlendFunction)) != 0)
                return result;
            if ((result = ((int)x.ColorDestinationBlend).CompareTo((int)y.ColorDestinationBlend)) != 0)
                return result;
            if ((result = ((int)x.ColorSourceBlend).CompareTo((int)y.ColorSourceBlend)) != 0)
                return result;
            if ((result = ((int)x.ColorWriteChannels).CompareTo((int)y.ColorWriteChannels)) != 0)
                return result;
#if !SILVERLIGHT
            if ((result = ((int)x.ColorWriteChannels1).CompareTo((int)y.ColorWriteChannels1)) != 0)
                return result;
            if ((result = ((int)x.ColorWriteChannels2).CompareTo((int)y.ColorWriteChannels2)) != 0)
                return result;
            if ((result = ((int)x.ColorWriteChannels3).CompareTo((int)y.ColorWriteChannels3)) != 0)
                return result;
            if ((result = x.MultiSampleMask.CompareTo(y.MultiSampleMask)) != 0)
                return result;
#endif
            return 0;
        }
    }
}
