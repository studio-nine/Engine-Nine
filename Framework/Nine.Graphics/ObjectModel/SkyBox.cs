#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Effects;
using Nine.Graphics.Primitives;
#endregion

namespace Nine.Graphics.ObjectModel
{
    public class SkyBox : Drawable
    {
        /// <summary>
        /// Gets or sets the skybox texture.
        /// </summary>
        [ContentSerializer]
        public TextureCube Texture { get; internal set; }

        private Effect effect;

        public override void Draw(GraphicsContext context)
        {
#if !WINDOWS_PHONE
            // Keep track of the effect to avoid the effect been constantly garbage collected
            // since graphics resources uses weak references now.
            effect = effect ?? GraphicsResources<SkyBoxEffect>.GetInstance(context.GraphicsDevice);

            context.ModelBatch.DrawSkyBox(Texture);
#endif
        }
    }
}