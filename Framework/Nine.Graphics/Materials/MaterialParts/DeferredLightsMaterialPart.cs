namespace Nine.Graphics.Materials.MaterialParts
{




    /*
    class DeferredLightsMaterialPart : MaterialPart
    {
        private EffectParameter halfPixelParameter;
        
        private Texture lightTexture;
        private EffectParameter lightTextureParameter;
        
        private Vector3 diffuseColor;
        private EffectParameter diffuseColorParameter;

        private Vector3 emissiveColor;
        private EffectParameter emissiveColorParameter;
        
        private Vector3 specularColor;
        private EffectParameter specularColorParameter;

        private float specularPower;
        private EffectParameter specularPowerParameter;

        [ContentSerializer(Optional = true)]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; DirtyMask |= diffuseColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 EmissiveColor
        {
            get { return emissiveColor; }
            set { emissiveColor = value; DirtyMask |= emissiveColorDirtyMask; }
        }

        [ContentSerializer(Optional = true)]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; DirtyMask |= specularColorDirtyMask; }
        }

        [ContentSerializerIgnore]
        public Texture LightTexture
        {
            get { return lightTexture; }
            set { if (lightTexture != value) { lightTexture = value; DirtyMask |= lightTextureDirtyMask; } }
        }

        [ContentSerializerIgnore]
        public float SpecularPower
        {
            get { return specularPower; }
            set { if (specularPower != value) { specularPower = value; DirtyMask |= specularPowerDirtyMask; } }
        }

        public DeferredLightsMaterialPart()
        {
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            SpecularColor = Vector3.Zero;
            SpecularPower = 16;

            diffuseColorParameter = GetParameter("DiffuseColor");
            emissiveColorParameter = GetParameter("EmissiveColor");
            specularColorParameter = GetParameter("SpecularColor");
            specularPowerParameter = GetParameter("SpecularPower");
            lightTextureParameter = GetParameter("LightTexture");
            halfPixelParameter = GetParameter("halfPixel");
        }

        protected internal override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.LightBuffer)
                LightTexture = texture as Texture2D;
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter != null)
                    diffuseColorParameter.SetValue(diffuseColor);
                DirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((DirtyMask & emissiveColorDirtyMask) != 0)
            {
                if (emissiveColorParameter != null)
                    emissiveColorParameter.SetValue(emissiveColor);
                DirtyMask &= ~emissiveColorDirtyMask;
            }

            if ((DirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter != null)
                    specularColorParameter.SetValue(specularColor);
                DirtyMask &= ~specularColorDirtyMask;
            }

            if ((DirtyMask & specularPowerDirtyMask) != 0)
            {
                if (specularPowerParameter != null)
                    specularPowerParameter.SetValue(specularPower);
                DirtyMask &= ~specularPowerDirtyMask;
            }

            if ((DirtyMask & lightTextureDirtyMask) != 0)
            {
                if (lightTextureParameter != null)
                    lightTextureParameter.SetValue(lightTexture);
                DirtyMask &= ~lightTextureDirtyMask;
            }

            if (halfPixelParameter != null)
                halfPixelParameter.SetValue(new Vector2(0.5f / GraphicsDevice.Viewport.Width, 0.5f / GraphicsDevice.Viewport.Height));
        }

        protected internal override void OnApply(MaterialPart part)
        {
            var effectPart = (DeferredLightsMaterialPart)part;
            effectPart.DiffuseColor = DiffuseColor;
            effectPart.EmissiveColor = EmissiveColor;
            effectPart.SpecularColor = SpecularColor;
            effectPart.LightTexture = LightTexture;
            effectPart.SpecularPower = SpecularPower;
        }

        protected internal override MaterialPart Clone()
        {
            return new DeferredLightsMaterialPart()
            {
                DiffuseColor = this.DiffuseColor,
                EmissiveColor = this.EmissiveColor,
                SpecularColor = this.SpecularColor,
                LightTexture = this.LightTexture,
                SpecularPower = this.SpecularPower,
            };
        }
    }
     */
}
