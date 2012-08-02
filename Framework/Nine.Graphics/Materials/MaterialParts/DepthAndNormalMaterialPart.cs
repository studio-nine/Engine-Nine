namespace Nine.Graphics.Materials.MaterialParts
{
    [ContentSerializable]
    class DepthAndNormalMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {

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
