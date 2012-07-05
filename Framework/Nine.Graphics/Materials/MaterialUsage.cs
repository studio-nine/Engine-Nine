#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.ObjectModel;
#endregion

namespace Nine.Graphics.Materials
{
    /// <summary>
    /// Defines commonly used material usages.
    /// </summary>
    public enum MaterialUsage
    {
        /// <summary>
        /// Default material usage.
        /// </summary>
        Default,

        /// <summary>
        /// The material is used to generate a shadowmap.
        /// </summary>
        ShadowMap,

        /// <summary>
        /// The material is used to generate a depth map.
        /// </summary>
        Depth,

        /// <summary>
        /// The material is used to generate a normal map.
        /// </summary>
        Normal,

        /// <summary>
        /// The material is used to generate the graphics buffer used in deferred lighting.
        /// </summary>
        DepthAndNormal,

        /// <summary>
        /// The material is used to draw multipass directional lights.
        /// </summary>
        DirectionalLight,
        
        /// <summary>
        /// The material is used to draw multipass point lights.
        /// </summary>        
        PointLight,

        /// <summary>
        /// The material is used to draw multipass spot lights.
        /// </summary>
        SpotLight,
    }
}