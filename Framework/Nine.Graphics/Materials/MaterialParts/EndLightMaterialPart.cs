namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using System.Collections.Generic;

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

        protected internal override void GetDependentParts(MaterialUsage usage, IList<Type> result)
        {
            result.Add(typeof(BeginLightMaterialPart));
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("EndLight") : null;
        }
    }
}
