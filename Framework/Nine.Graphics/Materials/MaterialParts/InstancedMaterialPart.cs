namespace Nine.Graphics.Materials.MaterialParts
{
    /// <summary>
    /// Defines a material part that is used for hardware instancing.
    /// </summary>
    public class InstancedMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {

        }

        protected internal override MaterialPart Clone()
        {
            return new InstancedMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return null;
        }
    }
}
