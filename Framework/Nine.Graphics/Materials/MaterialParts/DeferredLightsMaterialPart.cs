namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class DeferredLightsMaterialPart : MaterialPart
    {
        int lightBufferIndex;
        EffectParameter lightBufferParameter;
        EffectParameter halfPixelParameter;
        
        protected internal override void OnBind()
        {
            GetTextureParameter("LightTexture", out lightBufferParameter, out lightBufferIndex);
            halfPixelParameter = GetParameter("HalfPixel");
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (lightBufferParameter != null)
            {
                halfPixelParameter.SetValue(context.HalfPixel);
                lightBufferParameter.SetValue(context.textures[TextureUsage.LightBuffer]);
                context.graphics.SamplerStates[lightBufferIndex] = SamplerState.PointClamp;
            }
        }

        protected internal override void EndApplyLocalParameters(DrawingContext context)
        {
            if (lightBufferParameter != null)
                context.graphics.SamplerStates[lightBufferIndex] = context.SamplerState;
        }

        protected internal override void GetDependentPasses(ICollection<Type> passTypes)
        {
            passTypes.Add(typeof(LightPrePass));
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
