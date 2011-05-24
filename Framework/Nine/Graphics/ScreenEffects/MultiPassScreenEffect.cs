#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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
    /// Defines a post processing effect combined from multiple passes.
    /// </summary>
    public class MultiPassScreenEffect : IScreenEffect, IUpdateObject, IEffectTexture
    {
        /// <summary>
        /// Gets the GraphicsDevice associated with this instance.
        /// </summary>
        public GraphicsDevice GraphicsDevice { get; private set; }

        /// <summary>
        /// Gets all the passes used by this <c>MultiPassScreenEffect</c>.
        /// </summary>
        public MultiPassScreenEffectPassCollection Passes { get; private set; }

        /// <summary>
        /// Gets or sets the effect to combine the result of all passes.
        /// </summary>
        public Effect CombineEffect { get; set; }

        /// <summary>
        /// Gets or sets the blend state of this <c>MultiPassScreenEffect</c>.
        /// </summary>
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets whether this <c>MultiPassScreenEffect</c> is enabled.
        /// </summary>
        public bool Enabled { get; set; }

        /// <summary>
        /// Gets or sets the surface format of the render target.
        /// Specify null to use the surface format of the current backbuffer.
        /// </summary>
        public SurfaceFormat? SurfaceFormat { get; set; }

        /// <summary>
        /// Gets or sets the render target size.
        /// Specify null to use input texture size.
        /// </summary>
        public Vector2? RenderTargetSize { get; set; }

        /// <summary>
        /// Gets or sets the render target scale. This value is multiplied with
        /// <c>RenderTargetSize</c> to determine the final size of the render target.
        /// </summary>
        public float RenderTargetScale { get; set; }

        private List<Texture2D> passResults = new List<Texture2D>();

        /// <summary>
        /// Creates a new instance of <c>MultiPassScreenEffect</c>.
        /// </summary>
        public MultiPassScreenEffect(GraphicsDevice graphics)
        {
            if (graphics == null)
                throw new ArgumentNullException("graphics");

            GraphicsDevice = graphics;
            Enabled = true;
            BlendState = BlendState.Opaque;
            RenderTargetScale = 1;
            Passes = new MultiPassScreenEffectPassCollection();
        }

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            IEffectTexture update = CombineEffect as IEffectTexture;
            if (update != null)
                update.SetTexture(name, texture);

            foreach (IEffectTexture pass in Passes)
                pass.SetTexture(name, texture);
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual Texture2D Process(Texture2D input)
        {
            if (!Enabled || Passes.Count <= 0)
                return input;

            RenderTargetPool.AddRef(input as RenderTarget2D);
            RenderTarget2D renderTarget = RenderTargetPool.AddRef(GraphicsDevice, input, RenderTargetSize, RenderTargetScale, SurfaceFormat);
            renderTarget.Begin();

            ((IScreenEffect)this).ProcessAndDraw(input);

            RenderTargetPool.Release(renderTarget);
            RenderTargetPool.Release(input as RenderTarget2D);
            return renderTarget.End();
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void ProcessAndDraw(Texture2D input)
        {
            if (!Enabled || Passes.Count <= 0)
                return;

            RenderTargetPool.AddRef(input as RenderTarget2D);

            foreach (MultiPassScreenEffectPass pass in Passes)
            {
                if (pass.Enabled)
                {
                    if (pass.Effects.Count > 0)
                    {
                        RenderTarget2D passRenderTarget = RenderTargetPool.AddRef(GraphicsDevice, input, pass.RenderTargetSize, pass.RenderTargetScale, pass.SurfaceFormat);
                        passResults.Add(passRenderTarget);
                        passRenderTarget.Begin();
                        pass.ProcessAndDraw(input);
                        passRenderTarget.End();
                    }
                    else
                    {
                        passResults.Add(null);
                    }
                }
            }

            RenderTargetPool.Release(input as RenderTarget2D);

            for (int i = 0; i < Passes.Count; i++)
            {
                MultiPassScreenEffectPass pass = Passes[i];
                if (!pass.Enabled)
                    continue;

                Texture2D texture = pass.Effects.Count > 0 ? passResults[i] : input;

                if (CombineEffect != null)
                {
                    if (CombineEffect is IEffectTexture && !string.IsNullOrEmpty(pass.OutputTextureName))
                        ((IEffectTexture)CombineEffect).SetTexture(pass.OutputTextureName, texture);
                }
                else
                {
                    GraphicsDevice.DrawFullscreenQuad(texture, SamplerState.PointClamp, pass.BlendState, pass.Color, null);
                }
            }

            if (CombineEffect != null)
            {
                Texture2D texture = Passes.Count > 0 && passResults[0] != null ? passResults[0] : input;
                GraphicsDevice.DrawFullscreenQuad(texture, SamplerState.PointClamp, BlendState, Color.White, CombineEffect);
            }

            foreach (Texture2D passRenderTarget in passResults)
            {
                RenderTargetPool.Release(passRenderTarget as RenderTarget2D);
            }
            passResults.Clear();
        }
        
        [EditorBrowsable(EditorBrowsableState.Never)]
        public virtual void Update(GameTime gameTime)
        {
            if (Enabled)
            {
                foreach (IUpdateObject pass in Passes)
                    pass.Update(gameTime);

                if (CombineEffect is IUpdateObject)
                    ((IUpdateObject)CombineEffect).Update(gameTime);
            }
        }
    }

    /// <summary>
    /// Defines a pass used by <c>MultiPassScreenEffect</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MultiPassScreenEffectPass : ChainedScreenEffect
    {
        /// <summary>
        /// Gets or sets the surface format of the render target.
        /// Specify null to use the surface format of the current backbuffer.
        /// </summary>
        public SurfaceFormat? SurfaceFormat { get; set; }

        /// <summary>
        /// Gets or sets the render target size.
        /// Specify null to use input texture size.
        /// </summary>
        public Vector2? RenderTargetSize { get; set; }

        /// <summary>
        /// Gets or sets the render target scale. This value is multiplied with
        /// <c>RenderTargetSize</c> to determine the final size of the render target.
        /// </summary>
        public float RenderTargetScale { get; set; }

        /// <summary>
        /// Gets or sets the blend state of this pass. This value will
        /// determine how to blend with other passes when a <c>CombineEffect</c>
        /// is now found.
        /// </summary>
        public BlendState BlendState { get; set; }

        /// <summary>
        /// Gets or sets a Color value that is multiplied to this <c>MultiPassScreenEffectPass</c>
        /// when a <c>CombineEffect</c> is not found.
        /// </summary>
        public Color Color { get; set; }

        /// <summary>
        /// Gets or sets the <c>TextureName</c> of the output texture.
        /// The texture produced by this pass will be feed to the <c>CombineEffect</c>
        /// property of the parent <c>MultiPassScreenEffect</c> through
        /// <c>IEffectTexture</c> interface.
        /// </summary>
        public string OutputTextureName { get; set; }

        /// <summary>
        /// Creates a new instance of <c>ScreenEffectEdge</c>.
        /// </summary>
        public MultiPassScreenEffectPass() 
        {
            BlendState = BlendState.Opaque;
            Color = Color.White;
            RenderTargetScale = 1;
        }
    }

    /// <summary>
    /// Represents a collection of <c>ScreenEffectEdge</c>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MultiPassScreenEffectPassCollection : Collection<MultiPassScreenEffectPass>
    {
        internal MultiPassScreenEffectPassCollection() { }
    }
}

