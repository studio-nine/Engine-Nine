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
            result.Add(typeof(BeginLightMaterialPart));
            result.Add(typeof(EndLightMaterialPart));
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (directionParameter == null)
                return;

            var light = context.DirectionalLights[lightIndex];
            if (light != null && light.version != lightVersion)
            {
                directionParameter.SetValue(light.Direction);
                diffuseColorParameter.SetValue(light.DiffuseColor);
                specularColorParameter.SetValue(light.SpecularColor);
            }

            if (lightCollectionVersion != lightVersion && light == null)
            {
                directionParameter.SetValue(Vector3.Zero);
                directionParameter.SetValue(Vector3.Zero);
            }
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
