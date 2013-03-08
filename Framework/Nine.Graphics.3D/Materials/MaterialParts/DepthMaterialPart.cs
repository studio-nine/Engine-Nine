namespace Nine.Graphics.Materials.MaterialParts
{
    class DepthMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {
            MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override MaterialPart Clone()
        {
            return new DepthMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Depth ? GetShaderCode("Depth") : null;
        }
    }
}
