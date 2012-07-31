namespace Nine.Graphics.Materials.MaterialParts
{
    [ContentSerializable]
    class DepthMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {

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
