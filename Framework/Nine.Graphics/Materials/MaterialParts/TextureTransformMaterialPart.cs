#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
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
    public class TextureTransformMaterialPart : MaterialPart
    {
        public Matrix TextureTransform 
        {
            get { return transform; }
            set { transform = value; }
        }
        private Matrix transform = Matrix.Identity;

        private EffectParameter textureTransformParameter;

        protected internal override void OnBind()
        {
            if ((textureTransformParameter = GetParameter("TextureTransform")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            textureTransformParameter.SetValue(Nine.Graphics.TextureTransform.ToArray(transform));
        }

        protected internal override MaterialPart Clone()
        {
            return new TextureTransformMaterialPart() { TextureTransform = TextureTransform };
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("TextureTransform") : null;
        }
    }
}
