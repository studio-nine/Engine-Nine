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
    public class BasicDrawingPass : DrawingPass
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

        private FastList<int> opaqueOneSided = new FastList<int>();
        private FastList<int> opaqueTwoSided = new FastList<int>();
        private FastList<int> alphaBlendOneSided = new FastList<int>();
        private FastList<int> alphaBlendTwoSided = new FastList<int>();
        private FastList<int> additiveOneSided = new FastList<int>();
        private FastList<int> additiveTwoSided = new FastList<int>();
        
        private static Material[] materials = new Material[4];

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingPass"/> class.
        /// </summary>
        public BasicDrawingPass()
        {
            this.SortEnabled = false;
            this.sortComparer = new DrawableSortComparer();
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        /// <param name="drawables">
        /// A list of drawables about to be drawed in this drawing pass.
        /// </param>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            if (length <= 0)
                return;

            var graphics = context.GraphicsDevice;
            var dominantMaterial = Material;
            var endIndex = startIndex + length - 1;
            
            if (dominantMaterial != null)
            {
                // Draw with uniform material
                for (int i = startIndex; i <= endIndex; i++)
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
                if (materials.Length < drawables.Length)
                    Array.Resize(ref materials, drawables.Length);

                for (int i = startIndex; i <= endIndex;)
                {
                    var drawable = drawables[i];
                    if (drawable == null || !drawable.Visible)
                    {
                        // Swap with the last element
                        if (i != endIndex)
                        {
                            var temp = drawables[endIndex];
                            drawables[endIndex] = drawables[i];
                            drawables[i] = temp;
                        }
                        endIndex--;
                    }
                    else
                    {
                        drawable.BeginDraw(context);
                        Material material = drawable.Material;
                        if (MaterialUsage != MaterialUsage.Default && material != null)
                            material = material[MaterialUsage];
                        materials[i++] = material;
                    }
                }

                // Sort drawable objects based on material.
                // TODO: Disable sorting when drawing with multiple passes.
                if (SortEnabled)
                    Array.Sort(drawables, startIndex, length, sortComparer);

                for (int i = startIndex; i <= endIndex; i++)
                {
                    var drawable = drawables[i];
                    var material = materials[i];

                    if (material == null)
                    {
                        materials[i] = DefaultMaterial;
                        opaqueOneSided.Add(i);
                        continue;
                    }

                    var twoSided = material.TwoSided;
                    if (!material.IsTransparent)
                    {
                        if (twoSided)
                            opaqueTwoSided.Add(i);
                        else
                            opaqueOneSided.Add(i);
                    }
                    else if (material.IsAdditive)
                    {
                        if (twoSided)
                            additiveTwoSided.Add(i);
                        else
                            additiveOneSided.Add(i);
                    }
                    else
                    {
                        if (twoSided)
                            alphaBlendTwoSided.Add(i);
                        else
                            alphaBlendOneSided.Add(i);
                    }
                }
                
                // Draw opaque objects                
                graphics.DepthStencilState = DepthStencilState.Default;
                graphics.BlendState = BlendState.Opaque;

                if (opaqueTwoSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullNone;

                    for (int i = 0; i < opaqueTwoSided.Count; i++)
                    {
                        var index = opaqueTwoSided[i];
                        var drawable = drawables[index];
                        drawable.Draw(context, materials[index]);
                    }                    
                }
                if (opaqueOneSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                    for (int i = 0; i < opaqueOneSided.Count; i++)
                    {
                        var index = opaqueOneSided[i];
                        var drawable = drawables[index];
                        drawable.Draw(context, materials[index]);
                    }                    
                }

                // Draw transparent object backfaces using alpha blending
                graphics.DepthStencilState = DepthStencilState.DepthRead;
                graphics.BlendState = BlendState.AlphaBlend;

                if (alphaBlendTwoSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullClockwise;

                    for (int i = 0; i < alphaBlendTwoSided.Count; i++)
                    {
                        var index = alphaBlendTwoSided[i];
                        var drawable = drawables[index];
                        drawable.Draw(context, materials[index]);
                    }
                }

                // Draw transparent object front faces using alpha blending
                graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                for (int i = 0; i < alphaBlendTwoSided.Count; i++)
                {
                    var index = alphaBlendTwoSided[i];
                    var drawable = drawables[index];
                    drawable.Draw(context, materials[index]);
                }
                for (int i = 0; i < alphaBlendOneSided.Count; i++)
                {
                    var index = alphaBlendOneSided[i];
                    var drawable = drawables[index];
                    drawable.Draw(context, materials[index]);
                }

                // Draw transparent object backfaces using additive blending
                graphics.BlendState = BlendState.Additive;

                if (additiveTwoSided.Count > 0)
                {
                    graphics.RasterizerState = RasterizerState.CullClockwise;

                    for (int i = 0; i < additiveTwoSided.Count; i++)
                    {
                        var index = additiveTwoSided[i];
                        var drawable = drawables[index];
                        drawable.Draw(context, materials[index]);
                    }
                }

                // Draw transparent object front faces using additive blending
                graphics.RasterizerState = RasterizerState.CullCounterClockwise;

                for (int i = 0; i < additiveTwoSided.Count; i++)
                {
                    var index = additiveTwoSided[i];
                    var drawable = drawables[index];
                    drawable.Draw(context, materials[index]);
                }
                for (int i = 0; i < additiveOneSided.Count; i++)
                {
                    var index = additiveOneSided[i];
                    var drawable = drawables[index];
                    drawable.Draw(context, materials[index]);
                }

                opaqueOneSided.Clear();
                opaqueTwoSided.Clear();
                alphaBlendOneSided.Clear();
                alphaBlendTwoSided.Clear();
                additiveOneSided.Clear();
                additiveTwoSided.Clear();

                // End Draw
                for (int i = startIndex; i <= endIndex; i++)
                    drawables[i].EndDraw(context);
            }
        }
    }

    class DrawableSortComparer : IComparer<IDrawableObject>
    {
        public int Compare(IDrawableObject x, IDrawableObject y)
        {
            var materialX = x.Material;
            var materialY = y.Material;

            if (materialX == materialY)
                return 0;

            var materialSortOrderX = materialX != null ? materialX.SortOrder : 0;
            var materialSortOrderY = materialY != null ? materialY.SortOrder : 0;

            return materialSortOrderX.CompareTo(materialSortOrderY);
        }
    }
}