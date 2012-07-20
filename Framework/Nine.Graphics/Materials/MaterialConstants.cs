namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    static class MaterialConstants
    {
        public readonly static float Alpha = 1;
        public readonly static float SpecularPower = 16;
        public readonly static float FresnelFactor = 0;
        public readonly static float EnvironmentMapAmount = 1;
        public readonly static float SoftParticleFade = 1;
        public readonly static int WeightsPerVertex = 4;
        public readonly static int ReferenceAlpha = 128;
        public readonly static Vector3 DiffuseColor = Vector3.One;
        public readonly static Vector4 DiffuseColor4 = Vector4.One;
        public readonly static Vector3 EmissiveColor = Vector3.Zero;
        public readonly static Vector3 SpecularColor = Vector3.Zero;
        public readonly static Vector3 EnvironmentMapSpecular = Vector3.Zero;
        public readonly static CompareFunction AlphaFunction = CompareFunction.Greater;
        public readonly static bool FogEnabled = true;
        public readonly static float FogStart = 1000;
        public readonly static float FogEnd = 10000;
        public readonly static Vector3 FogColor = Vector3.One;
    }
}