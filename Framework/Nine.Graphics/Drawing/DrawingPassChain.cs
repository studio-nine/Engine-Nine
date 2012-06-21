#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ParticleEffects;
using System.Collections.Generic;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// Enables drawing pass chaining by rendering the previous pass to a render
    /// target and then propagate the result to the next pass.
    /// </summary>
    public class DrawingPassChain : DrawingPass
    {
        /// <summary>
        /// Gets the child passes of this drawing pass.
        /// </summary>
        public DrawingPassCollection Passes
        {
            get { return passes; }
        }
        private DrawingPassCollection passes = new DrawingPassCollection();

        private RenderTargetPool renderTargetPool;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingPassGroup"/> class.
        /// </summary>
        /// <param name="graphics"></param>
        public DrawingPassChain()
        {

        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            var overrideViewFrustumLastPass = false;

            var passOrder = Passes.GetEnabledAndSortedOrder();
            if (passOrder.Count <= 0)
                return;

            var input = context.textures[TextureUsage.Previous] as RenderTarget2D;
            var intermediate = input;
            RenderTargetPool.Lock(input);

            for (int i = 0; i < passOrder.Count; i++)
            {
                try
                {
                    var pass = Passes[passOrder[i]];
                    var overrideViewFrustum = false;

                    // Query the drawables in the current view frustum only when the view frustum changed
                    // or the pass overrides the frustum.
                    var passView = pass.View;
                    var passProjection = pass.Projection;

                    if (passView.HasValue)
                    {
                        View = passView.Value;
                        overrideViewFrustum = true;
                    }

                    if (passProjection.HasValue)
                    {
                        overrideViewFrustum = true;
                        Projection = passProjection.Value;
                    }

                    if (overrideViewFrustum || overrideViewFrustumLastPass)
                    {
                        DynamicDrawables.Clear();
                        BoundingFrustum viewFrustum = context.matrices.ViewFrustum;
                        context.Scene.FindAll(ref viewFrustum, DynamicDrawables);
                        overrideViewFrustumLastPass = overrideViewFrustum;
                    }

                    // Draw the pass the a render target if it is not the last pass
                    var notLastPass = (i != passOrder.Count - 1);
                    if (notLastPass)
                    {
                        intermediate = pass.PrepareRenderTarget(intermediate);
                        if (intermediate == null)
                        {
                            EnsureRenderTargetPool(context.GraphicsDevice);
                            intermediate = renderTargetPool.Create();
                        }
                        intermediate.Begin();
                        RenderTargetPool.Lock(intermediate);
                    }

                    // Clear the screen when we are drawing to the backbuffer.
                    else if (context.GraphicsDevice.GetRenderTargets().Length <= 0)
                    {   
                        context.GraphicsDevice.Clear(context.Settings.BackgroundColor);
                    }

                    pass.Draw(context, DynamicDrawables.Elements, 0, DynamicDrawables.Count);

                    if (notLastPass)
                    {
                        intermediate.End();
                        RenderTargetPool.Unlock(intermediate);
                        context.textures[TextureUsage.Previous] = intermediate;
                    }
                }
                finally
                {
                    // This input texture is not used once the first pass has used it.
                    if (i == 0)
                        RenderTargetPool.Unlock(input);
                }
            }
        }

        private void EnsureRenderTargetPool(GraphicsDevice graphics)
        {
            var pp = graphics.PresentationParameters;
            if (renderTargetPool == null)
                renderTargetPool = RenderTargetPool.AddRef(graphics
                                                         , graphics.Viewport.Width
                                                         , graphics.Viewport.Height
                                                         , false
                                                         , pp.BackBufferFormat
                                                         , pp.DepthStencilFormat
                                                         , pp.MultiSampleCount
                                                         , pp.RenderTargetUsage);
        }
    }
}