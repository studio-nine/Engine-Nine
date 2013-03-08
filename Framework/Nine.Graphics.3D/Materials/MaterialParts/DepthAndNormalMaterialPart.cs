namespace Nine.Graphics.Materials.MaterialParts
{
    class DepthAndNormalMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {
            MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override MaterialPart Clone()
        {
            return new DepthAndNormalMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.DepthAndNormal ? GetShaderCode("DepthAndNormal") : null;
        }
    }
}
