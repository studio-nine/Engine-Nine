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

namespace Nine.Graphics.ObjectModel
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
        /// Gets or sets whether high dynamic range lighting technique is used.
        /// </summary>
        public bool PreferHighDynamicRangeLighting { get; set; }

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
        /// Gets the debug settings.
        /// </summary>
        public GraphicsDebugSetting Debug { get; private set; }

        /// <summary>
        /// Initializes a new instance of <c>GraphicsSettings</c>.
        /// </summary>
        public GraphicsSettings()
        {
            LightEnabled = true;
            ShadowEnabled = true;
            ScreenEffectEnabled = true;
            PreferDeferredLighting = true;
            PreferHighDynamicRangeLighting = true;
            Debug = new GraphicsDebugSetting();
        }
    }

    /// <summary>
    /// Defines commonly used settings when debugging the renderer.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class GraphicsDebugSetting
    {
        public bool ShowBoundingBox { get; set; }
        public bool ShowLightFrustum { get; set; }
        public bool ShowWireframe { get; set; }
        public bool ShowSceneManager { get; set; }

        public bool ShowDepthBuffer { get; set; }
        public bool ShowNormalBuffer { get; set; }

        public Color BoundingBoxColor { get; set; }
        public Color LightFrustumColor { get; set; }
        public Color SceneManagerColor { get; set; }

        internal GraphicsDebugSetting()
        {
            BoundingBoxColor = Color.Pink;
            LightFrustumColor = Color.Yellow;
            SceneManagerColor = Color.White;
        }
    }
}