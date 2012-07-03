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
        private FastList<BlendState> blendStates = new FastList<BlendState>();
        private FastList<Pass> workingPasses = new FastList<Pass>();
        private PostEffect basicPostEffect;
        
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
            RenderTargetPool.Lock(InputTexture as RenderTarget2D);

            for (int p = 0; p < passes.Count; p++)
            {
                var pass = passes[p];
                if (!pass.Enabled)
                    continue;

                pass.GetActivePasses(workingPasses);

                RenderTarget2D intermediate = InputTexture as RenderTarget2D;
                for (int i = 0; i < workingPasses.Count - 1; i++)
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
                        basicPostEffect = new PostEffect(context.GraphicsDevice);
                        basicPostEffect.Material = new BasicMaterial(context.GraphicsDevice);
                    }
                    lastEffect = basicPostEffect;
                }

                blendStates.Add(pass.BlendState);
                lastEffects.Add(lastEffect);
                passResults.Add(intermediate);

                workingPasses.Clear();
            }

            RenderTargetPool.Unlock(InputTexture as RenderTarget2D);

            for (int p = 0; p < passResults.Count; p++)
            {
                lastEffects[p].BlendState = blendStates[p];
                lastEffects[p].InputTexture = passResults[p];
                lastEffects[p].Draw(context, drawables, startIndex, length);

                RenderTargetPool.Unlock(passResults[p]);
            }

            lastEffects.Clear();
            passResults.Clear();
            blendStates.Clear();
        }
    }
}

