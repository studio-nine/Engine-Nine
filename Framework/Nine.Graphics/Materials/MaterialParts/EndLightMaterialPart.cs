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
#if SILVERLIGHT
using Effect = Microsoft.Xna.Framework.Graphics.SilverlightEffect;
using EffectParameter = Microsoft.Xna.Framework.Graphics.SilverlightEffectParameter;
using EffectParameterCollection = Microsoft.Xna.Framework.Graphics.SilverlightEffectParametersCollection;
#endif
#endregion

namespace Nine.Graphics.Materials.MaterialParts
{
    [ContentSerializable]
    class EndLightMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {
            MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override MaterialPart Clone()
        {
            return new EndLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("EndLight") : null;
        }
    }
}
