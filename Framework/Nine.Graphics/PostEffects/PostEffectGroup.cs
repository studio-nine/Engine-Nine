#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
using Nine.Graphics.ObjectModel;
using Nine.Graphics.Materials;
using System.Windows.Markup;
#endregion

namespace Nine.Graphics.PostEffects
{
    /// <summary>
    /// Represents post processing effects.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("Passes")]
    public class PostEffectGroup : Pass, ISceneObject, IPostEffect
    {
        /// <summary>
        /// Gets a list of post processing chains contained by this group.
        /// </summary>
        public IList<PostEffectChain> Passes
        {
            get { return passes; }
        }
        private List<PostEffectChain> passes = new List<PostEffectChain>();

        /// <summary>
        /// Gets or sets the input texture to be processed.
        /// </summary>
        public Texture2D InputTexture { get; set; }

        /// <summary>
        /// Gets or sets the material to combine the composite the result
        /// of each contained pass.
        /// </summary>
        public Material Material { get; set; }

        private FastList<RenderTarget2D> passResults = new FastList<RenderTarget2D>();
        private FastList<PostEffect> lastEffects = new FastList<PostEffect>();
        private FastList<Pass> workingPasses = new FastList<Pass>();
        private PostEffect basicPostEffect;

        private Material vertexPassThrough;
        private FullScreenQuad fullScreenQuad;
        
        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        public PostEffectGroup()
        {

        }

        /// <summary>
        /// Gets all the passes that are going to be rendered.
        /// </summary>
        public override void GetActivePasses(IList<Pass> result)
        {
            if (Enabled && passes.Count > 0)
                result.Add(this);
        }

        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        void ISceneObject.OnAdded(DrawingContext context)
        {
            context.RootPass.Passes.Add(this);
        }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void ISceneObject.OnRemoved(DrawingContext context)
        {
            context.RootPass.Passes.Remove(this);
        }
        
        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            if (Material == null)
                DrawWithoutCombineMaterial(context, drawables, startIndex, length);
            else
                DrawWithCombineMaterial(context, drawables, startIndex, length);
        }

        private void DrawWithCombineMaterial(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            RenderTargetPool.Lock(InputTexture as RenderTarget2D);

            int i, p;
            for (p = 0; p < passes.Count; p++)
            {
                var pass = passes[p];
                if (!pass.Enabled)
                    continue;

                pass.GetActivePasses(workingPasses);

                RenderTarget2D intermediate = InputTexture as RenderTarget2D;
                for (i = 0; i < workingPasses.Count; i++)
                {
                    var workingPass = (PostEffect)workingPasses[i];

                    RenderTarget2D previous = intermediate;
                    RenderTargetPool.Lock(previous);
                    intermediate = workingPass.PrepareRenderTarget(context, intermediate);
                    intermediate.Begin();

                    workingPass.InputTexture = previous;
                    workingPass.Draw(context, drawables, startIndex, length);

                    intermediate.End();
                    RenderTargetPool.Unlock(previous);
                }

                passResults.Add(intermediate);
                workingPasses.Clear();
            }

            RenderTargetPool.Unlock(InputTexture as RenderTarget2D);

            if (fullScreenQuad == null)
            {
                fullScreenQuad = new FullScreenQuad(context.GraphicsDevice);
                vertexPassThrough = GraphicsResources<VertexPassThroughMaterial>.GetInstance(context.GraphicsDevice);
            }

            vertexPassThrough.BeginApply(context);
            vertexPassThrough.EndApply(context);

            context.GraphicsDevice.BlendState = BlendState.Opaque;
            context.GraphicsDevice.Textures[0] = InputTexture;

            Material.Texture = InputTexture;

            for (i = 0, p = 0; p < passes.Count; p++)
            {
                if (passes[p].Enabled)
                {
                    Material.SetTexture(passes[p].TextureUsage, passResults[i]);
                    i++;
                }
            }

            fullScreenQuad.Draw(context, Material);

            lastEffects.Clear();
            passResults.Clear();
        }

        private void DrawWithoutCombineMaterial(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {
            RenderTargetPool.Lock(InputTexture as RenderTarget2D);

            int i, p;
            for (p = 0; p < passes.Count; p++)
            {
                var pass = passes[p];
                if (!pass.Enabled)
                    continue;

                pass.GetActivePasses(workingPasses);

                RenderTarget2D intermediate = InputTexture as RenderTarget2D;
                for (i = 0; i < workingPasses.Count - 1; i++)
                {
                    var workingPass = (PostEffect)workingPasses[i];

                    RenderTarget2D previous = intermediate;
                    RenderTargetPool.Lock(previous);
                    intermediate = workingPass.PrepareRenderTarget(context, intermediate);
                    intermediate.Begin();

                    workingPass.InputTexture = previous;
                    workingPass.Draw(context, drawables, startIndex, length);

                    intermediate.End();
                    RenderTargetPool.Unlock(previous);
                }
                
                PostEffect lastEffect;
                RenderTargetPool.Lock(intermediate);
                
                if (workingPasses.Count > 0)
                {
                    lastEffect = (PostEffect)workingPasses[workingPasses.Count - 1];
                }
                else
                {
                    if (basicPostEffect == null)
                    {
                        basicPostEffect = new PostEffect();
                        basicPostEffect.Material = new BasicMaterial(context.GraphicsDevice);
                    }
                    lastEffect = basicPostEffect;
                }

                lastEffects.Add(lastEffect);
                passResults.Add(intermediate);

                workingPasses.Clear();
            }

            RenderTargetPool.Unlock(InputTexture as RenderTarget2D);

            for (i = 0, p = 0; p < passes.Count; p++)
            {
                if (passes[p].Enabled)
                {
                    lastEffects[i].BlendState = passes[p].BlendState;
                    lastEffects[i].InputTexture = passResults[i];
                    lastEffects[i].Draw(context, drawables, startIndex, length);

                    RenderTargetPool.Unlock(passResults[i]);
                    i++;
                }
            }

            lastEffects.Clear();
            passResults.Clear();
        }
    }
}

