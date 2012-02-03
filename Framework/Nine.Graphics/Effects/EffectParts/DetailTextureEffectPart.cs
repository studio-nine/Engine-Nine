#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
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

    internal class DetailTextureEffectPart : LinkedEffectPart, IEffectTexture
    {
        private uint DirtyMask = 0;
        
        private Texture2D texture;
        private EffectParameter textureParameter;
        private const uint textureDirtyMask = 1 << 0;

        private Vector2 detailTextureScale;
        private EffectParameter detailTextureScaleParameter;
        private const uint detailTextureScaleDirtyMask = 1 << 1;

        public override bool IsMaterial { get { return true; } }

        [ContentSerializer(Optional = true)]
        public Texture2D DetailTexture
        {
            get { return texture; }
            set { if (texture != value) { texture = value; DirtyMask |= textureDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector2 DetailTextureScale
        {
            get { return detailTextureScale; }
            set { detailTextureScale = value; DirtyMask |= detailTextureScaleDirtyMask; }
        }

        public DetailTextureEffectPart()
        {
            textureParameter = GetParameter("Texture");
            detailTextureScaleParameter = GetParameter("DetailTextureScale");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & textureDirtyMask) != 0)
            {
                if (textureParameter != null)
                    textureParameter.SetValue(texture);
                DirtyMask &= ~textureDirtyMask;
            }
            if ((DirtyMask & detailTextureScaleDirtyMask) != 0)
            {
                if (detailTextureScaleParameter != null)
                    detailTextureScaleParameter.SetValue(detailTextureScale);
                DirtyMask &= ~detailTextureScaleDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (DetailTextureEffectPart)part;
            effectPart.DetailTexture = DetailTexture;
            effectPart.DetailTextureScale = DetailTextureScale;
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new DetailTextureEffectPart()
            {
                DetailTexture = this.DetailTexture,
                DetailTextureScale = this.DetailTextureScale,
            };
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Detail)
                DetailTexture = texture as Texture2D;
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }
    }

#endif
}
