namespace Nine.Graphics.Materials.MaterialParts
{




    /*
    public class SpotLightEffectPart : MaterialPart, ISpotLight
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

        private float innerAngle;
        private EffectParameter innerAngleParameter;
        private const uint innerAngleDirtyMask = 1 << 5;

        private float outerAngle;
        private EffectParameter outerAngleParameter;
        private const uint outerAngleDirtyMask = 1 << 6;

        private float falloff;
        private EffectParameter falloffParameter;
        private const uint falloffDirtyMask = 1 << 7;

        private Vector3 direction;
        private EffectParameter directionParameter;
        private const uint directionDirtyMask = 1 << 8;

        [Nine.Serialization.ContentSerializable]
        public Vector3 Position
        {
            get { return position; }
            set { position = value; DirtyMask |= positionDirtyMask; }
        }

        [Nine.Serialization.ContentSerializable]
        public Vector3 Direction
        {
            get { return direction; }
            set { direction = value; DirtyMask |= directionDirtyMask; }
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

        [Nine.Serialization.ContentSerializable]
        public float InnerAngle
        {
            get { return innerAngle; }
            set { innerAngle = value; DirtyMask |= innerAngleDirtyMask; }
        }

        [Nine.Serialization.ContentSerializable]
        public float OuterAngle
        {
            get { return outerAngle; }
            set { outerAngle = value; DirtyMask |= outerAngleDirtyMask; }
        }

        [Nine.Serialization.ContentSerializable]
        public float Falloff
        {
            get { return falloff; }
            set { falloff = value; DirtyMask |= falloffDirtyMask; }
        }

        public SpotLightEffectPart()
        {
            Direction = new Vector3(0, -0.707107f, -0.707107f);
            DiffuseColor = Vector3.One;
            SpecularColor = Vector3.Zero;
            Range = 100;
            Attenuation = MathHelper.E;
            InnerAngle = MathHelper.PiOver4;
            OuterAngle = MathHelper.PiOver2;
            Falloff = 1;

            positionParameter = GetParameter("SpotLightPosition");
            directionParameter = GetParameter("SpotLightDirection");
            diffuseColorParameter = GetParameter("SpotLightDiffuseColor");
            specularColorParameter = GetParameter("SpotLightSpecularColor");
            rangeParameter = GetParameter("Range");
            attenuationParameter = GetParameter("Attenuation");
            innerAngleParameter = GetParameter("InnerAngle");
            outerAngleParameter = GetParameter("OuterAngle");
            falloffParameter = GetParameter("Falloff");
        }

        protected internal override void OnApply()
        {
            if ((DirtyMask & positionDirtyMask) != 0)
            {
                if (positionParameter != null)
                    positionParameter.SetValue(position);
                DirtyMask &= ~positionDirtyMask;
            }

            if ((DirtyMask & directionDirtyMask) != 0)
            {
                if (directionParameter != null)
                    directionParameter.SetValue(direction);
                DirtyMask &= ~directionDirtyMask;
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

            if ((DirtyMask & innerAngleDirtyMask) != 0)
            {
                if (innerAngleParameter != null)
                    innerAngleParameter.SetValue((float)Math.Cos(innerAngle * 0.5));
                DirtyMask &= ~innerAngleDirtyMask;
            }

            if ((DirtyMask & outerAngleDirtyMask) != 0)
            {
                if (outerAngleParameter != null)
                    outerAngleParameter.SetValue((float)Math.Cos(outerAngle * 0.5));
                DirtyMask &= ~outerAngleDirtyMask;
            }

            if ((DirtyMask & falloffDirtyMask) != 0)
            {
                if (falloffParameter != null)
                    falloffParameter.SetValue(falloff);
                DirtyMask &= ~falloffDirtyMask;
            }
        }

        protected internal override MaterialPart Clone()
        {
            return new SpotLightEffectPart()
            {
                Position = this.Position,
                DiffuseColor = this.DiffuseColor,
                SpecularColor = this.SpecularColor,
                Range = this.Range,
                Attenuation = this.Attenuation,
                InnerAngle = this.InnerAngle,
                OuterAngle = this.OuterAngle,
                Falloff = this.Falloff,
            };
        }
    }
     */
}
