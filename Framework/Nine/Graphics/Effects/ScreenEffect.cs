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

namespace Nine.Graphics.Effects
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
        /// Gets or sets whether the size of the render target used by this 
        /// ScreenEffectPass will be scaled down to 1/4.
        /// </summary>
        public bool DownScaleEnabled { get; set; }

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
        /// Gets the effects used by this ScreenEffectPass.
        /// </summary>
        public IList<Effect> Effects { get; private set; }

        /// <summary>
        /// Gets the child passes contained by this ScreenEffectPass.
        /// </summary>
        public IList<ScreenEffectPass> Passes { get; private set; }

        /// <summary>
        /// Creates a new instance of ScreenEffectPass.
        /// </summary>
        public ScreenEffectPass(GraphicsDevice graphics) : this(graphics, false, BlendState.Additive)
        {

        }

        /// <summary>
        /// Creates a new instance of ScreenEffectPass.
        /// </summary>
        public ScreenEffectPass(GraphicsDevice graphics, bool downScaleEnabled, BlendState blend)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            Pool = GraphicsResources<ScreenEffectRenderTargetPool>.GetInstance(graphics);
            DownScalePool = GraphicsResources<ScreenEffectRenderTargetDownScalePool>.GetInstance(graphics);

            this.GraphicsDevice = graphics;
            this.DownScaleEnabled = downScaleEnabled;
            this.BlendState = blend;
            this.Color = Color.White;
            this.Passes = new List<ScreenEffectPass>();
            this.Effects = new List<Effect>();
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

            // Draw first n - 1 effects to render target
            for (int i = 0; i < count; i++)
            {
                current = pool.MoveNext();
                pool.Begin(current);

                // TODO: Add linear sampling.
                GraphicsDevice.DrawSprite(input, SamplerState.PointClamp, BlendState.Opaque, Color, Effects[i]);

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


    /// <summary>
    /// Represents post processing effects.
    /// </summary>
    public class ScreenEffect : ScreenEffectPass
    {
        int renderTarget = 0;

        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        /// <param name="graphics">A GraphicsDevice instance.</param>
        public ScreenEffect(GraphicsDevice graphics) : base(graphics, false, BlendState.Opaque) 
        {
            if (graphics.PresentationParameters.RenderTargetUsage != RenderTargetUsage.PreserveContents)
                throw new NotSupportedException("ScreenEffect requires RenderTargetUsage to be RenderTargetUsage.PreserveContents.");
        }

        /// <summary>
        /// Begins the rendering of the scene to be post processed.
        /// </summary>
        public void Begin()
        {
            if (Effects.Count <= 0 && Passes.Count <= 0)
                return;

            renderTarget = Pool.MoveNext();
            Pool.Begin(renderTarget);
        }

        /// <summary>
        /// Ends the rendering of the scene, applying all the post processing effects.
        /// </summary>
        public void End()
        {
            if (Effects.Count <= 0 && Passes.Count <= 0)
                return;

            Texture2D backbuffer = Pool.End(renderTarget);
            
            Pool.Lock(renderTarget);
            Draw(backbuffer);
            Pool.Unlock(renderTarget);
        }
    }

    internal class ScreenEffectRenderTargetPool : IDisposable
    {
        int current = 0;
        List<int> locked = new List<int>();
        List<RenderTarget2D> renderTargets = new List<RenderTarget2D>();
        GraphicsDevice graphics;
        float renderTargetScale;

        public ScreenEffectRenderTargetPool(GraphicsDevice graphics)
            : this(graphics, 1) 
        { }

        public ScreenEffectRenderTargetPool(GraphicsDevice graphics, float renderTargetScale)
        {
            this.graphics = graphics;
            this.renderTargetScale = renderTargetScale;
        }

        public int MoveNext()
        {
            for (int i = 0; i < renderTargets.Count; i++)
            {
                current = (current + 1) % renderTargets.Count;
                if (IsLock(current))
                    continue;
                return current;
            }

            renderTargets.Add(CreateRenderTarget());
            locked.Add(0);
            return current = renderTargets.Count - 1;
        }

        private RenderTarget2D CreateRenderTarget()
        {
            int width = (int)(graphics.PresentationParameters.BackBufferWidth * renderTargetScale);
            int height = (int)(graphics.PresentationParameters.BackBufferHeight * renderTargetScale);

            return new RenderTarget2D(graphics, width, height, false,
                                      graphics.PresentationParameters.BackBufferFormat,
                                      graphics.PresentationParameters.DepthStencilFormat,
                                      0, RenderTargetUsage.DiscardContents);
        }

        public RenderTarget2D this[int index]
        {
            get { return renderTargets[index]; }
        }

        public bool IsLock(int i) { return locked[i] > 0; }
        public void Lock(int i) { locked[i]++; }
        public void Unlock(int i) { locked[i]--; }
        public void Begin(int i) { renderTargets[i].Begin(); }
        public Texture2D End(int i) { return renderTargets[i].End(); }
        
        public void Dispose()
        {
            foreach (RenderTarget2D renderTarget in renderTargets)
            {
                if (renderTarget != null)
                    renderTarget.Dispose();
            }
            renderTargets.Clear();
        }
    }

    internal class ScreenEffectRenderTargetDownScalePool : ScreenEffectRenderTargetPool 
    {
        public ScreenEffectRenderTargetDownScalePool(GraphicsDevice graphics) : base(graphics, 0.5f) { }
    }
}

