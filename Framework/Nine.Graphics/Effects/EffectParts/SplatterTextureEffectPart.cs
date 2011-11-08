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
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Effects.EffectParts
{
#if !WINDOWS_PHONE

    /// <summary>
    /// FIXME: This is an internal class, how to change these textures at runtime.
    /// </summary>
    internal class SplatterTextureEffectPart : LinkedEffectPart
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

        private Texture2D normalTextureX;
        private EffectParameter normalTextureXParameter;
        private const uint normalTextureXDirtyMask = 1 << 7;

        private Texture2D normalTextureY;
        private EffectParameter normalTextureYParameter;
        private const uint normalTextureYDirtyMask = 1 << 8;

        private Texture2D normalTextureZ;
        private EffectParameter normalTextureZParameter;
        private const uint normalTextureZDirtyMask = 1 << 9;

        private Texture2D normalTextureW;
        private EffectParameter normalTextureWParameter;
        private const uint normalTextureWDirtyMask = 1 << 10;
        
        private Vector3 diffuseColorX;
        private EffectParameter diffuseColorXParameter;
        private const uint diffuseColorXDirtyMask = 1 << 11;

        private Vector3 diffuseColorY;
        private EffectParameter diffuseColorYParameter;
        private const uint diffuseColorYDirtyMask = 1 << 12;

        private Vector3 diffuseColorZ;
        private EffectParameter diffuseColorZParameter;
        private const uint diffuseColorZDirtyMask = 1 << 13;

        private Vector3 diffuseColorW;
        private EffectParameter diffuseColorWParameter;
        private const uint diffuseColorWDirtyMask = 1 << 14;
        
        private Vector3 specularColorX;
        private EffectParameter specularColorXParameter;
        private const uint specularColorXDirtyMask = 1 << 15;

        private Vector3 specularColorY;
        private EffectParameter specularColorYParameter;
        private const uint specularColorYDirtyMask = 1 << 16;

        private Vector3 specularColorZ;
        private EffectParameter specularColorZParameter;
        private const uint specularColorZDirtyMask = 1 << 17;

        private Vector3 specularColorW;
        private EffectParameter specularColorWParameter;
        private const uint specularColorWDirtyMask = 1 << 18;

        private float specularPowerX;
        private EffectParameter specularPowerXParameter;
        private const uint specularPowerXDirtyMask = 1 << 19;

        private float specularPowerY;
        private EffectParameter specularPowerYParameter;
        private const uint specularPowerYDirtyMask = 1 << 20;

        private float specularPowerZ;
        private EffectParameter specularPowerZParameter;
        private const uint specularPowerZDirtyMask = 1 << 21;

        private float specularPowerW;
        private EffectParameter specularPowerWParameter;
        private const uint specularPowerWDirtyMask = 1 << 22;

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
        public  Texture2D  NormalMapX
        {
            get { return normalTextureX; }
            set { if (normalTextureX != value) { normalTextureX = value; DirtyMask |= normalTextureXDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public  Texture2D  NormalMapY
        {
            get { return normalTextureY; }
            set { if (normalTextureY != value) { normalTextureY = value; DirtyMask |= normalTextureYDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public  Texture2D  NormalMapZ
        {
            get { return normalTextureZ; }
            set { if (normalTextureZ != value) { normalTextureZ = value; DirtyMask |= normalTextureZDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public  Texture2D  NormalMapW
        {
            get { return normalTextureW; }
            set { if (normalTextureW != value) { normalTextureW = value; DirtyMask |= normalTextureWDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColorX
        {
            get { return diffuseColorX; }
            set { if (diffuseColorX != value) { diffuseColorX = value; DirtyMask |= diffuseColorXDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColorY
        {
            get { return diffuseColorY; }
            set { if (diffuseColorY != value) { diffuseColorY = value; DirtyMask |= diffuseColorYDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColorZ
        {
            get { return diffuseColorZ; }
            set { if (diffuseColorZ != value) { diffuseColorZ = value; DirtyMask |= diffuseColorZDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColorW
        {
            get { return diffuseColorW; }
            set { if (diffuseColorW != value) { diffuseColorW = value; DirtyMask |= diffuseColorWDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColorX
        {
            get { return specularColorX; }
            set { if (specularColorX != value) { specularColorX = value; DirtyMask |= specularColorXDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColorY
        {
            get { return specularColorY; }
            set { if (specularColorY != value) { specularColorY = value; DirtyMask |= specularColorYDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColorZ
        {
            get { return specularColorZ; }
            set { if (specularColorZ != value) { specularColorZ = value; DirtyMask |= specularColorZDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColorW
        {
            get { return specularColorW; }
            set { if (specularColorW != value) { specularColorW = value; DirtyMask |= specularColorWDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public float SpecularPowerX
        {
            get { return specularPowerX; }
            set { if (specularPowerX != value) { specularPowerX = value; DirtyMask |= specularPowerXDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public float SpecularPowerY
        {
            get { return specularPowerY; }
            set { if (specularPowerY != value) { specularPowerY = value; DirtyMask |= specularPowerYDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public float SpecularPowerZ
        {
            get { return specularPowerZ; }
            set { if (specularPowerZ != value) { specularPowerZ = value; DirtyMask |= specularPowerZDirtyMask; } }
        }

        [ContentSerializer(Optional = true)]
        public float SpecularPowerW
        {
            get { return specularPowerW; }
            set { if (specularPowerW != value) { specularPowerW = value; DirtyMask |= specularPowerWDirtyMask; } }
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

        public override bool IsMaterial { get { return true; } }

        public SplatterTextureEffectPart()
        {
            DirtyMask |= splatterTextureDirtyMask;
            DirtyMask |= maskDirtyMask;
            SplatterTextureScale = Vector2.One;
            DiffuseColorX = DiffuseColorY = DiffuseColorZ = DiffuseColorW = Vector3.One;
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

            if ((DirtyMask & normalTextureXDirtyMask) != 0)
            {
                if (normalTextureXParameter == null)
                    normalTextureXParameter = GetParameter("NormalMapX");
                normalTextureXParameter.SetValue(normalTextureX);
                DirtyMask &= ~normalTextureXDirtyMask;
            }

            if ((DirtyMask & normalTextureYDirtyMask) != 0)
            {
                if (normalTextureYParameter == null)
                    normalTextureYParameter = GetParameter("NormalMapY");
                normalTextureYParameter.SetValue(normalTextureY);
                DirtyMask &= ~normalTextureYDirtyMask;
            }

            if ((DirtyMask & normalTextureZDirtyMask) != 0)
            {
                if (normalTextureZParameter == null)
                    normalTextureZParameter = GetParameter("NormalMapZ");
                normalTextureZParameter.SetValue(normalTextureZ);
                DirtyMask &= ~normalTextureZDirtyMask;
            }

            if ((DirtyMask & normalTextureWDirtyMask) != 0)
            {
                if (normalTextureWParameter == null)
                    normalTextureWParameter = GetParameter("NormalMapW");
                normalTextureWParameter.SetValue(normalTextureW);
                DirtyMask &= ~normalTextureWDirtyMask;
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

            if ((DirtyMask & diffuseColorXDirtyMask) != 0)
            {
                if (diffuseColorXParameter == null)
                    diffuseColorXParameter = GetParameter("DiffuseColorX");
                diffuseColorXParameter.SetValue(diffuseColorX);
                DirtyMask &= ~diffuseColorXDirtyMask;
            }

            if ((DirtyMask & diffuseColorYDirtyMask) != 0)
            {
                if (diffuseColorYParameter == null)
                    diffuseColorYParameter = GetParameter("DiffuseColorY");
                diffuseColorYParameter.SetValue(diffuseColorY);
                DirtyMask &= ~diffuseColorYDirtyMask;
            }

            if ((DirtyMask & diffuseColorZDirtyMask) != 0)
            {
                if (diffuseColorZParameter == null)
                    diffuseColorZParameter = GetParameter("DiffuseColorZ");
                diffuseColorZParameter.SetValue(diffuseColorZ);
                DirtyMask &= ~diffuseColorZDirtyMask;
            }

            if ((DirtyMask & diffuseColorWDirtyMask) != 0)
            {
                if (diffuseColorWParameter == null)
                    diffuseColorWParameter = GetParameter("DiffuseColorW");
                diffuseColorWParameter.SetValue(diffuseColorW);
                DirtyMask &= ~diffuseColorWDirtyMask;
            }
        
            if ((DirtyMask & specularColorXDirtyMask) != 0)
            {
                if (specularColorXParameter == null)
                    specularColorXParameter = GetParameter("SpecularColorX");
                specularColorXParameter.SetValue(specularColorX);
                DirtyMask &= ~specularColorXDirtyMask;
            }

            if ((DirtyMask & specularColorYDirtyMask) != 0)
            {
                if (specularColorYParameter == null)
                    specularColorYParameter = GetParameter("SpecularColorY");
                specularColorYParameter.SetValue(specularColorY);
                DirtyMask &= ~specularColorYDirtyMask;
            }

            if ((DirtyMask & specularColorZDirtyMask) != 0)
            {
                if (specularColorZParameter == null)
                    specularColorZParameter = GetParameter("SpecularColorZ");
                specularColorZParameter.SetValue(specularColorZ);
                DirtyMask &= ~specularColorZDirtyMask;
            }

            if ((DirtyMask & specularColorWDirtyMask) != 0)
            {
                if (specularColorWParameter == null)
                    specularColorWParameter = GetParameter("SpecularColorW");
                specularColorWParameter.SetValue(specularColorW);
                DirtyMask &= ~specularColorWDirtyMask;
            }



            if ((DirtyMask & specularPowerXDirtyMask) != 0)
            {
                if (specularPowerXParameter == null)
                    specularPowerXParameter = GetParameter("SpecularPowerX");
                specularPowerXParameter.SetValue(specularPowerX);
                DirtyMask &= ~specularPowerXDirtyMask;
            }

            if ((DirtyMask & specularPowerYDirtyMask) != 0)
            {
                if (specularPowerYParameter == null)
                    specularPowerYParameter = GetParameter("SpecularPowerY");
                specularPowerYParameter.SetValue(specularPowerY);
                DirtyMask &= ~specularPowerYDirtyMask;
            }

            if ((DirtyMask & specularPowerZDirtyMask) != 0)
            {
                if (specularPowerZParameter == null)
                    specularPowerZParameter = GetParameter("SpecularPowerZ");
                specularPowerZParameter.SetValue(specularPowerZ);
                DirtyMask &= ~specularPowerZDirtyMask;
            }

            if ((DirtyMask & specularPowerWDirtyMask) != 0)
            {
                if (specularPowerWParameter == null)
                    specularPowerWParameter = GetParameter("SpecularPowerW");
                specularPowerWParameter.SetValue(specularPowerW);
                DirtyMask &= ~specularPowerWDirtyMask;
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
            effectPart.NormalMapX = NormalMapX;
            effectPart.NormalMapY = NormalMapY;
            effectPart.NormalMapZ = NormalMapZ;
            effectPart.NormalMapW = NormalMapW;
            effectPart.DiffuseColorX = DiffuseColorX;
            effectPart.DiffuseColorY = DiffuseColorY;
            effectPart.DiffuseColorZ = DiffuseColorZ;
            effectPart.DiffuseColorW = DiffuseColorW;
            effectPart.SpecularColorX = SpecularColorX;
            effectPart.SpecularColorY = SpecularColorY;
            effectPart.SpecularColorZ = SpecularColorZ;
            effectPart.SpecularColorW = SpecularColorW;
            effectPart.SpecularPowerX = SpecularPowerX;
            effectPart.SpecularPowerY = SpecularPowerY;
            effectPart.SpecularPowerZ = SpecularPowerZ;
            effectPart.SpecularPowerW = SpecularPowerW;
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
                NormalMapX = NormalMapX,
                NormalMapY = NormalMapY,
                NormalMapZ = NormalMapZ,
                NormalMapW = NormalMapW,
                DiffuseColorX = DiffuseColorX,
                DiffuseColorY = DiffuseColorY,
                DiffuseColorZ = DiffuseColorZ,
                DiffuseColorW = DiffuseColorW,
                SpecularColorX = SpecularColorX,
                SpecularColorY = SpecularColorY,
                SpecularColorZ = SpecularColorZ,
                SpecularColorW = SpecularColorW,
                SpecularPowerX = SpecularPowerX,
                SpecularPowerY = SpecularPowerY,
                SpecularPowerZ = SpecularPowerZ,
                SpecularPowerW = SpecularPowerW,
                SplatterTexture = SplatterTexture,
                SplatterTextureScale = SplatterTextureScale,
            };
        }
    }

#endif
}
