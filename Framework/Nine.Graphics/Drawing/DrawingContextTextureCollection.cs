#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.ComponentModel;
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
    /// Contains commonly used textures in a drawing context.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class DrawingContextTextureCollection
    {
        /// <summary>
        /// Gets a value indicating the maximum number of textures supported.
        /// </summary>
        public const int MaxTextureSlots = 64;

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawingContextTextureCollection"/> class.
        /// </summary>
        internal DrawingContextTextureCollection() { }

        /// <summary>
        /// Gets or sets the global texture with the specified texture usage.
        /// </summary>
        public Texture this[TextureUsage textureUsage]
        {
            get { return textureSlots[(int)textureUsage]; }
            set { textureSlots[(int)textureUsage] = value; }
        }
        private Texture[] textureSlots = new Texture[MaxTextureSlots];
    }
}