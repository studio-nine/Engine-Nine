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
    /// <summary>
    /// Defines a material part that is used for hardware instancing.
    /// </summary>
    [ContentSerializable]
    public class InstancedMaterialPart : MaterialPart
    {
        protected internal override void OnBind()
        {
            
        }

        protected internal override MaterialPart Clone()
        {
            return new InstancedMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return null;
        }
    }
}
