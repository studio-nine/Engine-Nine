namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Design;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class DualTextureMaterial : Material
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : Constants.DiffuseColor; }
            set { diffuseColor = (value == Constants.DiffuseColor ? (Vector3?)null : value); }
        }
        private Vector3? diffuseColor;

        public Texture2D Texture2 { get; set; }
        public bool VertexColorEnabled { get; set; }
        public bool PreferPerPixelLighting { get; set; }

        [TypeConverter(typeof(SamplerStateConverter))]
        public SamplerState SamplerState { get; set; }
        #endregion

        #region Fields
        private DualTextureEffect effect;
        private MaterialFogHelper fogHelper;

        private static Texture2D previousTexture;
        private static Texture2D previousTexture2;
        #endregion

        #region Methods
        public DualTextureMaterial(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            effect = GraphicsResources<DualTextureEffect>.GetInstance(graphics);
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Dual)
                Texture2 = texture as Texture2D;
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
            var previousDualTextureMaterial = previousMaterial as DualTextureMaterial;
            if (previousDualTextureMaterial == null)
            {
                effect.View = context.View;
                effect.Projection = context.Projection;

                fogHelper.Apply(context, effect);
            }
            
            if (alpha != Constants.Alpha)
                effect.Alpha = alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = diffuseColor.Value;

            if (previousDualTextureMaterial == null || previousTexture != texture)
                previousTexture = effect.Texture = texture;
            if (previousDualTextureMaterial == null || previousTexture2 != Texture2)
                previousTexture2 = effect.Texture2 = Texture2;

            effect.World = World;
            effect.VertexColorEnabled = VertexColorEnabled;

            if (SamplerState != null)
            {
                GraphicsDevice.SamplerStates[0] = SamplerState;
                GraphicsDevice.SamplerStates[1] = SamplerState;
            }

            effect.CurrentTechnique.Passes[0].Apply();
        }

        protected override void OnEndApply(DrawingContext context)
        {
            if (alpha != Constants.Alpha)
                effect.Alpha = Constants.Alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = Constants.DiffuseColor;

            if (SamplerState != null)
            {
                GraphicsDevice.SamplerStates[0] =
                GraphicsDevice.SamplerStates[1] = context.settings.SamplerState;
            }
        }

        protected override Material OnResolveMaterial(MaterialUsage usage, Material existingInstance)
        {
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
            return null;
        }
        #endregion
    }
}