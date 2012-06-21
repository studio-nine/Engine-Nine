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
    [ContentProperty("PostEffects")]
    public class PostEffectGroup : DrawingPassGroup, ISceneObject
    {
        /// <summary>
        /// Gets the materials used to draw this post effect.
        /// </summary>
        public IList<PostEffect> PostEffects
        {
            get { return postEffects; }
        }
        private List<PostEffect> postEffects = new List<PostEffect>();

        /// <summary>
        /// Gets or sets the material to combine the result of each post effect.
        /// </summary>
        public Material Material { get; set; }

        /// <summary>
        /// Gets or sets the state of the blend of this post effect.
        /// </summary>
        public BlendState BlendState { get; set; }
        
        /// <summary>
        /// Creates a new instance of ScreenEffect for post processing.
        /// </summary>
        public PostEffectGroup()
        {
            BlendState = BlendState.Opaque;
        }

        /// <summary>
        /// Draws this pass using the specified drawing context.
        /// </summary>
        public override void Draw(DrawingContext context, IDrawableObject[] drawables, int startIndex, int length)
        {

        }

        /// <summary>
        /// Called when this scene object is added to the scene.
        /// </summary>
        void ISceneObject.OnAdded(DrawingContext context)
        {
            context.MainPass.Passes.Add(this);
        }

        /// <summary>
        /// Called when this scene object is removed from the scene.
        /// </summary>
        void ISceneObject.OnRemoved(DrawingContext context)
        {
            context.MainPass.Passes.Remove(this);
        }
    }
}

