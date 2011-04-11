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
    class ScreenEffectRenderTargetPool : IDisposable
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

    class ScreenEffectRenderTargetDownScalePool : ScreenEffectRenderTargetPool
    {
        public ScreenEffectRenderTargetDownScalePool(GraphicsDevice graphics) : base(graphics, 0.5f) { }
    }
}

