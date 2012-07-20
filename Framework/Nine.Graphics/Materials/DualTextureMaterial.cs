namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [ContentSerializable]
    public class DualTextureMaterial : Material
    {
        #region Properties
        public float Alpha
        {
            get { return alpha.HasValue ? alpha.Value : MaterialConstants.Alpha; }
            set
            {
                if (value >= MaterialConstants.Alpha)
                {
                    alpha = null;
                    IsTransparent = cachedIsTransparent;
                }
                else
                {
                    if (alpha == null)
                        cachedIsTransparent = IsTransparent;
                    alpha = System.Math.Max(value, 0);
                }
            }
        }
        private float? alpha;
        private bool cachedIsTransparent;

        public Vector3 DiffuseColor
        {
            get { return diffuseColor.HasValue ? diffuseColor.Value : MaterialConstants.DiffuseColor; }
            set { diffuseColor = (value == MaterialConstants.DiffuseColor ? (Vector3?)null : value); }
        }
        private Vector3? diffuseColor;

        public Texture2D Texture2 { get; set; }
        public bool VertexColorEnabled { get; set; }
        public bool PreferPerPixelLighting { get; set; }
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

        public override void BeginApply(DrawingContext context)
        {
            var previousDualTextureMaterial = context.PreviousMaterial as DualTextureMaterial;
            if (previousDualTextureMaterial == null)
            {
                effect.View = context.View;
                effect.Projection = context.Projection;

                fogHelper.Apply(context, effect);
            }
            
            if (alpha.HasValue)
                effect.Alpha = alpha.Value;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = diffuseColor.Value;

            if (previousDualTextureMaterial == null || previousTexture != Texture)
                previousTexture = effect.Texture = Texture;
            if (previousDualTextureMaterial == null || previousTexture2 != Texture2)
                previousTexture2 = effect.Texture2 = Texture2;

            effect.World = World;
            effect.VertexColorEnabled = VertexColorEnabled;

            effect.CurrentTechnique.Passes[0].Apply();
        }

        public override void EndApply(DrawingContext context)
        {
            if (alpha.HasValue)
                effect.Alpha = MaterialConstants.Alpha;
            if (diffuseColor.HasValue)
                effect.DiffuseColor = MaterialConstants.DiffuseColor;
        }
        #endregion
    }
}