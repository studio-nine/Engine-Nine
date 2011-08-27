#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    internal class SplatterTextureEffectPart : LinkedEffectPart, IEffectSplatterTexture
    {
        private uint DirtyMask = 0;

        private Texture2D textureX;
        private EffectParameter textureXParameter;
        private const uint textureXDirtyMask = 1 << 0;

        private Texture2D textureY;
        private EffectParameter textureYParameter;
        private const uint textureYDirtyMask = 1 << 1;

        private Texture2D textureZ;
        private EffectParameter textureZParameter;
        private const uint textureZDirtyMask = 1 << 2;

        private Texture2D textureW;
        private EffectParameter textureWParameter;
        private const uint textureWDirtyMask = 1 << 3;

        private Texture2D splatterTexture;
        private EffectParameter splatterTextureParameter;
        private const uint splatterTextureDirtyMask = 1 << 4;

        private Vector2 splatterTextureScale = Vector2.One;
        private EffectParameter splatterTextureScaleParameter;
        private const uint splatterTextureScaleDirtyMask = 1 << 5;

        private EffectParameter maskParameter;
        internal const uint maskDirtyMask = 1 << 6;

        [ContentSerializer(Optional = true)]
        public Texture2D TextureX
        {
            get { return textureX; }
            set { if (textureX != value) { textureX = value; DirtyMask |= textureXDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Texture2D TextureY
        {
            get { return textureY; }
            set { if (textureY != value) { textureY = value; DirtyMask |= textureYDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Texture2D TextureZ
        {
            get { return textureZ; }
            set { if (textureZ != value) { textureZ = value; DirtyMask |= textureZDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Texture2D TextureW
        {
            get { return textureW; }
            set { if (textureW != value) { textureW = value; DirtyMask |= textureWDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Texture2D SplatterTexture
        {
            get { return splatterTexture; }
            set { if (splatterTexture != value) { splatterTexture = value; DirtyMask |= splatterTextureDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector2 SplatterTextureScale
        {
            get { return splatterTextureScale; }
            set { splatterTextureScale = value; DirtyMask |= splatterTextureScaleDirtyMask; }
        }


        public SplatterTextureEffectPart()
        {
            DirtyMask |= splatterTextureDirtyMask;
            DirtyMask |= maskDirtyMask;
            SplatterTextureScale = Vector2.One;
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & textureXDirtyMask) != 0)
            {
                if (textureXParameter == null)
                    textureXParameter = GetParameter("TextureX");
                textureXParameter.SetValue(textureX);
                DirtyMask &= ~textureXDirtyMask;
            }

            if ((DirtyMask & textureYDirtyMask) != 0)
            {
                if (textureYParameter == null)
                    textureYParameter = GetParameter("TextureY");
                textureYParameter.SetValue(textureY);
                DirtyMask &= ~textureYDirtyMask;
            }

            if ((DirtyMask & textureZDirtyMask) != 0)
            {
                if (textureZParameter == null)
                    textureZParameter = GetParameter("TextureZ");
                textureZParameter.SetValue(textureZ);
                DirtyMask &= ~textureZDirtyMask;
            }

            if ((DirtyMask & textureWDirtyMask) != 0)
            {
                if (textureWParameter == null)
                    textureWParameter = GetParameter("TextureW");
                textureWParameter.SetValue(textureW);
                DirtyMask &= ~textureWDirtyMask;
            }

            if ((DirtyMask & splatterTextureScaleDirtyMask) != 0)
            {
                if (splatterTextureScaleParameter == null)
                    splatterTextureScaleParameter = GetParameter("SplatterTextureScale");
                splatterTextureScaleParameter.SetValue(splatterTextureScale);
                DirtyMask &= ~splatterTextureScaleDirtyMask;
            }

            if ((DirtyMask & splatterTextureDirtyMask) != 0)
            {
                if (splatterTextureParameter == null)
                    splatterTextureParameter = GetParameter("SplatterTexture");
                splatterTextureParameter.SetValue(splatterTexture);
                DirtyMask &= ~splatterTextureDirtyMask;
            }

            if ((DirtyMask & maskDirtyMask) != 0)
            {
                if (maskParameter == null)
                    maskParameter = GetParameter("Mask");

                Vector4 mask = new Vector4();
                mask.X = (TextureX != null ? 1.0f : 0.0f);
                mask.Y = (TextureY != null ? 1.0f : 0.0f);
                mask.Z = (TextureZ != null ? 1.0f : 0.0f);
                mask.W = (TextureW != null ? 1.0f : 0.0f);
                
                maskParameter.SetValue(mask);
                DirtyMask &= ~maskDirtyMask;
            }
        }

        protected internal override void OnApply(LinkedEffectPart part)
        {
            var effectPart = (SplatterTextureEffectPart)part;
            effectPart.TextureX = TextureX;
            effectPart.TextureY = TextureY;
            effectPart.TextureZ = TextureZ;
            effectPart.TextureW = TextureW;
            effectPart.SplatterTexture = SplatterTexture;
            effectPart.SplatterTextureScale = SplatterTextureScale;            
        }

        protected internal override LinkedEffectPart Clone()
        {
            return new SplatterTextureEffectPart()
            {
                TextureX = TextureX,
                TextureY = TextureY,
                TextureZ = TextureZ,
                TextureW = TextureW,
                SplatterTexture = SplatterTexture,
                SplatterTextureScale = SplatterTextureScale,
            };
        }
    }

#endif
}
