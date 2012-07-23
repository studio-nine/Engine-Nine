namespace Nine.Graphics.Materials.MaterialParts
{
    using Nine.Graphics.Drawing;
    using Microsoft.Xna.Framework.Graphics;

    [ContentSerializable]
    class BeginLightMaterialPart : MaterialPart
    {
        private int ambientLightVersion;
        private EffectParameter eyePositionParameter;
        private EffectParameter ambientLightColorParameter;

        protected internal override void OnBind()
        {
            if ((eyePositionParameter = GetParameter("EyePosition")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((ambientLightColorParameter = GetParameter("AmbientLightColor")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            var ambientLight = context.AmbientLight;
            if (ambientLight.version != ambientLightVersion)
            {
                ambientLightColorParameter.SetValue(ambientLight.Value);
                ambientLightVersion = ambientLight.version;
            }
            eyePositionParameter.SetValue(context.EyePosition);
        }

        protected internal override MaterialPart Clone()
        {
            return new BeginLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("BeginLight") : null;
        }
    }
}
