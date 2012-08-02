namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    class VertexTransformMaterialPart : MaterialPart
    {
        private EffectParameter worldParameter;
        private EffectParameter viewProjectionParameter;
        private EffectParameter worldViewProjectionParameter;
        private EffectParameter worldInverseTransposeParameter;

        protected internal override void OnBind()
        {
            if ((worldParameter = GetParameter("World")) == null)
                MaterialGroup.MaterialParts.Remove(this);

            viewProjectionParameter = GetParameter("ViewProjection");
            worldViewProjectionParameter = GetParameter("WorldViewProjection");
            worldInverseTransposeParameter = GetParameter("WorldInverseTranspose");
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (viewProjectionParameter != null)
                viewProjectionParameter.SetValue(context.matrices.ViewProjection);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (worldParameter != null)
                worldParameter.SetValue(material.world);
            if (worldInverseTransposeParameter != null)
                worldViewProjectionParameter.SetValue(material.World * context.Matrices.ViewProjection);
            if (worldInverseTransposeParameter != null)
            {
                Matrix worldInverse;
                Matrix.Invert(ref material.world, out worldInverse);
                worldInverseTransposeParameter.SetValueTranspose(worldInverse);
            }
        }

        protected internal override void GetDependentParts(MaterialUsage usage, IList<Type> result)
        {
            if (usage == MaterialUsage.Depth)
                result.Add(typeof(DepthMaterialPart));
            if (usage == MaterialUsage.DepthAndNormal)
                result.Add(typeof(DepthAndNormalMaterialPart));
        }

        protected internal override MaterialPart Clone()
        {
            return new VertexTransformMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return GetShaderCode(MaterialGroup.MaterialParts.OfType<InstancedMaterialPart>().Any() ? "InstanceTransform" : "VertexTransform");
        }
    }
}
