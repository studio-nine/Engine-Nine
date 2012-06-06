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
    public class FogMaterialPart : MaterialPart
    {
        private int fogVersion;
        private EffectParameter fogColorParameter;
        private EffectParameter fogVectorParameter;

        protected internal override void OnBind()
        {
            if ((fogColorParameter = GetParameter("FogColor")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((fogVectorParameter = GetParameter("FogVector")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (fogVersion != context.Fog.version)
            {
                fogColorParameter.SetValue(context.Fog.Value.FogColor);
                fogVersion = context.Fog.version;
            }
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            var fog = context.Fog.Value;
            Matrix worldView = Matrix.Multiply(material.World, context.View);
            SetFogVector(ref worldView, fog.FogStart, fog.FogEnd, fogVectorParameter);
        }

        protected internal override MaterialPart Clone()
        {
            return new FogMaterialPart();
        }
        
        /// <summary>
        /// Sets a vector which can be dotted with the object space vertex position to compute fog amount.
        /// </summary>
        static void SetFogVector(ref Matrix worldView, float fogStart, float fogEnd, EffectParameter fogVectorParam)
        {
            if (fogStart == fogEnd)
            {
                // Degenerate case: force everything to 100% fogged if start and end are the same.
                fogVectorParam.SetValue(new Vector4(0, 0, 0, 1));
            }
            else
            {
                // We want to transform vertex positions into view space, take the resulting
                // Z value, then scale and offset according to the fog start/end distances.
                // Because we only care about the Z component, the shader can do all this
                // with a single dot product, using only the Z row of the world+view matrix.

                float scale = 1f / (fogStart - fogEnd);

                Vector4 fogVector = new Vector4();

                fogVector.X = worldView.M13 * scale;
                fogVector.Y = worldView.M23 * scale;
                fogVector.Z = worldView.M33 * scale;
                fogVector.W = (worldView.M43 + fogStart) * scale;

                fogVectorParam.SetValue(fogVector);
            }
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("Fog") : null;
        }
    }
}
