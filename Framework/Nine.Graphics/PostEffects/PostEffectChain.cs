﻿namespace Nine.Graphics.PostEffects
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Markup;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Design;
    using Nine.Graphics.Drawing;
    using Nine.Graphics.ObjectModel;

    /// <summary>
    /// Represents post processing effects.
    /// </summary>
    [ContentSerializable]
    [ContentProperty("Effects")]
    public class PostEffectChain : Pass, ISceneObject
    {
        /// <summary>
        /// Gets a list of post processing effects contained by this chain.
        /// </summary>
        public virtual IList<PostEffect> Effects
        {
            get { return effects; }
        }
        private List<PostEffect> effects = new List<PostEffect>();

        /// <summary>
        /// Gets or sets the texture usage of the result of this chain.
        /// </summary>
        public TextureUsage TextureUsage { get; set; }

        /// <summary>
        /// Gets or sets the state of the blend of this post effect.
        /// </summary>
        [TypeConverter(typeof(BlendStateConverter))]
        public BlendState BlendState { get; set; }
        
        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        public PostEffectChain()
        {
            BlendState = BlendState.Opaque;
        }

        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        public PostEffectChain(BlendState blendState, params PostEffect[] effects)
        {
            this.BlendState = blendState;
            this.effects.AddRange(effects);
        }

        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        public PostEffectChain(TextureUsage textureUsage, params PostEffect[] effects)
        {
            this.BlendState = BlendState.Opaque;
            this.TextureUsage = textureUsage;
            this.effects.AddRange(effects);
        }

        /// <summary>
        /// Gets all the passes that are going to be rendered.
        /// </summary>
        public override void GetActivePasses(IList<Pass> result)
        {
            if (Enabled)
            {
                for (int i = 0; i < effects.Count; i++)
                    if (effects[i].Enabled)
                        effects[i].GetActivePasses(result);

            }
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
        public override void Draw(DrawingContext context, IList<IDrawableObject> drawables)
        {
            throw new NotSupportedException();
        }
    }
}
