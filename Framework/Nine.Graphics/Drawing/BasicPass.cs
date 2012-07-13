#region Copyright 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Materials;
using Nine.Graphics.ParticleEffects;
using Nine.Graphics.ObjectModel;
using System.Collections.Generic;
#endregion

namespace Nine.Graphics.Drawing
{
    /// <summary>
    /// A drawing pass represents a single pass in the composition chain.
    /// </summary>
    public class BasicPass : Pass
    {
        /// <summary>
        /// Gets or sets a value indicating whether the drawable list will be 
        /// sorted based on material before they are rendered.
        /// The default value is to sort the drawables.
        /// </summary>
        public bool SortEnabled { get; set; }

        /// <summary>
        /// Gets or sets the dominant material used for this drawing pass. If this
        /// value is null, each drawable will be drawed using its own material, otherwise
        /// all the drawables will use the material specified in this property.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets the material used when the drawable object do not have any material specified.
        /// </summary>
        public Material DefaultMaterial { get; set; }

        /// <summary>
        /// Gets or sets the material usage for this drawing pass. When the material usage
        /// is not MaterialUsage.Default, the drawing pass will use the associated material
        /// with this specified material usage to draw each object.
        /// </summary>
        public MaterialUsage MaterialUsage { get; set; }

        /// <summary>
        /// The comparer for drawable object material sorting.
        /// </summary>
        private IComparer<IDrawableObject> sortComparer;

        // TODO: Enable additive blending
        private DrawingQueue opaque = new DrawingQueue();
        private DrawingQueue opaqueTwoSided = new DrawingQueue();

        private DrawingQueue transparent = new DrawingQueue();
        private DrawingQueue transparentTwoSided = new DrawingQueue();

        /// <summary>
        /// Initializes a new instance of the <see cref="Pass"/> class.
        /// </summary>
        public BasicPass()
        {
            this.SortEnabled = false;
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        /// <param name="drawables">
        /// A list of drawables about to be drawed in this drawing pass.
        /// </param>
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            var count = drawables.Count;
            if (count <= 0)
                return;

            var graphics = context.GraphicsDevice;
            var dominantMaterial = Material;
            
            if (dominantMaterial != null)
            {
                // Draw with uniform material
                for (int i = 0; i < count; i++)
                {
                    var drawable = drawables[i];
                    if (drawable != null && drawable.Visible)
                    {
                        drawable.BeginDraw(context);
                        drawable.Draw(context, dominantMaterial);
                        drawable.EndDraw(context);
                    }
                }
            }
            else
            {
                if (DefaultMaterial == null)
                    DefaultMaterial = new BasicMaterial(graphics) { LightingEnabled = true };

                // Begin Draw
                for (int i = 0; i < count; i++)
                {
                    var drawable = drawables[i];
                    if (drawable != null && drawable.Visible)
                    {
                        drawable.BeginDraw(context);
                        
                        var material = drawable.Material;
                        if (MaterialUsage != MaterialUsage.Default && material != null)
                            material = material[MaterialUsage];

                        if (material == null)
                        {
                            opaque.Add(drawable, DefaultMaterial);
                            continue;
                        }

                        while (material != null)
                        {
                            var twoSided = material.TwoSided;
                            if (material.IsTransparent)
                            {
                                if (twoSided)
                                    transparentTwoSided.Add(drawable, material);
                                else
                                    transparent.Add(drawable, material);
                            }
                            else
                            {
                                if (twoSided)
                                    opaqueTwoSided.Add(drawable, material);
                                else
                                    opaque.Add(drawable, material);
                            }
                            material = material.NextMaterial;
                        }
                    }
                }

                if (SortEnabled)
                {
                    opaque.Sort();
                    opaqueTwoSided.Sort();
                    transparent.Sort();
                    transparentTwoSided.Sort();
                }

                // Draw opaque objects                
                graphics.DepthStencilState = DepthStencilState.Default;
                graphics.BlendState = BlendState.Opaque;

                if (opaque.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                    for (int i = 0; i < opaque.Count; i++)
                    {
                        var entry = opaque.Elements[i];
                        entry.Drawable.Draw(context, entry.Material);
                    }                    
                }

                if (opaqueTwoSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullNone;

                    for (int i = 0; i < opaqueTwoSided.Count; i++)
                    {
                        var entry = opaqueTwoSided.Elements[i];
                        entry.Drawable.Draw(context, entry.Material);
                    }                    
                }

                // Draw transparent objects
                graphics.DepthStencilState = DepthStencilState.DepthRead;
                graphics.BlendState = BlendState.AlphaBlend;

                if (transparent.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                    for (int i = 0; i < transparent.Count; i++)
                    {
                        var entry = transparent.Elements[i];
                        entry.Drawable.Draw(context, entry.Material);
                    }
                }

                graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                if (transparentTwoSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullNone;

                    for (int i = 0; i < transparentTwoSided.Count; i++)
                    {
                        var entry = transparentTwoSided.Elements[i];
                        entry.Drawable.Draw(context, entry.Material);
                    }
                }

                opaque.Clear();
                opaqueTwoSided.Clear();
                transparent.Clear();
                transparentTwoSided.Clear();

                // End Draw
                for (int i = 0; i < count; i++)
                    drawables[i].EndDraw(context);
            }
        }
    }
}