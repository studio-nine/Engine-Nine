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
using Isles.Graphics.Vertices;
#endregion


namespace Isles.Graphics.ParticleEffects
{
    public sealed class ParticleEffectBatch
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
        private Effect effect;
        private GameTime time;


        public GraphicsDevice GraphicsDevice { get; private set; }
        public bool IsDisposed { get; private set; }


        public ParticleEffectBatch(GraphicsDevice graphics, int capacity)
        {
            if (capacity < 4)
                throw new ArgumentException("Capacity should be at least 4");

            GraphicsDevice = graphics;

            effect = new ParticleShaderEffect(graphics);

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
            if ((particleEffect.MinRotateSpeed == 0) && (particleEffect.MaxRotateSpeed == 0))
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


            EffectParameterCollection parameters = effect.Parameters;

            // Look up shortcuts for parameters that change every frame.
            parameters["View"].SetValue(view);
            parameters["Projection"].SetValue(projection);
            parameters["ViewportHeight"].SetValue(GraphicsDevice.Viewport.Height);
            parameters["CurrentTime"].SetValue((float)time.TotalGameTime.TotalSeconds);


            //BeginParticleRenderStates(GraphicsDevice.RenderState);
            {
                foreach (BatchItem<Key> batchItem in batch.Batches)
                {
                    parameters["Texture"].SetValue(batchItem.Key.Texture);

                    for (int i = 0; i < batchItem.Count; i++)
                    {
                        ParticleEffect particleEffect = batch.Values[batchItem.StartIndex + i];

                        parameters["Duration"].SetValue((float)particleEffect.Duration.TotalSeconds);
                        parameters["DurationRandomness"].SetValue(particleEffect.DurationRandomness);
                        parameters["Gravity"].SetValue(particleEffect.Gravity);
                        parameters["EndVelocity"].SetValue(particleEffect.EndVelocity);
                        parameters["MinColor"].SetValue(particleEffect.MinColor.ToVector4());
                        parameters["MaxColor"].SetValue(particleEffect.MaxColor.ToVector4());
                        parameters["RotateSpeed"].SetValue(new Vector2(particleEffect.MinRotateSpeed, particleEffect.MaxRotateSpeed));
                        parameters["StartSize"].SetValue(new Vector2(particleEffect.MinStartSize, particleEffect.MaxStartSize));
                        parameters["EndSize"].SetValue(new Vector2(particleEffect.MinEndSize, particleEffect.MaxEndSize));

                        batchItem.Key.Technique.Passes[0].Apply();

                        particleEffect.Draw(GraphicsDevice, time);
                    }
                }
            }
            //EndParticleRenderStates(GraphicsDevice.RenderState);
        }


        /*
        /// <summary>
        /// Helper for setting the renderstates used to draw particles.
        /// </summary>
        void BeginParticleRenderStates(BlendState renderState)
        {
            // Enable point sprites.
            renderState.PointSpriteEnable = true;
            renderState.PointSizeMax = 256;

            // Set the alpha blend mode.
            renderState.SetSpriteBlendMode(blendMode);

            // Set the alpha test mode.
            renderState.AlphaTestEnable = true;
            renderState.AlphaFunction = CompareFunction.Greater;
            renderState.ReferenceAlpha = 0;

            // Enable the depth buffer (so particles will not be visible through
            // solid objects like the ground plane), but disable depth writes
            // (so particles will not obscure other particles).
            renderState.DepthBufferEnable = true;
            renderState.DepthBufferWriteEnable = false;
        }


        /// <summary>
        /// Helper for setting the renderstates used to draw particles.
        /// </summary>
        void EndParticleRenderStates(RenderState renderState)
        {
            renderState.PointSpriteEnable = false;
            renderState.AlphaBlendEnable = false;
            renderState.SourceBlend = Blend.SourceAlpha;
            renderState.DestinationBlend = Blend.InverseSourceAlpha;
            renderState.AlphaTestEnable = false;
            renderState.DepthBufferWriteEnable = true;
        }
         */
    }
}
