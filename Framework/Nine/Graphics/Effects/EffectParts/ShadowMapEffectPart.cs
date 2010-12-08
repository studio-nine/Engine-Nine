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
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    public class ShadowMapEffectPart : LinkedEffectPart, IEffectTexture
    {
        private uint dirtyMask = 0;
        
        private Vector3 shadowColor;
        private EffectParameter shadowColorParameter;
        private const uint shadowColorDirtyMask = 1 << 0;

        private float depthBias;
        private EffectParameter depthBiasParameter;
        private const uint depthBiasDirtyMask = 1 << 1;

        private Matrix lightView;
        private Matrix lightProjection;
        private EffectParameter lightViewProjectionParameter;
        private EffectParameter farClipParameter;
        private const uint lightViewProjectionDirtyMask = 1 << 2;

        private Texture2D shadowMap;
        private EffectParameter shadowMapParameter;
        private const uint shadowMapDirtyMask = 1 << 3;

        [ContentSerializer(Optional = true)]
        public Vector3 ShadowColor
        {
            get { return shadowColor; }
            set { shadowColor = value; dirtyMask |= shadowColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float DepthBias
        {
            get { return depthBias; }
            set { depthBias = value; dirtyMask |= depthBiasDirtyMask; }
        }

        [ContentSerializerIgnore]
        public Matrix LightView
        {
            get { return lightView; }
            set { lightView = value; dirtyMask |= lightViewProjectionDirtyMask; }
        }

        [ContentSerializerIgnore]
        public Matrix LightProjection
        {
            get { return lightProjection; }
            set { lightProjection = value; dirtyMask |= lightViewProjectionDirtyMask; }
        }

        [ContentSerializerIgnore]
        public Texture2D ShadowMap
        {
            get { return shadowMap; }
            set { shadowMap = value; dirtyMask |= shadowMapDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((dirtyMask & shadowColorDirtyMask) != 0)
            {
                if (shadowColorParameter == null)
                    shadowColorParameter = GetParameter("ShadowColor");
                shadowColorParameter.SetValue(shadowColor);
                dirtyMask &= ~shadowColorDirtyMask;
            }

            if ((dirtyMask & depthBiasDirtyMask) != 0)
            {
                if (depthBiasParameter == null)
                    depthBiasParameter = GetParameter("DepthBias");
                depthBiasParameter.SetValue(depthBias);
                dirtyMask &= ~depthBiasDirtyMask;
            }

            if ((dirtyMask & lightViewProjectionDirtyMask) != 0)
            {
                if (lightViewProjectionParameter == null)
                    lightViewProjectionParameter = GetParameter("LightViewProjection");
                lightViewProjectionParameter.SetValue(lightView * lightProjection);
                if (farClipParameter == null)
                    farClipParameter = GetParameter("FarClip");
                farClipParameter.SetValue(Math.Abs(lightProjection.M43 / (Math.Abs(lightProjection.M33) - 1)));
                dirtyMask &= ~lightViewProjectionDirtyMask;
            }

            if ((dirtyMask & shadowMapDirtyMask) != 0)
            {
                if (shadowMapParameter == null)
                    shadowMapParameter = GetParameter("ShadowMap");
                shadowMapParameter.SetValue(shadowMap);
                dirtyMask &= ~shadowMapDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new ShadowMapEffectPart()
            {
                DepthBias = this.DepthBias,
                ShadowColor = this.ShadowColor,
                ShadowMap = this.ShadowMap,
                LightView = this.LightView,
                LightProjection = this.LightProjection,
            };
        }

        Texture2D IEffectTexture.Texture
        {
            get { return null; }
            set { }
        }

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.ShadowMap && texture is Texture2D)
                ShadowMap = texture as Texture2D;
        }
    }

#endif
}
