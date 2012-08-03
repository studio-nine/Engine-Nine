namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class FogMaterialPart : MaterialPart
    {
        private int fogVersion;
        private EffectParameter fogColorParameter;
        private EffectParameter fogVectorParameter;

        protected internal override void OnBind()
        {
            fogColorParameter = GetParameter("FogColor");
            fogVectorParameter = GetParameter("FogVector");
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (fogVersion != context.fogVersion && fogColorParameter != null)
            {
                fogColorParameter.SetValue(context.fogColor);
                fogVersion = context.fogVersion;
            }
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            Matrix worldView = Matrix.Multiply(material.World, context.View);
            SetFogVector(ref worldView, context.fogStart, context.fogEnd, fogVectorParameter);
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
