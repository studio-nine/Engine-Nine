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
        /// Gets or sets the color of the background.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets whether high dynamic range lighting technique is used.
        /// </summary>
        public bool PreferHighDynamicRangeLighting { get; set; }

        /// <summary>
        /// Gets or sets whether shadows are enabled.
        /// </summary>
        public bool ShadowEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether multi-pass shadow overlays are enabled.
        /// </summary>
        public bool MultiPassShadowEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether lights are enabled.
        /// </summary>
        public bool LightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether multi-pass light overlays are enabled.
        /// </summary>
        public bool MultiPassLightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fog is enabled.
        /// </summary>
        public bool FogEnable { get; set; }

        /// <summary>
        /// Gets or sets whether lights are enabled.
        /// </summary>
        public bool ScreenEffectEnabled { get; set; }

        /// <summary>
        /// Gets or sets preferred shadowmap resolution.
        /// </summary>
        public int ShadowMapResolution { get; set; }

        /// <summary>
        /// Gets or sets the depth bias for shadow map. The default value is 0.005f.
        /// </summary>
        public float ShadowMapDepthBias { get; set; }

        /// <summary>
        /// Gets or sets the default font.
        /// </summary>
        public SpriteFont DefaultFont { get; set; }

        /// <summary>
        /// Gets the debug settings.
        /// </summary>
        public GraphicsDebugSetting Debug { get; private set; }

        /// <summary>
        /// Initializes a new instance of <c>GraphicsSettings</c>.
        /// </summary>
        public GraphicsSettings()
        {
            FogEnable = true;
            LightingEnabled = true;
            ShadowEnabled = true;
            ScreenEffectEnabled = true;
            PreferHighDynamicRangeLighting = true;
            ShadowMapResolution = 1024;
            ShadowMapDepthBias = 0.005f;
            BackgroundColor = Color.Black;
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
        public bool ShowLightBuffer { get; set; }
        public bool ShowShadowMap { get; set; }

        public bool ShowStatistics { get; set; }

        public Color BoundingBoxColor { get; set; }
        public Color LightFrustumColor { get; set; }
        public Color ShadowFrustumColor { get; set; }
        public Color SceneManagerColor { get; set; }
        public Color StatisticsColor { get; set; }

        internal GraphicsDebugSetting()
        {
            BoundingBoxColor = new Color(255, 192, 203, 255);
            LightFrustumColor = new Color(255, 255, 0, 255);
            ShadowFrustumColor = new Color(70, 130, 180, 255);
            SceneManagerColor = new Color(255, 255, 255, 255);
            StatisticsColor = new Color(245, 245, 245, 255);
        }
    }
}