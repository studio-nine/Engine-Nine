namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class SkinnedMaterial : Material, IEffectSkinned
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

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

        public Vector3 SpecularColor
        {
            get { return specularColor.HasValue ? specularColor.Value : Vector3.One; }
            set { specularColor = (value == Vector3.One ? (Vector3?)null : value); }
        }
        private Vector3? specularColor;
        
        public float SpecularPower
        {
            get { return specularPower.HasValue ? specularPower.Value : Constants.SpecularPower; }
            set { specularPower = (value == Constants.SpecularPower ? (float?)null : value); }
        }
        private float? specularPower;

        public int WeightsPerVertex
        {
            get { return weightsPerVertex.HasValue ? weightsPerVertex.Value : Constants.WeightsPerVertex; }
            set { weightsPerVertex = (value == Constants.WeightsPerVertex ? (int?)null : value); }
        }
        private int? weightsPerVertex;

        public bool PreferPerPixelLighting { get; set; }
        bool IEffectSkinned.SkinningEnabled { get { return true; } set { } }

#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public SamplerState SamplerState { get; set; }
        #endregion

        #region Fields
        private SkinnedEffect effect;

        private static Texture2D previousTexture;
        #endregion

        #region Methods
        public SkinnedMaterial(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            effect = GraphicsResources<SkinnedEffect>.GetInstance(graphics);
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            effect.SetBoneTransforms(boneTransforms);
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
            var previousSkinnedMaterial = previousMaterial as SkinnedMaterial;
            if (previousSkinnedMaterial == null)
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
            if (specularColor.HasValue)
                effect.SpecularColor = specularColor.Value;
            if (specularPower.HasValue)
                effect.SpecularPower = specularPower.Value;
            if (weightsPerVertex.HasValue)
                effect.WeightsPerVertex = weightsPerVertex.Value;

            if (previousSkinnedMaterial == null || previousTexture != texture)
                previousTexture = effect.Texture = texture;

            effect.World = World;
            effect.PreferPerPixelLighting = PreferPerPixelLighting;

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
            if (specularColor.HasValue)
                effect.SpecularColor = Vector3.One;
            if (specularPower.HasValue)
                effect.SpecularPower = Constants.SpecularPower;
            if (weightsPerVertex.HasValue)
                effect.WeightsPerVertex = Constants.WeightsPerVertex;

            if (SamplerState != null)
                GraphicsDevice.SamplerStates[0] = context.settings.SamplerState;
        }

        protected override Material OnResolveMaterial(MaterialUsage usage, Material existingInstance)
        {
            if (usage == MaterialUsage.Depth)
            {
                var result = (existingInstance as DepthMaterial) ?? new DepthMaterial(GraphicsDevice) { SkinningEnabled = true };
                result.TextureEnabled = (texture != null && IsTransparent);
                return result;
            }

            if (usage == MaterialUsage.DepthAndNormal)
            {
                var result = (existingInstance as DepthAndNormalMaterial) ?? new DepthAndNormalMaterial(GraphicsDevice) { SkinningEnabled = true };
                result.specularPower = specularPower;
                return result;
            }
            return null;
        }
        #endregion
    }
}