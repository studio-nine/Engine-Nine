namespace Nine.Graphics.Materials.MaterialParts
{
    class EndLightMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {
            MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override MaterialPart Clone()
        {
            return new EndLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("EndLight") : null;
        }
    }
}
