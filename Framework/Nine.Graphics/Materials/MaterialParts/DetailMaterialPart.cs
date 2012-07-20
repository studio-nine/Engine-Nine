#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Nine.Graphics.Drawing;
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Materials.MaterialParts
{
    [ContentSerializable]
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
            if ((textureParameter = GetParameter("Texture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((detailTextureSettingsParameter = GetParameter("DetailTextureSettings")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            Vector4 detailTextureSettings = new Vector4();
            detailTextureSettings.X = DetailTextureScale.X;
            detailTextureSettings.Y = DetailTextureScale.Y;
            detailTextureSettings.Z = Attenuation;
            detailTextureSettings.W = Distance;

            detailTextureSettingsParameter.SetValue(detailTextureSettings);
            textureParameter.SetValue(DetailTexture);
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

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Detail)
                DetailTexture = texture as Texture2D;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("DetailTexture") : null;
        }
    }
}
