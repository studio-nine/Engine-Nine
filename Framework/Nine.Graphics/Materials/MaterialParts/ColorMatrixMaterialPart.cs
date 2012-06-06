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
    public class ColorMatrixMaterialPart : MaterialPart
    {   
        public Matrix ColorMatrix 
        {
            get { return colorMatrix; }
            set { colorMatrix = value; }
        }
        private Matrix colorMatrix = Matrix.Identity;

        private EffectParameter transformParameter;

        protected internal override void OnBind()
        {
            if ((transformParameter = GetParameter("Transform")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            transformParameter.SetValue(colorMatrix);
        }

        protected internal override MaterialPart Clone()
        {
            return new ColorMatrixMaterialPart() { colorMatrix = this.colorMatrix };
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("ColorMatrix") : null;
        }
    }
}
