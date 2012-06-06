#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
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
    public class DualTextureMaterialPart : MaterialPart
    {
        private EffectParameter textureParameter;

        public Texture2D Texture2 { get; set; }

        protected internal override void OnBind()
        {
            if ((textureParameter = GetParameter("Texture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            textureParameter.SetValue(Texture2);
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Dual)
                Texture2 = texture as Texture2D;
        }

        protected internal override MaterialPart Clone()
        {
            return new DualTextureMaterialPart() { Texture2 = this.Texture2 };
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("DualTexture") : null;
        }
    }
}
