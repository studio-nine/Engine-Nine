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

    public class DualTextureEffectPart : LinkedEffectPart
    {
        private uint dirtyMask = 0;
        private Texture2D texture;
        private EffectParameter textureParameter;
        private const uint textureDirtyMask = 1 << 0;

        [ContentSerializer(Optional = true)]
        public Texture2D Texture
        {
            get { return texture; }
            set { texture = value; dirtyMask |= textureDirtyMask; }
        }

        protected internal override void OnApply()
        {
            if ((dirtyMask & textureDirtyMask) != 0)
            {
                if (textureParameter == null)
                    textureParameter = GetParameter("Texture");
                textureParameter.SetValue(texture);
                dirtyMask &= ~textureDirtyMask;
            }
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new DualTextureEffectPart()
            {
                Texture = this.Texture,
            };
        }
    }

#endif
}
