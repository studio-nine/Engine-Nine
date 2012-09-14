namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;    
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class AlphaTestMaterial : Material
    {
        #region Properties
        public GraphicsDevice GraphicsDevice { get; private set; }

        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : Constants.DiffuseColor; }
            set { diffuseColor = (value == Constants.DiffuseColor ? (Vector3?)null : value); }
        }
        private Vector3? diffuseColor;

        public int ReferenceAlpha
        {
            get { return referenceAlpha.HasValue ? referenceAlpha.Value : Constants.ReferenceAlpha; }
            set { referenceAlpha = (value == Constants.ReferenceAlpha ? (int?)null : value); }
        }
        private int? referenceAlpha;

        public CompareFunction AlphaFunction
        {
            get { return alphaFunction.HasValue ? alphaFunction.Value : Constants.AlphaFunction; }
            set { alphaFunction = (value == Constants.AlphaFunction ? (CompareFunction?)null : value); }
        }
        private CompareFunction? alphaFunction;

        public bool VertexColorEnabled { get; set; }

#if WINDOWS
        [TypeConverter(typeof(Nine.Graphics.Design.SamplerStateConverter))]
#endif
        public SamplerState SamplerState { get; set; }
        #endregion

        #region Fields
        private AlphaTestEffect effect;

        private static Texture2D previousTexture;
        #endregion

        #region Methods
        public AlphaTestMaterial(GraphicsDevice graphics)
        {
            GraphicsDevice = graphics;
            effect = GraphicsResources<AlphaTestEffect>.GetInstance(graphics);
            effect.ReferenceAlpha = Constants.ReferenceAlpha;
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
            var previousAlphaTestMaterial = previousMaterial as AlphaTestMaterial;
            if (previousAlphaTestMaterial == null)
            {
                effect.View = context.View;
                effect.Projection = context.Projection;

                ApplyFog(context, effect);
            }

            if (alpha != Constants.Alpha)
                effect.Alpha = alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = diffuseColor.Value;
            if (referenceAlpha.HasValue)
                effect.ReferenceAlpha = referenceAlpha.Value;
            if (alphaFunction.HasValue)
                effect.AlphaFunction = alphaFunction.Value;

            if (previousAlphaTestMaterial == null || previousTexture != texture)
                previousTexture = effect.Texture = texture;
            
            effect.World = World;
            effect.VertexColorEnabled = VertexColorEnabled;

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
            if (referenceAlpha.HasValue)
                effect.ReferenceAlpha = Constants.ReferenceAlpha;
            if (alphaFunction.HasValue)
                effect.AlphaFunction = Constants.AlphaFunction;

            if (SamplerState != null)
                GraphicsDevice.SamplerStates[0] = context.settings.SamplerState;
        }

        protected override Material OnResolveMaterial(MaterialUsage usage, Material existingInstance)
        {
#if !WINDOWS_PHONE
            if (usage == MaterialUsage.Depth)
            {
                var result = (existingInstance as DepthMaterial) ?? new DepthMaterial(GraphicsDevice) { TextureEnabled = true };
                result.referenceAlpha = referenceAlpha;
                return result;
            }
#endif
            return null;
        }
        #endregion
    }
}