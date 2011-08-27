#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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

    internal class BeginLightEffectPart : LinkedEffectPart, IEffectMatrices, IEffectMaterial
    {
        private uint DirtyMask = 0;
        
        private float specularPower;
        private EffectParameter specularPowerParameter;
        private const uint specularPowerDirtyMask = 1 << 0;

        private Matrix view;
        private EffectParameter eyePositionParameter;
        private const uint eyePositionDirtyMask = 1 << 1;

        private Vector3 diffuseColor;
        private EffectParameter diffuseColorParameter;
        private const uint diffuseColorDirtyMask = 1 << 2;

        private Vector3 emissiveColor;
        private EffectParameter emissiveColorParameter;
        private const uint emissiveColorDirtyMask = 1 << 3;

        private Vector3 specularColor;
        private EffectParameter specularColorParameter;
        private const uint specularColorDirtyMask = 1 << 4;

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; DirtyMask |= diffuseColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }
            set { emissiveColor = value; DirtyMask |= emissiveColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; DirtyMask |= specularColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float SpecularPower
        {
            get { return specularPower; }
            set { specularPower = value; DirtyMask |= specularPowerDirtyMask; }
        }

        float IEffectMaterial.Alpha
        {
            get { return 1; }
            set { }
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & specularPowerDirtyMask) != 0)
            {
                if (specularPowerParameter == null)
                    specularPowerParameter = GetParameter("SpecularPower");
                specularPowerParameter.SetValue(specularPower);
                DirtyMask &= ~specularPowerDirtyMask;
            }

            if ((DirtyMask & eyePositionDirtyMask) != 0)
            {
                if (eyePositionParameter == null)
                    eyePositionParameter = GetParameter("EyePosition");
                eyePositionParameter.SetValue(Matrix.Invert(view).Translation);
                DirtyMask &= ~eyePositionDirtyMask;
            }

            if ((DirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter == null)
                    diffuseColorParameter = GetParameter("DiffuseColor");
                diffuseColorParameter.SetValue(diffuseColor);
                DirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((DirtyMask & emissiveColorDirtyMask) != 0)
            {
                if (emissiveColorParameter == null)
                    emissiveColorParameter = GetParameter("EmissiveColor");
                emissiveColorParameter.SetValue(emissiveColor);
                DirtyMask &= ~emissiveColorDirtyMask;
            }

            if ((DirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter == null)
                    specularColorParameter = GetParameter("SpecularColor");
                specularColorParameter.SetValue(specularColor);
                DirtyMask &= ~specularColorDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (BeginLightEffectPart)part;
            effectPart.DiffuseColor = DiffuseColor;
            effectPart.EmissiveColor = EmissiveColor;
            effectPart.SpecularColor = SpecularColor;
            effectPart.SpecularPower = SpecularPower;
        }
        
        public BeginLightEffectPart()
        {
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            SpecularColor = Vector3.Zero;
            SpecularPower = 16;
        }


        protected internal override LinkedEffectPart Clone()
        {
            return new BeginLightEffectPart()
            {
                DiffuseColor = this.DiffuseColor,
                EmissiveColor = this.EmissiveColor,
                SpecularColor = this.SpecularColor,
                SpecularPower = this.SpecularPower,
            };
        }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.Projection { get { return Matrix.Identity; } set { } }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.World { get { return Matrix.Identity; } set { } }

        [ContentSerializerIgnore]
        Matrix IEffectMatrices.View
        {
            get { return view; }
            set { view = value; DirtyMask |= eyePositionDirtyMask; }
        }
    }

#endif
}
