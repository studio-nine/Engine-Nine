namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class TextureTransformMaterialPart : MaterialPart
    {
        public Matrix TextureTransform 
        {
            get { return transform; }
            set { transform = value; }
        }
        private Matrix transform = Matrix.Identity;

        private EffectParameter textureTransformParameter;

        protected internal override void OnBind()
        {
            textureTransformParameter = GetParameter("TextureTransform");
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (textureTransformParameter != null)
                textureTransformParameter.SetValue(Nine.Graphics.TextureTransform.ToArray(transform));
        }

        protected internal override MaterialPart Clone()
        {
            return new TextureTransformMaterialPart() { TextureTransform = TextureTransform };
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("TextureTransform") : null;
        }
    }
}
