namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class ColorMatrixMaterialPart : MaterialPart
    {   
        public Matrix ColorMatrix 
        {
            get { return colorMatrix; }
            set { colorMatrix = value; }
        }
        private Matrix colorMatrix = Matrix.Identity;

        private EffectParameter transformParameter;

        protected internal override void OnBind()
        {
            if ((transformParameter = GetParameter("Transform")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            transformParameter.SetValue(colorMatrix);
        }

        protected internal override MaterialPart Clone()
        {
            return new ColorMatrixMaterialPart() { colorMatrix = this.colorMatrix };
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("ColorMatrix") : null;
        }
    }
}
