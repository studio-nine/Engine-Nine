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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    internal class ShadowMapEffectPart : LinkedEffectPart, IEffectTexture, IEffectShadowMap
    {
        private uint DirtyMask = 0;
        
        private Vector3 shadowColor;
        private EffectParameter shadowColorParameter;
        private const uint shadowColorDirtyMask = 1 << 0;

        private float depthBias;
        private EffectParameter depthBiasParameter;
        private const uint depthBiasDirtyMask = 1 << 1;

        private Matrix lightViewProjection;
        private EffectParameter lightViewProjectionParameter;
        private const uint lightViewProjectionDirtyMask = 1 << 2;

        private Texture2D shadowMap;
        private EffectParameter shadowMapParameter;
        private EffectParameter shadowMapSizeParameter;
        private const uint shadowMapDirtyMask = 1 << 3;

        public override bool IsMaterial
        {
            get { return true; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set { if (shadowColor != value) { shadowColor = value; DirtyMask |= shadowColorDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public float DepthBias
        {
            get { return depthBias; }
            set { if (depthBias != value) { depthBias = value; DirtyMask |= depthBiasDirtyMask; } }
        }

        [ContentSerializerIgnore]
        public Matrix LightViewProjection
        {
            get { return lightViewProjection; }
            set { lightViewProjection = value; DirtyMask |= lightViewProjectionDirtyMask; }
        }
        
        [ContentSerializerIgnore]
        public Texture2D ShadowMap
        {
            get { return shadowMap; }
            set { if (shadowMap != value) { shadowMap = value; DirtyMask |= shadowMapDirtyMask; } }
        }

        public ShadowMapEffectPart()
        {
            ShadowColor = Vector3.One * 0.5f;
            DepthBias = 0.005f;

            shadowColorParameter = GetParameter("ShadowColor");
            depthBiasParameter = GetParameter("DepthBias");
            lightViewProjectionParameter = GetParameter("LightViewProjection");
            shadowMapParameter = GetParameter("ShadowMap");
            shadowMapSizeParameter = GetParameter("ShadowMapTexelSize");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & shadowColorDirtyMask) != 0)
            {
                if (shadowColorParameter != null)
                    shadowColorParameter.SetValue(shadowColor);
                DirtyMask &= ~shadowColorDirtyMask;
            }

            if ((DirtyMask & depthBiasDirtyMask) != 0)
            {
                if (depthBiasParameter != null)
                    depthBiasParameter.SetValue(depthBias);
                DirtyMask &= ~depthBiasDirtyMask;
            }

            if ((DirtyMask & lightViewProjectionDirtyMask) != 0)
            {
                if (lightViewProjectionParameter != null)
                    lightViewProjectionParameter.SetValue(lightViewProjection);
                DirtyMask &= ~lightViewProjectionDirtyMask;
            }

            if ((DirtyMask & shadowMapDirtyMask) != 0)
            {
                if (shadowMapParameter != null)
                    shadowMapParameter.SetValue(shadowMap);
                if (shadowMapSizeParameter != null && shadowMap != null)
                    shadowMapSizeParameter.SetValue(new Vector2(1.0f / shadowMap.Width, 1.0f / shadowMap.Height));
                DirtyMask &= ~shadowMapDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (ShadowMapEffectPart)part;
            effectPart.DepthBias = DepthBias;
            effectPart.ShadowColor = ShadowColor;
            effectPart.ShadowMap = ShadowMap;
            effectPart.LightViewProjection = LightViewProjection;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new ShadowMapEffectPart()
            {
                DepthBias = this.DepthBias,
                ShadowColor = this.ShadowColor,
                ShadowMap = this.ShadowMap,
                LightViewProjection = this.LightViewProjection,
            };
        }

        Texture2D IEffectTexture.Texture
        {
            get { return null; }
            set { }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.ShadowMap && texture is Texture2D)
                ShadowMap = texture as Texture2D;
        }
    }

#endif
}
