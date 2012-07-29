namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    static class MaterialConstants
    {
        public const float Alpha = 1;
        public const float SpecularPower = 16;
        public const float FresnelFactor = 0;
        public const float EnvironmentMapAmount = 1;
        public const float SoftParticleFade = 1;
        public const int WeightsPerVertex = 4;
        public const int ReferenceAlpha = 128;
        public const bool FogEnabled = true;
        public const float FogStart = 1000;
        public const float FogEnd = 10000;
        public const float BlurAmount = 2;

        public readonly static Vector3 DiffuseColor = Vector3.One;
        public readonly static Vector4 DiffuseColor4 = Vector4.One;
        public readonly static Vector3 EmissiveColor = Vector3.Zero;
        public readonly static Vector3 SpecularColor = Vector3.Zero;
        public readonly static Vector3 EnvironmentMapSpecular = Vector3.Zero;
        public readonly static CompareFunction AlphaFunction = CompareFunction.Greater;
        public readonly static Vector3 FogColor = Vector3.One;
    }
}