namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
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
            if ((textureTransformParameter = GetParameter("TextureTransform")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
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
