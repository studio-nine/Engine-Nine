#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics
{
    /// <summary>
    /// Defines commonly used settings when drawing using the renderer.
    /// </summary>
    public class GraphicsSettings
    {
        /// <summary>
        /// Gets or sets whether deferred lighting technique is used.
        /// </summary>
        public bool PreferDeferredLighting { get; set; }

        /// <summary>
        /// Gets or sets whether shadows are enabled.
        /// </summary>
        public bool ShadowEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether lights are enabled.
        /// </summary>
        public bool LightEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether lights are enabled.
        /// </summary>
        public bool ScreenEffectEnabled { get; set; }

        /// <summary>
        /// Initializes a new instance of <c>GraphicsSettings</c>.
        /// </summary>
        public GraphicsSettings()
        {
            LightEnabled = true;
            ShadowEnabled = true;
            ScreenEffectEnabled = true;
            PreferDeferredLighting = true;
        }
    }
}