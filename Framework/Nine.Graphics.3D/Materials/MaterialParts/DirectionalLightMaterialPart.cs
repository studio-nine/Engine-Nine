namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class DirectionalLightMaterialPart : MaterialPart
    {
        private int lightIndex;
        private int lightVersion;
        private int lightCollectionVersion;

        private EffectParameter directionParameter;
        private EffectParameter diffuseColorParameter;
        private EffectParameter specularColorParameter;

        protected internal override void OnBind()
        {
            directionParameter = GetParameter("DirLightDirection");
            diffuseColorParameter = GetParameter("DirLightDiffuseColor");
            specularColorParameter = GetParameter("DirLightSpecularColor");

            foreach (var part in MaterialGroup.MaterialParts)
            {
                if (part is DirectionalLightMaterialPart)
                {
                    if (part == this)
                        break;
                    lightIndex++;
                }
            }
        }

        protected internal override void GetDependentParts(MaterialUsage usage, IList<Type> result)
        {
            result.Add(typeof(EndLightMaterialPart));
        }

        protected internal override void ApplyGlobalParameters(DrawingContext3D context)
        {
            if (directionParameter == null)
                return;

            var light = context.directionalLights[lightIndex];

            directionParameter.SetValue(light.Direction);
            diffuseColorParameter.SetValue(light.DiffuseColor);
            if (specularColorParameter != null)
                specularColorParameter.SetValue(light.SpecularColor);
        }

        protected internal override MaterialPart Clone()
        {
            return new DirectionalLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("DirectionalLight") : null;
        }
    }
}
