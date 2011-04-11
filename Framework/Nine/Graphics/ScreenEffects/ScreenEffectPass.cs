#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.ScreenEffects
{
    /// <summary>
    /// Defines a pass in a post processing effect.
    /// A ScreenEffect pass is a collection of effects that are processed
    /// indepedent of other passes. The result of each pass is merged
    /// to produce the final result.
    /// </summary>
    public class ScreenEffectPass
    {
        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets or sets the BlendState of this ScreenEffectPass to blend with other passes.
        /// The default value is BlendState.Additive.
        /// </summary>
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets a Color value that is multiplied to this ScreenEffectPass.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets whether the size of the render target used by this 
        /// ScreenEffectPass will be scaled down to 1/4.
        /// </summary>
        public bool DownScaleEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether filter should be applied when down scaled textures are sampled.
        /// </summary>
        public bool FilteringEnabled { get; set; }

        /// <summary>
        /// Gets the effects used by this ScreenEffectPass.
        /// </summary>
        public List<Effect> Effects { get; private set; }

        /// <summary>
        /// Gets the child passes contained by this ScreenEffectPass.
        /// </summary>
        [ContentSerializer(Optional = true)]
        public List<ScreenEffectPass> Passes { get; private set; }

        internal ScreenEffectPass() { }

        /// <summary>
        /// Creates a new instance of ScreenEffectPass.
        /// </summary>
        public ScreenEffectPass(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Pool = GraphicsResources<ScreenEffectRenderTargetPool>.GetInstance(graphics);
            DownScalePool = GraphicsResources<ScreenEffectRenderTargetDownScalePool>.GetInstance(graphics);

            this.GraphicsDevice = graphics;
            this.DownScaleEnabled = false;
            this.BlendState = BlendState.Additive;
            this.FilteringEnabled = true;
            this.Color = Color.White;
            this.Passes = new List<ScreenEffectPass>();
            this.Effects = new List<Effect>();
        }

        /// <summary>
        /// Creates a new instance of ScreenEffectPass.
        /// </summary>
        public ScreenEffectPass(GraphicsDevice graphics, params Effect[] effects)
            : this(graphics)
        {
            this.Effects.AddRange(effects);
        }

        /// <summary>
        /// Draw each effect and each pass in this pass with the input texture.
        /// </summary>
        internal void Draw(Texture2D input)
        {
            ScreenEffectRenderTargetPool pool = DownScaleEnabled ? DownScalePool : Pool;

            int current = -1;
            int previous = -1;
            int count = Passes.Count <= 0 ? Effects.Count - 1 : Effects.Count;

            ScaleEffect scaleEffect = null;
            if (DownScaleEnabled && FilteringEnabled && count > 0)
            {
                // Use software filtering.
                scaleEffect = GraphicsResources<ScaleEffect>.GetInstance(GraphicsDevice);
                scaleEffect.SourceTextureDimensions = new Vector2(input.Width, input.Height);
            }

            // Draw first n - 1 effects to render target
            for (int i = 0; i < count; i++)
            {
                // Downscale
                if (i == 0 && DownScaleEnabled && FilteringEnabled)
                {
                    current = pool.MoveNext();
                    pool.Begin(current);

                    GraphicsDevice.DrawSprite(input, SamplerState.PointClamp, BlendState.Opaque, Color, scaleEffect);

                    input = pool.End(current);
                    if (previous >= 0)
                        pool.Unlock(previous);
                    pool.Lock(current);
                    previous = current;
                }


                current = pool.MoveNext();
                pool.Begin(current);

                GraphicsDevice.DrawSprite(input, SamplerState.PointClamp, BlendState.Opaque, Color, Effects[i]);

                input = pool.End(current);
                if (previous >= 0)
                    pool.Unlock(previous);
                pool.Lock(current);
                previous = current;
            }

            // Upscale
            if (DownScaleEnabled && FilteringEnabled && count > 0)
            {
                current = pool.MoveNext();
                pool.Begin(current);

                scaleEffect.SourceTextureDimensions = new Vector2(input.Width, input.Height);
                GraphicsDevice.DrawSprite(input, SamplerState.PointClamp, BlendState.Opaque, Color, scaleEffect);

                input = pool.End(current);
                if (previous >= 0)
                    pool.Unlock(previous);
                pool.Lock(current);
                previous = current;
            }

            if (previous >= 0)
                pool.Unlock(previous);

            if (Passes.Count <= 0)
            {
                if (Effects.Count <= 0)
                    GraphicsDevice.DrawSprite(input, SamplerState.PointClamp, BlendState, Color);
                else
                    GraphicsDevice.DrawSprite(input, SamplerState.PointClamp, BlendState, Color, Effects[Effects.Count - 1]);
                return;
            }

            // Draw each passes
            if (current >= 0)
                pool.Lock(current);
            foreach (ScreenEffectPass pass in Passes)
            {
                pass.Draw(input);
            }
            if (current >= 0)
                pool.Unlock(current);
        }

        internal static ScreenEffectRenderTargetPool Pool;
        internal static ScreenEffectRenderTargetDownScalePool DownScalePool;
    }

    class ScreenEffectPassReader : ContentTypeReader<ScreenEffectPass>
    {
        protected override ScreenEffectPass Read(ContentReader input, ScreenEffectPass existingInstance)
        {
            return InternalRead(input, existingInstance);
        }

        internal static ScreenEffectPass InternalRead(ContentReader input, ScreenEffectPass existingInstance)
        {
            if (existingInstance == null)
                existingInstance = new ScreenEffectPass(input.ContentManager.ServiceProvider.GetService<IGraphicsDeviceService>().GraphicsDevice);

            existingInstance.BlendState = input.ReadObject<BlendState>();
            existingInstance.Color = input.ReadColor();
            existingInstance.DownScaleEnabled = input.ReadBoolean();
            existingInstance.FilteringEnabled = input.ReadBoolean();
            int nEffect = input.ReadInt32();
            for (int i = 0; i < nEffect; i++)
                existingInstance.Effects.Add(input.ReadObject<Effect>());
            int nPass = input.ReadInt32();
            for (int i = 0; i < nPass; i++)
                existingInstance.Passes.Add(input.ReadObject<ScreenEffectPass>());
            return existingInstance;
        }
    }
}

