namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class DualTextureMaterialPart : MaterialPart
    {
        private EffectParameter textureParameter;

        public Texture2D Texture2 { get; set; }

        protected internal override void OnBind()
        {
            if ((textureParameter = GetParameter("Texture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            textureParameter.SetValue(Texture2);
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Dual)
                Texture2 = texture as Texture2D;
        }

        protected internal override MaterialPart Clone()
        {
            return new DualTextureMaterialPart() { Texture2 = this.Texture2 };
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("DualTexture") : null;
        }
    }
}
