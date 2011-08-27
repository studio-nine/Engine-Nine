#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.ObjectModel;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines commonly used texture usage.
    /// </summary>
    public enum TextureUsage
    {
        /// <summary>
        /// Default texture usage.
        /// </summary>
        None,

        /// <summary>
        /// Specifies the target texture is used as diffuse texture.
        /// </summary>
        Diffuse,

        /// <summary>
        /// Specifies the target texture is used as ambient texture.
        /// </summary>
        Ambient,

        /// <summary>
        /// Specifies the target texture is used as emissive texture.
        /// </summary>
        Emissive,

        /// <summary>
        /// Specifies the target texture is used as specular texture.
        /// </summary>
        Specular,

        /// <summary>
        /// Specifies the target texture is used as detail texture.
        /// </summary>
        Detail,

        /// <summary>
        /// Specifies the target texture is used as overlay texture.
        /// </summary>
        Overlay,

        /// <summary>
        /// Specifies the target texture is used as dual texture.
        /// </summary>
        Dual,

        /// <summary>
        /// Specifies the target texture is used as reflection texture.
        /// </summary>
        Reflection,

        /// <summary>
        /// Specifies the target texture is used as refraction texture.
        /// </summary>
        Refraction,

        /// <summary>
        /// Specifies the target texture is used as lightmap texture.
        /// </summary>
        Lightmap,

        /// <summary>
        /// Specifies the target texture is used as lightmap texture.
        /// </summary>
        DarkMap,

        /// <summary>
        /// Specifies the target texture is used as luminance texture.
        /// </summary>
        Luminance,

        /// <summary>
        /// Specifies the target texture is used as bloom texture.
        /// </summary>
        Bloom,

        /// <summary>
        /// Specifies the target texture is used as blur texture.
        /// </summary>
        Blur,

        /// <summary>
        /// Specifies the target texture is used as shadowmap texture.
        /// </summary>
        ShadowMap,

        /// <summary>
        /// Specifies the target texture is used as normalmap texture.
        /// </summary>
        NormalMap,

        /// <summary>
        /// Specifies the target texture is used as heightmap texture.
        /// </summary>
        Heightmap,

        /// <summary>
        /// Specifies the target texture is used as bump texture.
        /// </summary>
        BumpMap,

        /// <summary>
        /// Specifies the target texture is used as environment texture.
        /// </summary>
        EnvironmentMap,

        /// <summary>
        /// Specifies the target texture is used as depth buffer texture.
        /// </summary>
        DepthBuffer,

        /// <summary>
        /// Specifies the target texture is used as light buffer texture.
        /// </summary>
        LightBuffer,

        /// <summary>
        /// Specifies the target texture is used as decal texture.
        /// </summary>
        Decal,
    }
}