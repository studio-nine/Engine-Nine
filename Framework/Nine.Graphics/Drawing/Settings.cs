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
        /// Gets or sets a value indicating whether default debug control is enabled.
        /// </summary>
        public bool DefaultDebugControlEnabled { get; set; }

        /// <summary>
        /// Gets or sets the overall material quality for each leveled material in the scene.
        /// </summary>
        public float MaterialQuality { get; set; }

        /// <summary>
        /// Gets or sets the texture filter quality for this drawing pass.
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
        public SamplerState SamplerState
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
                        samplerState.Filter = textureFilter;
                        samplerState.MaxAnisotropy = maxAnisotropy;
                        samplerStateNeedsUpdate = false;
                    }
                }
                return samplerState;
            }
        }

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
        }
    }
}