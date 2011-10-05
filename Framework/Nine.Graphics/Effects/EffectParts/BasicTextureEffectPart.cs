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

    internal class BasicTextureEffectPart : LinkedEffectPart, IEffectTexture
    {
        private uint DirtyMask = 0;
        
        private Texture2D texture;
        private EffectParameter textureParameter;
        private const uint textureDirtyMask = 1 << 0;

        private Vector3 overlayColor;
        private EffectParameter overlayColorParameter;
        private const uint overlayColorDirtyMask = 1 << 1;

        [ContentSerializer(Optional = true)]
        public Texture2D Texture
        {
            get { return texture; }
            set { if (texture != value) { texture = value; DirtyMask |= textureDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 OverlayColor
        {
            get { return overlayColor; }
            set { overlayColor = value; DirtyMask |= overlayColorDirtyMask; }
        }

        public override bool IsMaterial { get { return true; } }

        public BasicTextureEffectPart()
        {
            textureParameter = GetParameter("Texture");
            overlayColorParameter = GetParameter("OverlayColor");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & textureDirtyMask) != 0)
            {
                if (textureParameter != null)
                    textureParameter.SetValue(texture);
                DirtyMask &= ~textureDirtyMask;
            }

            if ((DirtyMask & overlayColorDirtyMask) != 0)
            {
                if (overlayColorParameter != null)
                    overlayColorParameter.SetValue(overlayColor);
                DirtyMask &= ~overlayColorDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (BasicTextureEffectPart)part;
            effectPart.Texture = Texture;
            effectPart.OverlayColor = OverlayColor;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new BasicTextureEffectPart()
            {
                Texture = this.Texture,
                OverlayColor = this.OverlayColor,
            };
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {

        }
    }

#endif
}
