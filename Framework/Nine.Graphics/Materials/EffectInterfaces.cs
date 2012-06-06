#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Materials
{
#if !WINDOWS_PHONE

    /// <summary>
    /// Defines an interface for effects that supports shadow mapping.
    /// </summary>
    public interface IEffectShadowMap
    {
        /// <summary>
        /// Gets or sets a small depth bias value that is added to the shadow map depth
        /// when comparing the object depth with depth in the shadow map.
        /// </summary>
        float DepthBias { get; set; }

        /// <summary>
        /// Gets or sets the color of the shadow.
        /// </summary>
        Vector3 ShadowColor { get; set; }

        /// <summary>
        /// Gets or sets the view projection matrix of the light that casts the shadow.
        /// </summary>
        Matrix LightViewProjection { get; set; }

        /// <summary>
        /// Gets or sets the shadow map texture that holds the depth values in x(r) channel.
        /// </summary>
        Texture2D ShadowMap { get; set; }
    }

    /// <summary>
    /// Defines an interface for effects that supports arbitrary texture transform.
    /// </summary>
    public interface IEffectTextureTransform
    {
        /// <summary>
        /// Gets or sets the texture transform matrix.
        /// </summary>
        /// <see cref="T:Nine.Graphics.TextureTransform"/>
        Matrix TextureTransform { get; set; }
    }

    /// <summary>
    /// Defines an interface for effects that supports arbitrary color transform.
    /// </summary>
    public interface IEffectColorMatrix
    {
        /// <summary>
        /// Gets or sets the color transform matrix.
        /// </summary>
        /// <see cref="T:Nine.Graphics.ColorMatrix"/>
        Matrix ColorMatrix { get; set; }
    }

#endif
}