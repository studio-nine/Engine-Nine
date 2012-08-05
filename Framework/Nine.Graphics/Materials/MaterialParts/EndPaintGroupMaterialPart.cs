namespace Nine.Graphics.Materials.MaterialParts
{
    class EndPaintGroupMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {
            MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override MaterialPart Clone()
        {
            return new EndPaintGroupMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return GetShaderCode("EndPaintGroup");
        }
    }
}
