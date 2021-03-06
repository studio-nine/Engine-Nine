﻿namespace Nine.Graphics.Materials.MaterialParts
{
    using System.Linq;
    
    class TangentTransformMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {
            MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override MaterialPart Clone()
        {
            return new TangentTransformMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return (usage == MaterialUsage.Default || usage == MaterialUsage.DepthAndNormal || usage == MaterialUsage.Normal) ? 
                GetShaderCode("TangentTransform").Replace("{$SKINNED}", MaterialGroup != null && MaterialGroup.MaterialParts.Any(p => p is SkinnedMaterialPart) ? "" : "//")
                                                 .Replace("{$INSTANCED}", MaterialGroup != null && MaterialGroup.MaterialParts.Any(p => p is InstancedMaterialPart) ? "" : "//") : null;
        }
    }
}
