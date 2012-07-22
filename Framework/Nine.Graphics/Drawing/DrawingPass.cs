namespace Nine.Graphics.Drawing
{
    using System.Collections.Generic;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Materials;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// A drawing pass represents a single pass in the composition chain.
    /// </summary>
    public class DrawingPass : Pass
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
        /// Gets or sets the texture filter quanlity for this drawing pass.
        /// </summary>
        public TextureFilter TextureFilter
        {
            get { return textureFilter; }
            set 
            {
                if (textureFilter != value)
                {
                    textureFilter = value;
                    samplerStateNeedsUpdate = true;
                }
            }
        }
        private TextureFilter textureFilter = TextureFilter.Linear;
     
        /// <summary>
        /// Gets or sets the maximum anisotropy. The default value is 4.
        /// </summary>
        public int MaxAnisotropy
        {
            get { return maxAnisotropy; }
            set
            {
                if (maxAnisotropy != value)
                {
                    maxAnisotropy = value;
                    samplerStateNeedsUpdate = true;
                }
            }
        }
        private int maxAnisotropy = 4;

        private bool samplerStateNeedsUpdate = false;
        private SamplerState samplerState = SamplerState.LinearWrap;


        private DrawingQueue opaque = new DrawingQueue();
        private DrawingQueue opaqueTwoSided = new DrawingQueue();

        private DrawingQueue transparent = new DrawingQueue();
        private DrawingQueue transparentTwoSided = new DrawingQueue();

        /// <summary>
        /// Initializes a new instance of the <see cref="Pass"/> class.
        /// </summary>
        public DrawingPass()
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
            var defaultMaterial = DefaultMaterial ?? (DefaultMaterial = new BasicMaterial(graphics) { LightingEnabled = true });

            UpdateSamplerState(graphics);

            try
            {
                // Begin Draw
                for (int i = 0; i < count; i++)
                {
                    var drawable = drawables[i];
                    if (drawable != null && drawable.Visible)
                    {
                        drawable.BeginDraw(context);

                        var material = dominantMaterial ?? drawable.Material ?? defaultMaterial;
                        if (MaterialUsage != MaterialUsage.Default)
                            material = material[MaterialUsage];

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
                        graphics.BlendState = entry.Material.IsAdditive ? BlendState.Additive : BlendState.AlphaBlend;
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

                // End Draw
                for (int i = 0; i < count; i++)
                    drawables[i].EndDraw(context);
            }
        }

        private void UpdateSamplerState(GraphicsDevice graphics)
        {
            if (samplerStateNeedsUpdate)
            {
                samplerState = new SamplerState();
                samplerState.AddressU = TextureAddressMode.Wrap;
                samplerState.AddressV = TextureAddressMode.Wrap;
                samplerState.AddressW = TextureAddressMode.Wrap;
                samplerState.Filter = textureFilter;
                samplerState.MaxAnisotropy = maxAnisotropy;
                samplerStateNeedsUpdate = false;
            }
            graphics.SamplerStates[0] = samplerState;
        }
    }
}