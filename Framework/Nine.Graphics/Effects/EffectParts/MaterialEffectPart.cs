#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
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

    internal class MaterialEffectPart : LinkedEffectPart, IEffectMaterial
    {
        private uint DirtyMask = 0;
        
        private float specularPower;
        private EffectParameter specularPowerParameter;
        private const uint specularPowerDirtyMask = 1 << 0;

        private Vector3 diffuseColor;
        private EffectParameter diffuseColorParameter;
        private const uint diffuseColorDirtyMask = 1 << 2;

        private Vector3 emissiveColor;
        private EffectParameter emissiveColorParameter;
        private const uint emissiveColorDirtyMask = 1 << 3;

        private Vector3 specularColor;
        private EffectParameter specularColorParameter;
        private const uint specularColorDirtyMask = 1 << 4;

        private float alpha;
        private EffectParameter alphaParameter;
        private const uint alphaDirtyMask = 1 << 5;

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

        [ContentSerializer(Optional = true)]
        public float Alpha
        {
            get { return alpha; }
            set { alpha = value; DirtyMask |= alphaDirtyMask; }
        }

        public override bool IsMaterial { get { return true; } }

        public MaterialEffectPart()
        {
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            SpecularColor = Vector3.Zero;
            SpecularPower = 16;
            Alpha = 1;

            specularPowerParameter = GetParameter("SpecularPower");
            diffuseColorParameter = GetParameter("DiffuseColor");
            emissiveColorParameter = GetParameter("EmissiveColor");
            specularColorParameter = GetParameter("SpecularColor");
            alphaParameter = GetParameter("Alpha");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & specularPowerDirtyMask) != 0)
            {
                if (specularPowerParameter != null)
                    specularPowerParameter.SetValue(specularPower);
                DirtyMask &= ~specularPowerDirtyMask;
            }

            if ((DirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter != null)
                    diffuseColorParameter.SetValue(diffuseColor);
                DirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((DirtyMask & emissiveColorDirtyMask) != 0)
            {
                if (emissiveColorParameter != null)
                    emissiveColorParameter.SetValue(emissiveColor);
                DirtyMask &= ~emissiveColorDirtyMask;
            }

            if ((DirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter != null)
                    specularColorParameter.SetValue(specularColor);
                DirtyMask &= ~specularColorDirtyMask;
            }

            if ((DirtyMask & alphaDirtyMask) != 0)
            {
                if (alphaParameter != null)
                    alphaParameter.SetValue(alpha);
                DirtyMask &= ~alphaDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (MaterialEffectPart)part;
            effectPart.DiffuseColor = DiffuseColor;
            effectPart.EmissiveColor = EmissiveColor;
            effectPart.SpecularColor = SpecularColor;
            effectPart.SpecularPower = SpecularPower;
            effectPart.Alpha = Alpha;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new MaterialEffectPart()
            {
                DiffuseColor = this.DiffuseColor,
                EmissiveColor = this.EmissiveColor,
                SpecularColor = this.SpecularColor,
                SpecularPower = this.SpecularPower,
                Alpha = this.Alpha,
            };
        }
    }

#endif
}
