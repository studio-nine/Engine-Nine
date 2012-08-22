namespace Nine.Graphics.Materials.MaterialParts
{
    using Nine.Graphics.Drawing;
    using Microsoft.Xna.Framework.Graphics;

    class BeginLightMaterialPart : MaterialPart
    {
        private EffectParameter eyePositionParameter;
        private EffectParameter ambientLightColorParameter;

        protected internal override void OnBind()
        {
            eyePositionParameter = GetParameter("EyePosition");
            ambientLightColorParameter = GetParameter("AmbientLightColor");
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (eyePositionParameter != null)
            {
                ambientLightColorParameter.SetValue(context.ambientLightColor);
                eyePositionParameter.SetValue(context.CameraPosition);
            }
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
