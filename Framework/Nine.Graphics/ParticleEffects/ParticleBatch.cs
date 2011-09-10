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

        private bool hasBegin = false;
        private PrimitiveBatch primitiveBatch;
        private List<ParticleBatchItem> batches;
        private ParticleBatchSortComparer comparer = new ParticleBatchSortComparer();

        private SamplerState samplerState;
        private DepthStencilState depthStencilState;
        private RasterizerState rasterizerState;

        private Matrix view;
        private Matrix projection;

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
        }

        public void Begin()
        {
            Begin(Matrix.Identity, Matrix.Identity);
        }

        public void Begin(Matrix view, Matrix projection)
        {
            Begin(view, projection, null, null, null);
        }

        public void Begin(Matrix view, Matrix projection, SamplerState samplerState, DepthStencilState depthStencilState, RasterizerState rasterizerState)
        {
            if (hasBegin)
                throw new InvalidOperationException(Strings.AlreadyInBeginEndPair);

            this.view = view;
            this.projection = projection;
            this.samplerState = samplerState;
            this.depthStencilState = depthStencilState;
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
                batches.Add(new ParticleBatchItem() { Type = particleEffect.ParticleType, ParticleEffect = particleEffect, Axis = particleEffect.Up  });
            
            foreach (var childEffect in particleEffect.ChildEffects)
                Draw(childEffect);

            foreach (var endingEffect in particleEffect.EndingEffects)
                Draw(endingEffect);
        }

        public void End()
        {
            if (!hasBegin)
                throw new InvalidOperationException(Strings.NotInBeginEndPair);

            if (batches.Count > 0)
            {
                batches.Sort(comparer);
                BlendState blendState = batches[0].ParticleEffect.BlendState;
                primitiveBatch.Begin(PrimitiveSortMode.Deferred, view, projection, blendState, samplerState, depthStencilState, rasterizerState);
                for (int i = 0; i < batches.Count; i++)
                {
                    if (batches[i].ParticleEffect.BlendState != blendState)
                    {
                        primitiveBatch.End();
                        blendState = batches[i].ParticleEffect.BlendState;
                        primitiveBatch.Begin(PrimitiveSortMode.Deferred, view, projection, blendState, samplerState, depthStencilState, rasterizerState);
                    }
                    Draw(batches[i]);
                }
                primitiveBatch.End();
            }

            batches.Clear();
            hasBegin = false;

            PrimitiveCount += primitiveBatch.PrimitiveCount;
            VertexCount += primitiveBatch.VertexCount;
        }

        private void Draw(ParticleBatchItem item)
        {
            ParticleEffect particleEffect = item.ParticleEffect;

            Matrix? textureTransform = null;
            if (particleEffect.SourceRectangle.HasValue)
                textureTransform = TextureTransform.CreateSourceRectange(particleEffect.Texture, particleEffect.SourceRectangle);

            if (item.Type == ParticleType.Billboard)
            {
                particleEffect.ForEach((ref Particle particle) =>
                {
                    primitiveBatch.DrawBillboard(particleEffect.Texture,
                                                particle.Position,
                                                particle.Size,
                                                particle.Size,
                                                particle.Rotation, Vector3.UnitZ, textureTransform, null,
                                                particle.Color * particle.Alpha);
                });
            }
            else if (item.Type == ParticleType.ConstrainedBillboard)
            {
                particleEffect.ForEach((ref Particle particle) =>
                {
                    Vector3 forward = Vector3.Normalize(particle.Velocity);
                    forward *= 0.5f * particle.Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

                    primitiveBatch.DrawConstrainedBillboard(particleEffect.Texture,
                                                particle.Position - forward,
                                                particle.Position + forward,
                                                particle.Size,
                                                textureTransform, null,
                                                particle.Color * particle.Alpha);
                });
            }
            else if (item.Type == ParticleType.ConstrainedBillboardUp)
            {
                particleEffect.ForEach((ref Particle particle) =>
                {
                    Vector3 forward = 0.5f * item.Axis * particle.Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

                    primitiveBatch.DrawConstrainedBillboard(particleEffect.Texture,
                                                particle.Position - forward,
                                                particle.Position + forward,
                                                particle.Size,
                                                textureTransform, null,
                                                particle.Color * particle.Alpha);
                });
            }
            else if (item.Type == ParticleType.RibbonTrail)
            {
                particleEffect.ForEach((ref Particle particle) =>
                {
                    Vector3 forward = 0.5f * item.Axis * particle.Size * particleEffect.Stretch * particleEffect.Texture.Width / particleEffect.Texture.Height;

                    primitiveBatch.DrawConstrainedBillboard(particleEffect.Texture,
                                                particle.Position - forward,
                                                particle.Position + forward,
                                                particle.Size,
                                                textureTransform, null,
                                                particle.Color * particle.Alpha);
                });
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
            if ((result = x.AlphaBlendFunction.CompareTo(y.AlphaBlendFunction)) != 0)
                return result;
            if ((result = x.AlphaDestinationBlend.CompareTo(y.AlphaDestinationBlend)) != 0)
                return result;
            if ((result = x.AlphaSourceBlend.CompareTo(y.AlphaSourceBlend)) != 0)
                return result;
            if ((result = x.BlendFactor.PackedValue.CompareTo(y.BlendFactor.PackedValue)) != 0)
                return result;
            if ((result = x.ColorBlendFunction.CompareTo(y.ColorBlendFunction)) != 0)
                return result;
            if ((result = x.ColorDestinationBlend.CompareTo(y.ColorDestinationBlend)) != 0)
                return result;
            if ((result = x.ColorSourceBlend.CompareTo(y.ColorSourceBlend)) != 0)
                return result;
            if ((result = x.ColorWriteChannels.CompareTo(y.ColorWriteChannels)) != 0)
                return result;
            if ((result = x.ColorWriteChannels1.CompareTo(y.ColorWriteChannels1)) != 0)
                return result;
            if ((result = x.ColorWriteChannels2.CompareTo(y.ColorWriteChannels2)) != 0)
                return result;
            if ((result = x.ColorWriteChannels3.CompareTo(y.ColorWriteChannels3)) != 0)
                return result;
            if ((result = x.MultiSampleMask.CompareTo(y.MultiSampleMask)) != 0)
                return result;
            return 0;
        }
    }
}
