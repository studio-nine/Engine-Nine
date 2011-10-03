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

    class PixelShaderOutputEffectPart : LinkedEffectPart, IEffectMaterial
    {
        private uint DirtyMask = 0;

        private float specularPower;
        private EffectParameter specularPowerParameter;
        private const uint specularPowerDirtyMask = 1 << 0;

        [ContentSerializer(Optional = true)]
        public float SpecularPower
        {
            get { return specularPower; }
            set { specularPower = value; DirtyMask |= specularPowerDirtyMask; }
        }

        public override bool IsMaterial { get { return true; } }

        protected internal override void OnApply()
        {
            if ((DirtyMask & specularPowerDirtyMask) != 0)
            {
                if (specularPowerParameter == null)
                    specularPowerParameter = GetParameter("SpecularPower");
                if (specularPowerParameter != null)
                    specularPowerParameter.SetValue(specularPower);
                DirtyMask &= ~specularPowerDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (PixelShaderOutputEffectPart)part;
            effectPart.SpecularPower = SpecularPower;
        }

        public PixelShaderOutputEffectPart()
        {
            SpecularPower = 16;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new PixelShaderOutputEffectPart()
            {
                SpecularPower = this.SpecularPower
            };
        }

        float IEffectMaterial.Alpha
        {
            get { return 1; }
            set { }
        }

        Vector3 IEffectMaterial.DiffuseColor
        {
            get { return Vector3.One; }
            set { }
        }

        Vector3 IEffectMaterial.EmissiveColor
        {
            get { return Vector3.Zero; }
            set { }
        }

        Vector3 IEffectMaterial.SpecularColor
        {
            get { return Vector3.Zero; }
            set { }
        }
    }

#endif
}
