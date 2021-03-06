﻿namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Text;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class ShadowMapMaterialPart : MaterialPart, IEffectShadowMap
    {
        private EffectParameter shadowColorParameter;
        private EffectParameter depthBiasParameter;
        private EffectParameter lightViewProjectionParameter;
        private EffectParameter shadowMapSizeParameter;
        private EffectParameter shadowMapParameter;
        private int shadowMapSamplerIndex;

        public Vector3 ShadowColor { get; set; }
        public Matrix LightViewProjection { get; set; }
        public Texture2D ShadowMap { get; set; }

        public int FilterSize
        {
            get { return filterSize; }
            set
            {
                if (filterSize != value)
                {
                    filterSize = value;
                    NotifyShaderChanged();
                }
            }
        }
        private int filterSize = 3;

        public int Seed
        {
            get { return seed; }
            set
            {
                if (value != seed)
                {
                    seed = value;
                    NotifyShaderChanged();
                }
            }
        }
        private int seed = 20090817;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShadowMapMaterialPart"/> class.
        /// </summary>
        internal ShadowMapMaterialPart()
        {
            ShadowColor = Vector3.One * 0.5f;
        }
        
        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal override void OnBind()
        {
            shadowColorParameter = GetParameter("ShadowColor");
            depthBiasParameter = GetParameter("DepthBias");
            lightViewProjectionParameter = GetParameter("LightViewProjection");
            shadowMapSizeParameter = GetParameter("ShadowMapTexelSize");
            GetTextureParameter("ShadowMap", out shadowMapParameter, out shadowMapSamplerIndex);
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext3D context, MaterialGroup material)
        {
            if (shadowMapParameter == null)
                return;
            
            var light = context.DirectionalLight;
            if (light.ShadowMap == null)
                return;
            
            var texture = light.ShadowMap.Texture;
            if (texture == null)
                return;
            
            shadowMapParameter.SetValue(texture);
            shadowMapSizeParameter.SetValue(new Vector2(1.0f / texture.Width, 1.0f / texture.Height));
            lightViewProjectionParameter.SetValue(light.ShadowFrustum.Matrix);
            shadowColorParameter.SetValue(ShadowColor);
            context.graphics.SamplerStates[shadowMapSamplerIndex] = SamplerState.PointClamp;
        }

        protected internal override void EndApplyLocalParameters(DrawingContext3D context)
        {
            if (shadowMapParameter != null)
            {
                context.graphics.Textures[shadowMapSamplerIndex] = null;
                context.graphics.SamplerStates[shadowMapSamplerIndex] = context.SamplerState;
            }
        }

        protected internal override void GetDependentParts(MaterialUsage usage, IList<Type> result)
        {
            result.Add(typeof(EndLightMaterialPart));
        }

        protected internal override MaterialPart Clone()
        {
            return new ShadowMapMaterialPart()
            {
                ShadowColor = this.ShadowColor,
                ShadowMap = this.ShadowMap,
                LightViewProjection = this.LightViewProjection,
                filterSize = this.filterSize,
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
            return GetShaderCode("ShadowMap").Replace("{$SAMPLECOUNT}", (filterSize * filterSize).ToString())
                                             .Replace("{$FILTERTAPS}", CreateFilterTaps());
        }

        private string CreateFilterTaps()
        {
            var taps = new StringBuilder();
            var inv = 1.0 / filterSize;
            var random = new Random(seed);
            for (int x = 0; x < filterSize; ++x)
            {
                for (int y = 0; y < filterSize; ++y)
                {
                    // Create a random filter disc base on
                    // http://http.developer.nvidia.com/GPUGems2/gpugems2_chapter17.html
                    var u = (x + random.NextDouble()) * inv;
                    var v = (y + random.NextDouble()) * inv;

                    v = Math.Sqrt(v);
                    u = u * Math.PI * 2;

                    var xx = v * Math.Cos(u);
                    var yy = v * Math.Sin(u);

                    taps.Append("{");
                    taps.Append(xx.ToString(CultureInfo.InvariantCulture));
                    taps.Append(",");
                    taps.Append(yy.ToString(CultureInfo.InvariantCulture));
                    taps.AppendLine("},");
                }
            }
            return taps.ToString();
        }
    }
}
