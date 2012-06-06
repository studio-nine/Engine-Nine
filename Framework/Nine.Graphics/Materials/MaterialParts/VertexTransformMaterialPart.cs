#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Materials.MaterialParts
{
    [ContentSerializable]
    class VertexTransformMaterialPart : MaterialPart
    {
        private EffectParameter worldParameter;
        private EffectParameter worldViewProjectionParameter;
        private EffectParameter worldInverseTransposeParameter;

        protected internal override void OnBind()
        {
            if ((worldParameter = GetParameter("World")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((worldViewProjectionParameter = GetParameter("WorldViewProjection")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((worldInverseTransposeParameter = GetParameter("WorldInverseTranspose")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (worldParameter != null)
                worldParameter.SetValue(material.world);
            if (worldInverseTransposeParameter != null)
            {
                Matrix worldInverse;
                Matrix.Invert(ref material.world, out worldInverse);
                worldInverseTransposeParameter.SetValueTranspose(worldInverse);
            }
            worldViewProjectionParameter.SetValue(material.World * context.Matrices.ViewProjection);
        }

        protected internal override MaterialPart Clone()
        {
            return new VertexTransformMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("VertexTransform") : null;
        }
    }
}
