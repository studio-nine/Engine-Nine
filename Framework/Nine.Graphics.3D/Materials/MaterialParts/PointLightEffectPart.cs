namespace Nine.Graphics.Materials.MaterialParts
{





#if !WINDOWS_PHONE
    /*
    internal class PointLightEffectPart : MaterialPart, IPointLight
    {
        private uint DirtyMask = 0;
        
        private Vector3 position;
        private EffectParameter positionParameter;
        private const uint positionDirtyMask = 1 << 0;

        private Vector3 diffuseColor;
        private EffectParameter diffuseColorParameter;
        private const uint diffuseColorDirtyMask = 1 << 1;

        private Vector3 specularColor;
        private EffectParameter specularColorParameter;
        private const uint specularColorDirtyMask = 1 << 2;

        private float range;
        private EffectParameter rangeParameter;
        private const uint rangeDirtyMask = 1 << 3;

        private float attenuation;
        private EffectParameter attenuationParameter;
        private const uint attenuationDirtyMask = 1 << 4;
        
        [Nine.Serialization.ContentSerializable]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; DirtyMask |= positionDirtyMask; }
        }

        [Nine.Serialization.ContentSerializable]
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; DirtyMask |= diffuseColorDirtyMask; }
        }

        [Nine.Serialization.ContentSerializable]
        public Vector3 SpecularColor
        {
            get { return specularColor; }
            set { specularColor = value; DirtyMask |= specularColorDirtyMask; }
        }

        [Nine.Serialization.ContentSerializable]
        public float Range
        {
            get { return range; }
            set { range = value; DirtyMask |= rangeDirtyMask; }
        }

        [Nine.Serialization.ContentSerializable]
        public float Attenuation
        {
            get { return attenuation; }
            set { attenuation = value; DirtyMask |= attenuationDirtyMask; }
        }

        public PointLightEffectPart()
        {
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
            Range = 10;
            Attenuation = MathHelper.E;

            positionParameter = GetParameter("PointLightPosition");
            diffuseColorParameter = GetParameter("PointLightDiffuseColor");
            specularColorParameter = GetParameter("PointLightSpecularColor");
            rangeParameter = GetParameter("Range");
            attenuationParameter = GetParameter("Attenuation");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & positionDirtyMask) != 0)
            {
                if (positionParameter != null)
                    positionParameter.SetValue(position);
                DirtyMask &= ~positionDirtyMask;
            }

            if ((DirtyMask & diffuseColorDirtyMask) != 0)
            {
                if (diffuseColorParameter != null)
                    diffuseColorParameter.SetValue(diffuseColor);
                DirtyMask &= ~diffuseColorDirtyMask;
            }

            if ((DirtyMask & specularColorDirtyMask) != 0)
            {
                if (specularColorParameter != null)
                    specularColorParameter.SetValue(specularColor);
                DirtyMask &= ~specularColorDirtyMask;
            }

            if ((DirtyMask & rangeDirtyMask) != 0)
            {
                if (rangeParameter != null)
                    rangeParameter.SetValue(range);
                DirtyMask &= ~rangeDirtyMask;
            }

            if ((DirtyMask & attenuationDirtyMask) != 0)
            {
                if (attenuationParameter != null)
                    attenuationParameter.SetValue(attenuation);
                DirtyMask &= ~attenuationDirtyMask;
            }
        }

        protected internal override MaterialPart Clone()
        {
            return new PointLightEffectPart()
            {
                Position = this.Position,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                Range = this.Range,
                Attenuation = this.Attenuation,
            };
        }
    }
    */
#endif
}
