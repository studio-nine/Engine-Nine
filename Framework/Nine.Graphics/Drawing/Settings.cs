namespace Nine.Graphics.Drawing
{
    using System;
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Microsoft.Xna.Framework.Input;
#if SILVERLIGHT
    using Keys = System.Windows.Input.Key;
#endif

    [Serializable]
    public class Settings
    {
        /// <summary>
        /// Gets or sets the color of the background.
        /// </summary>
        public Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets whether shadows are enabled.
        /// </summary>
        public bool ShadowEnabled { get; set; }

        /// <summary>
        /// Gets or sets whether lights are enabled.
        /// </summary>
        public bool LightingEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether fog is enabled.
        /// </summary>
        public bool FogEnable { get; set; }

        /// <summary>
        /// Gets or sets post processing effects are enabled.
        /// </summary>
        public bool PostEffectEnabled { get; set; }

        /// <summary>
        /// Gets or sets preferred shadowmap resolution.
        /// </summary>
        public int ShadowMapResolution { get; set; }
        
        /// <summary>
        /// Gets or sets the default font.
        /// </summary>
        public SpriteFont DefaultFont { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether default debug control is enabled.
        /// </summary>
        public bool DefaultDebugControlEnabled { get; set; }

        /// <summary>
        /// Gets or sets the overal material quanlity for each leveled material in the scene.
        /// </summary>
        public float MaterialQuality { get; set; }

        /// <summary>
        /// Gets or sets the texture filter quanlity for this drawing pass.
        /// </summary>
        public TextureFilter TextureFilter
        {
            get { return textureFilter; }
            set
            {
                if (textureFilter != value)
                {
                    textureFilter = value;
                    samplerStateNeedsUpdate = true;
                }
            }
        }
        private TextureFilter textureFilter = TextureFilter.Linear;

        /// <summary>
        /// Gets or sets the maximum anisotropy. The default value is 4.
        /// </summary>
        public int MaxAnisotropy
        {
            get { return maxAnisotropy; }
            set
            {
                if (maxAnisotropy != value)
                {
                    maxAnisotropy = value;
                    samplerStateNeedsUpdate = true;
                }
            }
        }
        private int maxAnisotropy = 4;

        internal bool DefaultSamplerStateChanged = true;

        private bool samplerStateNeedsUpdate = false;
        private SamplerState samplerState = SamplerState.LinearWrap;

        /// <summary>
        /// Gets the default sampler state.
        /// </summary>
        public SamplerState DefaultSamplerState
        {
            get
            {
                if (samplerStateNeedsUpdate)
                {
                    samplerStateNeedsUpdate = false;
                    if (maxAnisotropy == 4)
                    {
                        if (textureFilter == TextureFilter.Linear)
                            samplerState = SamplerState.LinearWrap;
                        else if (textureFilter == TextureFilter.Point)
                            samplerState = SamplerState.PointWrap;
                        else if (textureFilter == TextureFilter.Anisotropic)
                            samplerState = SamplerState.AnisotropicWrap;
                        else
                            samplerStateNeedsUpdate = true;
                    }
                    else
                    {
                        samplerStateNeedsUpdate = true;
                    }

                    if (samplerStateNeedsUpdate)
                    {
                        samplerState = new SamplerState();
                        samplerState.AddressU = TextureAddressMode.Wrap;
                        samplerState.AddressV = TextureAddressMode.Wrap;
                        samplerState.AddressW = TextureAddressMode.Wrap;
                        samplerState.Filter = textureFilter;
                        samplerState.MaxAnisotropy = maxAnisotropy;
                        samplerStateNeedsUpdate = false;
                    }
                }
                return samplerState;
            }
        }

        /// <summary>
        /// Gets the debug settings.
        /// </summary>
        public GraphicsDebugSetting Debug { get; private set; }

        /// <summary>
        /// Initializes a new instance of <c>GraphicsSettings</c>.
        /// </summary>
        public Settings()
        {
            FogEnable = true;
            LightingEnabled = true;
            ShadowEnabled = true;
            PostEffectEnabled = true;
            ShadowMapResolution = 1024;
            MaterialQuality = 1;
            BackgroundColor = Color.Black;
            Debug = new GraphicsDebugSetting();
        }

        internal void Update()
        {
            if (DefaultDebugControlEnabled)
            {
                var keyboardState = Keyboard.GetState();

                ShadowEnabled = !keyboardState.IsKeyDown(Keys.F2);
                LightingEnabled = !keyboardState.IsKeyDown(Keys.F3);
                FogEnable = !keyboardState.IsKeyDown(Keys.F4);
                PostEffectEnabled = !keyboardState.IsKeyDown(Keys.F5);

                Debug.ShowWireframe = keyboardState.IsKeyDown(Keys.D1);
                Debug.ShowBoundingBox = keyboardState.IsKeyDown(Keys.D2);
                Debug.ShowLightFrustum = keyboardState.IsKeyDown(Keys.D3);
                Debug.ShowSceneManager = keyboardState.IsKeyDown(Keys.D4);
                Debug.ShowShadowMap = keyboardState.IsKeyDown(Keys.D5);
                Debug.ShowStatistics = keyboardState.IsKeyDown(Keys.D6);
                Debug.ShowDepthBuffer = keyboardState.IsKeyDown(Keys.D7);
                Debug.ShowNormalBuffer = keyboardState.IsKeyDown(Keys.D8);
                Debug.ShowLightBuffer = keyboardState.IsKeyDown(Keys.D9);
            }
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