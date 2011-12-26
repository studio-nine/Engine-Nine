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
#if !WINDOWS_PHONE
        /// <summary>
        /// Gets or sets the distance where the particle pixels start to fade.
        /// </summary>
        public float SoftParticleFade
        {
            get { return softParticleEffect.DepthFade; }
            set { softParticleEffect.DepthFade = value; }
        }

        /// <summary>
        /// Gets or sets the current depth buffer to enable the rendering of soft particles.
        /// </summary>
        public Texture2D DepthBuffer
        {
            get { return softParticleEffect.DepthBuffer; }
            set { softParticleEffect.DepthBuffer = value; }
        }
#endif
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
#if !WINDOWS_PHONE
        private SoftParticleEffect softParticleEffect;
#endif
        private RenderTarget2D blendTexture;
        private PrimitiveBatch primitiveBatch;
        private List<ParticleBatchItem> batches;
        private HashSet<BlendState> backgroundBlendStates = new HashSet<BlendState>();
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
#if !WINDOWS_PHONE
            softParticleEffect = new SoftParticleEffect(graphics);
#endif
                        
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
            this.backgroundBlendStates.Clear();
        }

        public void Draw(ParticleEffect particleEffect)
        {   
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            for (int i = 0; i < particleEffect.ChildEffects.Count; i++)
                Draw(particleEffect.ChildEffects[i]);

            if (particleEffect.Texture != null)
            {
                BlendState backgroundBlendState = null;
                if (particleEffect.BackgroundBlendState != null && particleEffect.BackgroundBlendState != particleEffect.BlendState)
                {
                    backgroundBlendState = particleEffect.BackgroundBlendState ?? particleEffect.BlendState;
                    backgroundBlendStates.Add(backgroundBlendState);
                }

                if (batches.Count <= batchCount)
                {
                    batchCount++;
                    batches.Add(new ParticleBatchItem()
                    {
                        Enabled = true,
                        Type = particleEffect.ParticleType,
                        ParticleEffect = particleEffect,
                        Axis = particleEffect.Up,
                        BackgroundBlendState = backgroundBlendState,
                    });
                }
                else
                {
                    var item = batches[batchCount++];
                    item.Enabled = true;
                    item.Type = particleEffect.ParticleType;
                    item.ParticleEffect = particleEffect;
                    item.Axis = particleEffect.Up;
                    item.BackgroundBlendState = backgroundBlendState;
                }
            }

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

                DrawBatches(DepthStencilState.Default, alphaTestEffect, true, false, null);
                DrawBatches(DepthStencilState.DepthRead, null, false, false, null);
#if !WINDOWS_PHONE
                DrawBatches(DepthStencilState.DepthRead, softParticleEffect, false, true, null);
#endif

                if (backgroundBlendStates.Count > 0)
                {
                    EnsureBlendTexture();

                    foreach (var backgroundBlendState in backgroundBlendStates)
                    {
                        blendTexture.Begin();

                        DrawBatches(DepthStencilState.Default, alphaTestEffect, true, false, backgroundBlendState);
                        DrawBatches(DepthStencilState.DepthRead, null, false, false, backgroundBlendState);
#if !WINDOWS_PHONE
                        DrawBatches(DepthStencilState.DepthRead, softParticleEffect, false, true, null);
#endif

                        blendTexture.End();

                        // TODO: Stencil optimization?
                        GraphicsDevice.DrawFullscreenQuad(blendTexture, SamplerState.PointClamp, Color.White, null);
                    }
                }

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

        private void DrawBatches(DepthStencilState depthStencilState, Effect effect, bool depthSort, bool softParticle, BlendState compareBlendState)
        {
            var blendState = batches[0].ParticleEffect.BlendState;

            primitiveBatch.Begin(PrimitiveSortMode.Deferred, view, projection, blendState, samplerState, depthStencilState, rasterizerState, effect);
            for (int i = 0; i < batchCount; i++)
            {
                var batch = batches[i];
                if (batch.BackgroundBlendState != compareBlendState)
                    continue;

                if (depthSort)
                {
                    if (!batch.ParticleEffect.DepthSortEnabled)
                        continue;
                    
                    // FIXME: The alpha test effect has to be applied.
                    alphaTestEffect.ReferenceAlpha = batch.ParticleEffect.ReferenceAlpha;
                }
                if (batch.ParticleEffect.BlendState != blendState)
                {
                    primitiveBatch.End();
                    blendState = batch.ParticleEffect.BlendState;
                    primitiveBatch.Begin(PrimitiveSortMode.Deferred, view, projection, blendState, samplerState, depthStencilState, rasterizerState, effect);
                }

#if WINDOWS_PHONE
                Draw(batch);
#else
                // Draw soft particles only when a valid depth buffer is set.
                bool softParticleEnabled = batch.ParticleEffect.SoftParticleEnabled & (DepthBuffer != null);
                if (depthSort || (!(softParticle ^ softParticleEnabled)))
                    Draw(batch);
#endif
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
                textureTransform = TextureTransform.CreateFromSourceRectange(particleEffect.Texture, particleEffect.SourceRectangle);

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
                                         particle.Rotation, 
                                         particleEffect.Up, textureTransform, null,
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

        private void EnsureBlendTexture()
        {
            if (blendTexture == null || blendTexture.IsDisposed ||
#if !SILVERLIGHT
                blendTexture.IsContentLost ||
#endif
                blendTexture.Width != GraphicsDevice.Viewport.Width || blendTexture.Height != GraphicsDevice.Viewport.Height)
            {
                // Depth buffers are disabled since we cannot share them between render targets in Xna 4.0
                blendTexture = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                                                  GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.None);
            }
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
                if (blendTexture != null)
                {
                    blendTexture.Dispose();
                    blendTexture = null;
                }
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
        public BlendState BackgroundBlendState;
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
