namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class AlphaTestMaterialPart : MaterialPart
    {
        private EffectParameter alphaTestParameter;

        public CompareFunction AlphaFunction
        {
            get { return alphaFunction; }
            set { if (alphaFunction != value) { alphaFunction = value; NotifyShaderChanged(); } }
        }
        private CompareFunction alphaFunction = MaterialConstants.AlphaFunction;

        public int ReferenceAlpha { get; set; }

        protected internal override void OnBind()
        {
            if ((alphaTestParameter = GetParameter("AlphaTest")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            Vector4 alphaTest = new Vector4();

            // Convert reference alpha from 8 bit integer to 0-1 float format.
            float reference = (float)ReferenceAlpha / 255f;

            // Comparison tolerance of half the 8 bit integer precision.
            const float threshold = 0.5f / 255f;

            switch (alphaFunction)
            {
                case CompareFunction.Less:
                    // Shader will evaluate: clip((a < x) ? z : w)
                    alphaTest.X = reference - threshold;
                    alphaTest.Z = 1;
                    alphaTest.W = -1;
                    break;

                case CompareFunction.LessEqual:
                    // Shader will evaluate: clip((a < x) ? z : w)
                    alphaTest.X = reference + threshold;
                    alphaTest.Z = 1;
                    alphaTest.W = -1;
                    break;

                case CompareFunction.GreaterEqual:
                    // Shader will evaluate: clip((a < x) ? z : w)
                    alphaTest.X = reference - threshold;
                    alphaTest.Z = -1;
                    alphaTest.W = 1;
                    break;

                case CompareFunction.Greater:
                    // Shader will evaluate: clip((a < x) ? z : w)
                    alphaTest.X = reference + threshold;
                    alphaTest.Z = -1;
                    alphaTest.W = 1;
                    break;

                case CompareFunction.Equal:
                    // Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
                    alphaTest.X = reference;
                    alphaTest.Y = threshold;
                    alphaTest.Z = 1;
                    alphaTest.W = -1;
                    break;

                case CompareFunction.NotEqual:
                    // Shader will evaluate: clip((abs(a - x) < Y) ? z : w)
                    alphaTest.X = reference;
                    alphaTest.Y = threshold;
                    alphaTest.Z = -1;
                    alphaTest.W = 1;
                    break;

                case CompareFunction.Never:
                    // Shader will evaluate: clip((a < x) ? z : w)
                    alphaTest.Z = -1;
                    alphaTest.W = -1;
                    break;

                case CompareFunction.Always:
                default:
                    // Shader will evaluate: clip((a < x) ? z : w)
                    alphaTest.Z = 1;
                    alphaTest.W = 1;
                    break;
            }
            alphaTestParameter.SetValue(alphaTest);
        }

        protected internal override MaterialPart Clone()
        {
            return new AlphaTestMaterialPart() { alphaFunction = this.alphaFunction, ReferenceAlpha = this.ReferenceAlpha };
        }

        protected internal override void OnResolveMaterialPart(MaterialUsage usage, MaterialPart existingInstance)
        {
            var part = ((AlphaTestMaterialPart)existingInstance);
            part.ReferenceAlpha = ReferenceAlpha;
            part.alphaFunction = alphaFunction;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            // When using alpha test with transparent materials, don't write to depth.
            if (MaterialGroup.IsTransparent && (usage == MaterialUsage.Depth || usage == MaterialUsage.DepthAndNormal))
                return null;

            bool eqne = (alphaFunction == CompareFunction.Equal || alphaFunction == CompareFunction.NotEqual);
            return GetShaderCode("AlphaTest").Replace("{$EQNE}", eqne ? "" : "//").Replace("{$LTGT}", eqne ? "//" : "");
        }
    }
}
