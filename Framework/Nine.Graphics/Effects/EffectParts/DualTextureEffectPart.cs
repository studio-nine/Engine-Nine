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

    internal class DualTextureEffectPart : LinkedEffectPart, IEffectTexture
    {
        private uint DirtyMask = 0;
        
        private Texture2D texture;
        private EffectParameter textureParameter;
        private const uint textureDirtyMask = 1 << 0;

        public override bool IsMaterial { get { return true; } }

        [ContentSerializer(Optional = true)]
        public Texture2D Texture
        {
            get { return texture; }
            set { if (texture != value) { texture = value; DirtyMask |= textureDirtyMask; } }
        }

        public DualTextureEffectPart()
        {
            textureParameter = GetParameter("Texture");
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
            var effectPart = (DualTextureEffectPart)part;
            effectPart.Texture = Texture;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new DualTextureEffectPart()
            {
                Texture = this.Texture,
            };
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }

        public void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Dual)
                Texture = texture as Texture2D;
        }
    }

#endif
}
