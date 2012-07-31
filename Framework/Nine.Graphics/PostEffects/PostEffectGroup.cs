namespace Nine.Graphics.PostEffects
{
    using System;
    using System.Collections.Generic;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Content;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.Materials;
    using Nine.Graphics.ObjectModel;

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
        public virtual IList<PostEffectChain> Passes
        {
            get { return passes; }
        }
        private List<PostEffectChain> passes = new List<PostEffectChain>();

        /// <summary>
        /// Gets or sets the input texture to be processed.
        /// </summary>
        [ContentSerializerIgnore]
        public Texture2D InputTexture { get; set; }

        /// <summary>
        /// Gets the preferred surface format for the input texture.
        /// </summary>
        public virtual SurfaceFormat? InputFormat 
        {
            get { return null; } 
        }

        /// <summary>
        /// Gets or sets the material to combine the composite the result
        /// of each contained pass.
        /// </summary>
        public virtual Material Material { get; set; }

        private FastList<RenderTarget2D> passResults = new FastList<RenderTarget2D>();
        private FastList<PostEffect> lastEffects = new FastList<PostEffect>();
        private FastList<Pass> workingPasses = new FastList<Pass>();
        private PostEffect basicPostEffect;

        private FullScreenQuad fullScreenQuad;
        
        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        public PostEffectGroup()
        {

        }

        /// <summary>
        /// Gets all the pass types that are required by this pass.
        /// </summary>
        protected internal override void GetDependentPasses(ICollection<Type> passTypes)
        {
            var count = passes.Count;
            for (int i = 0; i < count; i++)
            {
                var pass = passes[i];
                if (pass.Enabled)
                    pass.GetDependentPasses(passTypes);
            }
            if (Material != null)
                Material.GetDependentPasses(passTypes);
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
            AddDependency(context.MainPass);
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
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            if (Material == null)
                DrawWithoutCombineMaterial(context, drawables);
            else
                DrawWithCombineMaterial(context, drawables);
        }

        private void DrawWithCombineMaterial(DrawingContext context, IList<IDrawableObject> drawables)
        {
            RenderTargetPool.Lock(InputTexture as RenderTarget2D);

            int i, p;
            for (p = 0; p < passes.Count; p++)
            {
                passes[p].GetActivePasses(workingPasses);

                RenderTarget2D intermediate = InputTexture as RenderTarget2D;
                for (i = 0; i < workingPasses.Count; i++)
                {
                    var workingPass = (PostEffect)workingPasses[i];

                    RenderTarget2D previous = intermediate;
                    RenderTargetPool.Lock(previous);
                    intermediate = workingPass.PrepareRenderTarget(context, intermediate, null);
                    intermediate.Begin();

                    workingPass.InputTexture = previous;
                    workingPass.Draw(context, drawables);

                    intermediate.End();
                    RenderTargetPool.Unlock(previous);
                }

                passResults.Add(intermediate);
                workingPasses.Clear();
            }

            RenderTargetPool.Unlock(InputTexture as RenderTarget2D);

            if (fullScreenQuad == null)
                fullScreenQuad = new FullScreenQuad(context.GraphicsDevice);

            context.GraphicsDevice.BlendState = BlendState.Opaque;
            Material.texture = InputTexture;

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

        private void DrawWithoutCombineMaterial(DrawingContext context, IList<IDrawableObject> drawables)
        {
            RenderTargetPool.Lock(InputTexture as RenderTarget2D);

            int i, p;
            for (p = 0; p < passes.Count; p++)
            {
                passes[p].GetActivePasses(workingPasses);

                RenderTarget2D intermediate = InputTexture as RenderTarget2D;
                for (i = 0; i < workingPasses.Count - 1; i++)
                {
                    var workingPass = (PostEffect)workingPasses[i];

                    RenderTarget2D previous = intermediate;
                    RenderTargetPool.Lock(previous);
                    intermediate = workingPass.PrepareRenderTarget(context, intermediate, null);
                    intermediate.Begin();

                    workingPass.InputTexture = previous;
                    workingPass.Draw(context, drawables);

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
                    lastEffects[i].Draw(context, drawables);

                    RenderTargetPool.Unlock(passResults[i]);
                    i++;
                }
            }

            lastEffects.Clear();
            passResults.Clear();
        }
    }
}

