namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class DeferredLightsMaterialPart : MaterialPart
    {
        EffectParameter lightBufferParameter;
        
        protected internal override void OnBind()
        {
            if ((lightBufferParameter = GetParameter("LightTexture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            lightBufferParameter.SetValue(context.textures[TextureUsage.LightBuffer]);
        }

        protected internal override void GetDependentPasses(ICollection<Type> passTypes)
        {
            passTypes.Add(typeof(LightPrePass));
        }

        protected internal override void GetDependentParts(MaterialUsage usage, IList<Type> result)
        {
            result.Add(typeof(BeginLightMaterialPart));
            result.Add(typeof(EndLightMaterialPart));
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("DeferredLights") : null;
        }

        protected internal override MaterialPart Clone()
        {
            return new DeferredLightsMaterialPart();
        }
    }
}
