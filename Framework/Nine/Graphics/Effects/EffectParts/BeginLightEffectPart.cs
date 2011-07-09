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
        private uint dirtyMask = 0;
        
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
            set { diffuseColor = value; dirtyMask |= diffuseColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }
            set { emissiveColor = value; dirtyMask |= emissiveColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; dirtyMask |= specularColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public float SpecularPower
        {
            get { return specularPower; }
            set { specularPower = value; dirtyMask |= specularPowerDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((dirtyMask & specularPowerDirtyMask) != 0)
            {
                if (specularPowerParameter == null)
                    specularPowerParameter = GetParameter("SpecularPower");
                specularPowerParameter.SetValue(specularPower);
                dirtyMask &= ~specularPowerDirtyMask;
            }

            if ((dirtyMask & eyePositionDirtyMask) != 0)
            {
                if (eyePositionParameter == null)
                    eyePositionParameter = GetParameter("EyePosition");
                eyePositionParameter.SetValue(Matrix.Invert(view).Translation);
                dirtyMask &= ~eyePositionDirtyMask;
            }

            if ((dirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter == null)
                    diffuseColorParameter = GetParameter("DiffuseColor");
                diffuseColorParameter.SetValue(diffuseColor);
                dirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((dirtyMask & emissiveColorDirtyMask) != 0)
            {
                if (emissiveColorParameter == null)
                    emissiveColorParameter = GetParameter("EmissiveColor");
                emissiveColorParameter.SetValue(emissiveColor);
                dirtyMask &= ~emissiveColorDirtyMask;
            }

            if ((dirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter == null)
                    specularColorParameter = GetParameter("SpecularColor");
                specularColorParameter.SetValue(specularColor);
                dirtyMask &= ~specularColorDirtyMask;
            }
        }       
        
        public BeginLightEffectPart()
        {
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            SpecularColor = Vector3.One;
            SpecularPower = 32;
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
            set { view = value; dirtyMask |= eyePositionDirtyMask; }
        }
    }

#endif
}
