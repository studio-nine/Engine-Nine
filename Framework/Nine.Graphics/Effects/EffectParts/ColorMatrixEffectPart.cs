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

    internal class ColorMatrixEffectPart : LinkedEffectPart
    {
        private uint dirtyMask = 0;
        private Matrix transform;
        private EffectParameter transformParameter;
        private const uint transformDirtyMask = 1 << 0;

        [ContentSerializer(Optional = true)]
        public Matrix Transform 
        {
            get { return transform; }
            set { transform = value; dirtyMask |= transformDirtyMask; }
        }

        public ColorMatrixEffectPart()
        {
            Transform = Matrix.Identity;
        }

        protected internal override void OnApply()
        {
            if ((dirtyMask & transformDirtyMask) != 0)
            {
                if (transformParameter == null)
                    transformParameter = GetParameter("Transform");
                transformParameter.SetValue(transform);
                dirtyMask &= ~transformDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new ColorMatrixEffectPart()
            {
                Transform = this.Transform,
            };
        }
    }

#endif
}
