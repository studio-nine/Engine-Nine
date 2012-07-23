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
    public class AmbientLightMaterialPart : MaterialPart
    {
        private int ambientLightVersion;
        private EffectParameter ambientLightColorParameter;

        protected internal override void OnBind()
        {
            if ((ambientLightColorParameter = GetParameter("AmbientLightColor")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            if (context.ambientLightColorVersion != ambientLightVersion)
            {
                ambientLightColorParameter.SetValue(context.ambientLightColor);
                ambientLightVersion = context.ambientLightColorVersion;
            }
        }

        protected internal override MaterialPart Clone()
        {
            return new AmbientLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("AmbientLight") : null;
        }
    }
}
