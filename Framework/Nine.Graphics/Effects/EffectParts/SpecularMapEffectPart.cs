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

    internal class SpecularMapEffectPart : LinkedEffectPart, IEffectTexture
    {
        private uint DirtyMask = 0;
        
        private Texture2D texture;
        private EffectParameter textureParameter;
        private const uint textureDirtyMask = 1 << 0;

        public SpecularMapEffectPart()
        {
            textureParameter = GetParameter("Texture");
        }

        [ContentSerializer(Optional = true)]
        public Texture2D SpecularMap
        {
            get { return texture; }
            set { if (texture != value) { texture = value; DirtyMask |= textureDirtyMask; } }
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & textureDirtyMask) != 0)
            {
                if (textureParameter != null)
                    textureParameter.SetValue(texture);
                DirtyMask &= ~textureDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (SpecularMapEffectPart)part;
            effectPart.SpecularMap = SpecularMap;
        }
        
        protected internal override LinkedEffectPart Clone()
        {
            return new SpecularMapEffectPart()
            {
                SpecularMap = this.SpecularMap,
            };
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Specular)
                SpecularMap = texture as Texture2D;
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }
    }

#endif
}
