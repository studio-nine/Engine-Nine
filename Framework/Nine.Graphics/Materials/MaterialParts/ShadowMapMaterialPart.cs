namespace Nine.Graphics.Materials.MaterialParts
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class ShadowMapMaterialPart : MaterialPart, IEffectShadowMap
    {
        private EffectParameter shadowColorParameter;
        private EffectParameter depthBiasParameter;
        private EffectParameter lightViewProjectionParameter;
        private EffectParameter shadowMapParameter;
        private EffectParameter shadowMapSizeParameter;

        public Vector3 ShadowColor { get; set; }
        public float DepthBias { get; set; }
        public Matrix LightViewProjection { get; set; }
        public Texture2D ShadowMap { get; set; }

        [DefaultValue(10)]
        public int SampleCount
        {
            get { return sampleCount; }
            set
            {
                if (value != sampleCount)
                {
                    sampleCount = value;
                    NotifyShaderChanged();
                }
            }
        }
        private int sampleCount = 10;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowMapMaterialPart"/> class.
        /// </summary>
        internal ShadowMapMaterialPart()
        {
            ShadowColor = Vector3.One * 0.5f;
            DepthBias = 0.005f;
        }
        
        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal override void OnBind()
        {
            shadowColorParameter = GetParameter("ShadowColor");
            depthBiasParameter = GetParameter("DepthBias");
            lightViewProjectionParameter = GetParameter("LightViewProjection");
            shadowMapParameter = GetParameter("ShadowMap");
            shadowMapSizeParameter = GetParameter("ShadowMapTexelSize");
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (shadowColorParameter != null)
            {
                shadowColorParameter.SetValue(ShadowColor);
                depthBiasParameter.SetValue(DepthBias);
                lightViewProjectionParameter.SetValue(LightViewProjection);
                shadowMapParameter.SetValue(ShadowMap);
                shadowMapSizeParameter.SetValue(new Vector2(1.0f / ShadowMap.Width, 1.0f / ShadowMap.Height));
            }
        }

        protected internal override MaterialPart Clone()
        {
            return new ShadowMapMaterialPart()
            {
                DepthBias = this.DepthBias,
                ShadowColor = this.ShadowColor,
                ShadowMap = this.ShadowMap,
                LightViewProjection = this.LightViewProjection,
                sampleCount = this.sampleCount,
            };
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.ShadowMap && texture is Texture2D)
                ShadowMap = texture as Texture2D;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            if (usage != MaterialUsage.Default)
                return null;
            return GetShaderCode("ShadowMap").Replace("{$SAMPLECOUNT}", SampleCount.ToString());
        }
    }
}
