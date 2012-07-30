namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;
    using Microsoft.Xna.Framework;

    [ContentSerializable]
    public class DeferredLightsMaterialPart : MaterialPart
    {
        EffectParameter halfPixelParameter;
        EffectParameter lightBufferParameter;
        
        protected internal override void OnBind()
        {
            if ((halfPixelParameter = GetParameter("HalfPixel")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((lightBufferParameter = GetParameter("LightTexture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            var lightBuffer = context.textures[TextureUsage.LightBuffer] as Texture2D;
            lightBufferParameter.SetValue(lightBuffer);
            if (lightBuffer != null)
            {
                var halfPixel = new Vector2();
                halfPixel.X = 0.5f / lightBuffer.Width;
                halfPixel.Y = 0.5f / lightBuffer.Height;
                halfPixelParameter.SetValue(halfPixel);
            }
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
