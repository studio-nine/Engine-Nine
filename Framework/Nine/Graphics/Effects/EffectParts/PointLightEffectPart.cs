#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    internal class PointLightEffectPart : LinkedEffectPart, IEffectPointLight
    {
        private uint dirtyMask = 0;
        
        private Vector3 position;
        private EffectParameter positionParameter;
        private const uint positionDirtyMask = 1 << 0;

        private Vector3 diffuseColor;
        private EffectParameter diffuseColorParameter;
        private const uint diffuseColorDirtyMask = 1 << 1;

        private Vector3 specularColor;
        private EffectParameter specularColorParameter;
        private const uint specularColorDirtyMask = 1 << 2;

        private float range;
        private EffectParameter rangeParameter;
        private const uint rangeDirtyMask = 1 << 3;

        private float attenuation;
        private EffectParameter attenuationParameter;
        private const uint attenuationDirtyMask = 1 << 4;
        
        [ContentSerializer(Optional = true)]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; dirtyMask |= positionDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; dirtyMask |= diffuseColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; dirtyMask |= specularColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Range
        {
            get { return range; }
            set { range = value; dirtyMask |= rangeDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; dirtyMask |= attenuationDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((dirtyMask & positionDirtyMask) != 0)
            {
                if (positionParameter == null)
                    positionParameter = GetParameter("PointLightPosition");
                positionParameter.SetValue(position);
                dirtyMask &= ~positionDirtyMask;
            }

            if ((dirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter == null)
                    diffuseColorParameter = GetParameter("PointLightDiffuseColor");
                diffuseColorParameter.SetValue(diffuseColor);
                dirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((dirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter == null)
                    specularColorParameter = GetParameter("PointLightSpecularColor");
                specularColorParameter.SetValue(specularColor);
                dirtyMask &= ~specularColorDirtyMask;
            }

            if ((dirtyMask & rangeDirtyMask) != 0)
            {
                if (rangeParameter == null)
                    rangeParameter = GetParameter("Range");
                rangeParameter.SetValue(range);
                dirtyMask &= ~rangeDirtyMask;
            }

            if ((dirtyMask & attenuationDirtyMask) != 0)
            {
                if (attenuationParameter == null)
                    attenuationParameter = GetParameter("Attenuation");
                attenuationParameter.SetValue(attenuation);
                dirtyMask &= ~attenuationDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new PointLightEffectPart()
            {
                Position = this.Position,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                Range = this.Range,
                Attenuation = this.Attenuation,
            };
        }
    }

#endif
}
