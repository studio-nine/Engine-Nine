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
    /// Enables a group of drawing passes to be rendered one by one.
    /// </summary>
    public class DrawingPassGroup : DrawingPass
    {
        /// <summary>
        /// Gets the child passes of this drawing pass.
        /// </summary>
        public DrawingPassCollection Passes
        {
            get { return passes; }
        }
        private DrawingPassCollection passes = new DrawingPassCollection();

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingPassGroup"/> class.
        /// </summary>
        /// <param name="graphics"></param>
        public DrawingPassGroup()
        {

        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            var passOrder = Passes.GetEnabledAndSortedOrder();
            if (passOrder.Count <= 0)
                return;

            var overrideViewFrustumLastPass = false;

            var input = context.textures[TextureUsage.Previous] as RenderTarget2D;
            RenderTargetPool.Lock(input);

            try
            {
                for (int i = 0; i < passOrder.Count; i++)
                {
                    var pass = Passes[passOrder[i]];
                    var overrideViewFrustum = false;

                    if (pass != null && pass.Enabled)
                    {
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

                        pass.Draw(context, DynamicDrawables.Elements, 0, DynamicDrawables.Count);
                    }
                }
            }
            finally
            {
                RenderTargetPool.Unlock(input);
            }
        }
    }
}