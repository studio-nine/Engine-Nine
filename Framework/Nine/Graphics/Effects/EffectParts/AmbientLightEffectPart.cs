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

    public class AmbientLightEffectPart : LinkedEffectPart
    {
        private uint dirtyMask = 0;
        private Vector3 ambientLightColor;
        private EffectParameter ambientLightColorParameter;
        private const uint ambientLightDirtyMask = 1 << 0;

        [ContentSerializer(Optional = true)]
        public Vector3 AmbientLightColor 
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; dirtyMask |= ambientLightDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((dirtyMask & ambientLightDirtyMask) != 0)
            {
                if (ambientLightColorParameter == null)
                    ambientLightColorParameter = GetParameter("AmbientLightColor");
                ambientLightColorParameter.SetValue(ambientLightColor);
                dirtyMask &= ~ambientLightDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new AmbientLightEffectPart()
            {
                AmbientLightColor = this.AmbientLightColor,
            };
        }
    }

#endif
}
