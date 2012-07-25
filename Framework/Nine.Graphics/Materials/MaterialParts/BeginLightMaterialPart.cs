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
            if (ambientLightVersion != context.ambientLightColorVersion)
            {
                ambientLightColorParameter.SetValue(context.ambientLightColor);
                ambientLightVersion = context.ambientLightColorVersion;
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
