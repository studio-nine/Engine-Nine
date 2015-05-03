namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    public class DetailMaterialPart : MaterialPart
    {
        private EffectParameter textureParameter;
        private EffectParameter detailTextureSettingsParameter;

        public Texture2D DetailTexture { get; set; }

        public float Attenuation { get; set; }

        public float Distance { get; set; }

        public Vector2 DetailTextureScale { get; set; }

        public DetailMaterialPart()
        {
            DetailTextureScale = Vector2.One;
            Attenuation = MathHelper.E;
            Distance = 10;
        }

        protected internal override void OnBind()
        {
            textureParameter = GetParameter("Texture");
            detailTextureSettingsParameter = GetParameter("DetailTextureSettings");
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext3D context, MaterialGroup material)
        {
            if (textureParameter != null)
            {
                Vector4 detailTextureSettings = new Vector4();
                detailTextureSettings.X = DetailTextureScale.X;
                detailTextureSettings.Y = DetailTextureScale.Y;
                detailTextureSettings.Z = Attenuation;
                detailTextureSettings.W = Distance;

                detailTextureSettingsParameter.SetValue(detailTextureSettings);
                textureParameter.SetValue(DetailTexture);
            }
        }

        protected internal override MaterialPart Clone()
        {
            return new DetailMaterialPart()
            {
                DetailTexture = this.DetailTexture,
                DetailTextureScale = this.DetailTextureScale,
                Attenuation = this.Attenuation,
                Distance = this.Distance,
            };
        }

        public override void SetTexture(Nine.Graphics.TextureUsage usage, Texture texture)
        {
            if (usage == Nine.Graphics.TextureUsage.Detail)
                DetailTexture = texture as Texture2D;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("DetailTexture") : null;
        }
    }
}
