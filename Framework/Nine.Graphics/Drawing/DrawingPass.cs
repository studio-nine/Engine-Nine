namespace Nine.Graphics.Drawing
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics;

    /// <summary>
    /// A drawing pass represents a single pass in the composition chain.
    /// </summary>
    [NotContentSerializable]
    public class DrawingPass : Pass
    {
        /// <summary>
        /// Gets or sets a value indicating whether the background will be cleared to the background color
        /// specified in settings. The default value is false.
        /// </summary>
        internal bool ClearBackground;

        /// <summary>
        /// Gets or sets a value indicating whether the drawable list will be sorted based on material before 
        /// they are rendered. The default value is false.
        /// </summary>
        public bool MaterialSortEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether transparent objects will be sorted base on their distance
        /// to the camera. The default value is false.
        /// </summary>
        public bool TransparencySortEnabled { get; set; }

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

        private DrawingQueue opaque = new DrawingQueue();
        private DrawingQueue opaqueTwoSided = new DrawingQueue();

        private DrawingQueue transparent = new DrawingQueue();
        private DrawingQueue transparentTwoSided = new DrawingQueue();

        /// <summary>
        /// Initializes a new instance of the <see cref="Pass"/> class.
        /// </summary>
        public DrawingPass()
        {
            this.MaterialSortEnabled = false;
            this.TransparencySortEnabled = false;
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        /// <param name="drawables">
        /// A list of drawables about to be rendered in this drawing pass.
        /// </param>
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (ClearBackground)
                context.graphics.Clear(context.settings.BackgroundColor);

            var count = drawables.Count;
            if (count <= 0)
                return;

            var graphics = context.graphics;
            var dominantMaterial = Material;
            var defaultMaterial = DefaultMaterial ?? (DefaultMaterial = new BasicMaterial(graphics) 
            {
                LightingEnabled = true, PreferPerPixelLighting = true,
            });

            try
            {
                // Begin Draw
                for (int i = 0; i < count; ++i)
                {
                    var drawable = drawables[i];

                    var material = dominantMaterial ?? drawable.Material ?? defaultMaterial;
                    if (MaterialUsage != MaterialUsage.Default)
                        material = material.GetMaterialByUsage(MaterialUsage);

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

                if (MaterialSortEnabled)
                {
                    opaque.SortByMaterial();
                    opaqueTwoSided.SortByMaterial();
                    
                    if (!TransparencySortEnabled)
                    {
                        transparent.SortByMaterial();
                        transparentTwoSided.SortByMaterial();
                    }
                }
                
                if (TransparencySortEnabled)
                {
                    transparent.SortByViewDistance(ref context.matrices.cameraPosition);
                    transparentTwoSided.SortByViewDistance(ref context.matrices.cameraPosition);
                }

                //---------------------------------------------------------------------
                // Sampler state management rules:
                //
                // - When possible, do not modify sampler states in HLSL.
                // - When a material modifies any sampler state, it should restore the 
                //   state to defaults.
                // - When a post processing material modifies sampler state 1-n, it should
                //   restore the state to defaults.
                // - When a post processing material modifies sampler state 0, no need to restore.
                // - A post processing material should always set sampler state 0.
                //
                // - All sampler states will be set to the default on application startup
                //   or the default state changed.
                // - During the drawing pass, the first sampler state is reset to default
                //   to correct the changes made in post processing.
                //---------------------------------------------------------------------
                graphics.SamplerStates[0] = context.settings.SamplerState;

                // Draw opaque objects     
                graphics.DepthStencilState = DepthStencilState.Default;
                graphics.BlendState = BlendState.Opaque;

                if (opaque.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                    for (int i = 0; i < opaque.Count; ++i)
                    {
                        var entry = opaque.Elements[i];
                        entry.Drawable.Draw(context, entry.Material);
                    }
                }

                if (opaqueTwoSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullNone;

                    for (int i = 0; i < opaqueTwoSided.Count; ++i)
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

                    for (int i = 0; i < transparent.Count; ++i)
                    {
                        var entry = transparent.Elements[i];
                        graphics.BlendState = entry.Material.IsAdditive ? BlendState.Additive : BlendState.AlphaBlend;
                        entry.Drawable.Draw(context, entry.Material);
                    }
                }

                graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                if (transparentTwoSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullNone;

                    for (int i = 0; i < transparentTwoSided.Count; ++i)
                    {
                        var entry = transparentTwoSided.Elements[i];
                        graphics.BlendState = entry.Material.IsAdditive ? BlendState.Additive : BlendState.AlphaBlend;
                        entry.Drawable.Draw(context, entry.Material);
                    }
                }
            }
            finally
            {
                opaque.Clear();
                opaqueTwoSided.Clear();
                transparent.Clear();
                transparentTwoSided.Clear();

                // Counter clock wise culling is the default rasterizer state. Restore it afer the drawing pass.
                graphics.RasterizerState = RasterizerState.CullCounterClockwise;
            }
        }
    }
}