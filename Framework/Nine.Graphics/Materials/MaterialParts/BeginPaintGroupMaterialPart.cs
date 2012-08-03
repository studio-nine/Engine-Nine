namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    class BeginPaintGroupMaterialPart : MaterialPart
    {
        private EffectParameter maskTextureScaleParameter;
        private EffectParameter maskTexture0Parameter;
        private EffectParameter maskTexture1Parameter;

        private Texture2D maskTexture0;
        private Texture2D maskTexture1;
        private Vector2 maskTextureScale;

        protected internal override void OnBind()
        {
            maskTextureScaleParameter = GetParameter("MaskTextureScale");
            maskTexture0Parameter = GetParameter("MaskTexture0");
            maskTexture1Parameter = GetParameter("MaskTexture1");

            var maskTextures = MaterialPaintGroup.GetMaskTextures(MaterialGroup);
            maskTexture0 = maskTextures != null && maskTextures.Count > 0 ? maskTextures[0] as Texture2D : null;
            maskTexture1 = maskTextures != null && maskTextures.Count > 1 ? maskTextures[1] as Texture2D : null;
            maskTextureScale = MaterialPaintGroup.GetMaskTextureScale(MaterialGroup);

            maskTextureScale.X = 1.0f / maskTextureScale.X;
            maskTextureScale.Y = 1.0f / maskTextureScale.Y;
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (maskTextureScaleParameter != null)
            {
                maskTextureScaleParameter.SetValue(maskTextureScale);
                maskTexture0Parameter.SetValue(maskTexture0);
                maskTexture1Parameter.SetValue(maskTexture1);
            }
        }

        protected internal override MaterialPart Clone()
        {
            return new BeginLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("BeginPaintGroup") : null;
        }
    }
}
