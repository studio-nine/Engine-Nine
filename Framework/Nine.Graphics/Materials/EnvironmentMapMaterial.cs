namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class EnvironmentMapMaterial : Material
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

        public float FresnelFactor
        {
            get { return fresnelFactor.HasValue ? fresnelFactor.Value : Constants.FresnelFactor; }
            set { fresnelFactor = (value == Constants.FresnelFactor ? (float?)null : value); }
        }
        private float? fresnelFactor;

        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : Constants.DiffuseColor; }
            set { diffuseColor = (value == Constants.DiffuseColor ? (Vector3?)null : value); }
        }
        private Vector3? diffuseColor;

        public Vector3 EmissiveColor
        {
            get { return emissiveColor.HasValue ? emissiveColor.Value : Constants.EmissiveColor; }
            set { emissiveColor = (value == Constants.EmissiveColor ? (Vector3?)null : value); }
        }
        private Vector3? emissiveColor;

        public float EnvironmentMapAmount
        {
            get { return environmentMapAmount.HasValue ? environmentMapAmount.Value : Constants.EnvironmentMapAmount; }
            set { environmentMapAmount = (value == Constants.EnvironmentMapAmount ? (float?)null : value); }
        }
        private float? environmentMapAmount;

        public Vector3 EnvironmentMapSpecular
        {
            get { return environmentMapSpecular.HasValue ? environmentMapSpecular.Value : Constants.EnvironmentMapSpecular; }
            set { environmentMapSpecular = (value == Constants.EnvironmentMapSpecular ? (Vector3?)null : value); }
        }
        private Vector3? environmentMapSpecular;

        public TextureCube EnvironmentMap { get; set; }

#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public SamplerState SamplerState { get; set; }
        #endregion

        #region Fields
        private EnvironmentMapEffect effect;

        private static Texture2D previousTexture;
        private static TextureCube previousEnvironmentMap;
        #endregion

        #region Methods
        public EnvironmentMapMaterial(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            effect = GraphicsResources<EnvironmentMapEffect>.GetInstance(graphics);
        }

        public override T Find<T>()
        {
            if (typeof(T) == typeof(IEffectMatrices) || typeof(T) == typeof(IEffectFog))
            {
                return effect as T;
            }
            return base.Find<T>();
        }

        protected override void OnBeginApply(DrawingContext context, Material previousMaterial)
        {
            var previousEnvironmentMapMaterial = previousMaterial as EnvironmentMapMaterial;
            if (previousEnvironmentMapMaterial == null)
            {
                effect.View = context.View;
                effect.Projection = context.Projection;

                ApplyLights(context, effect);
                ApplyFog(context, effect);
            }

            if (alpha != Constants.Alpha)
                effect.Alpha = alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = diffuseColor.Value;
            if (emissiveColor.HasValue)
                effect.EmissiveColor = emissiveColor.Value;
            if (environmentMapAmount.HasValue)
                effect.EnvironmentMapAmount = environmentMapAmount.Value;
            if (environmentMapSpecular.HasValue)
                effect.EnvironmentMapSpecular = environmentMapSpecular.Value;
            if (fresnelFactor.HasValue)
                effect.FresnelFactor = fresnelFactor.Value;

            if (previousEnvironmentMapMaterial == null || previousTexture != texture)
                previousTexture = effect.Texture = texture;

            if (previousEnvironmentMapMaterial == null || previousEnvironmentMap != EnvironmentMap)
                previousEnvironmentMap = effect.EnvironmentMap = EnvironmentMap;

            effect.World = World;

            if (SamplerState != null)
                GraphicsDevice.SamplerStates[0] = SamplerState;

            effect.CurrentTechnique.Passes[0].Apply();
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (alpha != Constants.Alpha)
                effect.Alpha = Constants.Alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = Constants.DiffuseColor;
            if (emissiveColor.HasValue)
                effect.EmissiveColor = Constants.EmissiveColor;
            if (environmentMapAmount.HasValue)
                effect.EnvironmentMapAmount = Constants.EnvironmentMapAmount;
            if (environmentMapSpecular.HasValue)
                effect.EnvironmentMapSpecular = Constants.EnvironmentMapSpecular;
            if (environmentMapSpecular.HasValue)
                effect.FresnelFactor = Constants.FresnelFactor;

            if (SamplerState != null)
                GraphicsDevice.SamplerStates[0] = context.settings.SamplerState;
        }

        protected override Material OnResolveMaterial(MaterialUsage usage, Material existingInstance)
        {
#if !WINDOWS_PHONE
            if (usage == MaterialUsage.Depth)
            {
                var result = (existingInstance as DepthMaterial) ?? new DepthMaterial(GraphicsDevice);
                result.TextureEnabled = (texture != null && IsTransparent);
                return result;
            }

            if (usage == MaterialUsage.DepthAndNormal)
            {
                return (existingInstance as DepthAndNormalMaterial) ?? new DepthAndNormalMaterial(GraphicsDevice);
            }
#endif
            return null;
        }
        #endregion
    }
}