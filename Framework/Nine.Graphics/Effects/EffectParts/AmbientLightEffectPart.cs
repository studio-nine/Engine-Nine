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
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    internal class AmbientLightEffectPart : LinkedEffectPart, IAmbientLight
    {
        private uint DirtyMask = 0;
        
        private Vector3 ambientLightColor;
        private EffectParameter ambientLightColorParameter;
        private const uint ambientLightDirtyMask = 1 << 0;

        [ContentSerializer(Optional = true)]
        public Vector3 AmbientLightColor 
        {
            get { return ambientLightColor; }
            set { ambientLightColor = value; DirtyMask |= ambientLightDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & ambientLightDirtyMask) != 0)
            {
                if (ambientLightColorParameter == null)
                    ambientLightColorParameter = GetParameter("AmbientLightColor");
                ambientLightColorParameter.SetValue(ambientLightColor);
                DirtyMask &= ~ambientLightDirtyMask;
            }
        }
        
        public AmbientLightEffectPart()
        {
            AmbientLightColor = Vector3.One * 0.2f;
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
