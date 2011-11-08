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
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    internal class PointLightEffectPart : LinkedEffectPart, IPointLight
    {
        private uint DirtyMask = 0;
        
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
            set { position = value; DirtyMask |= positionDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; DirtyMask |= diffuseColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; DirtyMask |= specularColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Range
        {
            get { return range; }
            set { range = value; DirtyMask |= rangeDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; DirtyMask |= attenuationDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & positionDirtyMask) != 0)
            {
                if (positionParameter == null)
                    positionParameter = GetParameter("PointLightPosition");
                positionParameter.SetValue(position);
                DirtyMask &= ~positionDirtyMask;
            }

            if ((DirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter == null)
                    diffuseColorParameter = GetParameter("PointLightDiffuseColor");
                diffuseColorParameter.SetValue(diffuseColor);
                DirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((DirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter == null)
                    specularColorParameter = GetParameter("PointLightSpecularColor");
                specularColorParameter.SetValue(specularColor);
                DirtyMask &= ~specularColorDirtyMask;
            }

            if ((DirtyMask & rangeDirtyMask) != 0)
            {
                if (rangeParameter == null)
                    rangeParameter = GetParameter("Range");
                rangeParameter.SetValue(range);
                DirtyMask &= ~rangeDirtyMask;
            }

            if ((DirtyMask & attenuationDirtyMask) != 0)
            {
                if (attenuationParameter == null)
                    attenuationParameter = GetParameter("Attenuation");
                attenuationParameter.SetValue(attenuation);
                DirtyMask &= ~attenuationDirtyMask;
            }
        }

        public PointLightEffectPart()
        {
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
            Range = 10;
            Attenuation = MathHelper.E;
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
