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
    class BeginLightMaterialPart : MaterialPart
    {
        private EffectParameter eyePositionParameter;

        protected internal override void OnBind()
        {
            if ((eyePositionParameter = GetParameter("EyePosition")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            eyePositionParameter.SetValue(context.EyePosition);
        }

        protected internal override MaterialPart Clone()
        {
            return new BeginLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("BeginLight") : null;
        }
    }
}
