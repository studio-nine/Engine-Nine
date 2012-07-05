#region Copyright 2009 - 2012 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2012 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Linq;
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
    /// Defines a material part that provides normals from normal maps
    /// </summary>
    [ContentSerializable]
    public class NormalMapMaterialPart : MaterialPart
    {      
        private EffectParameter textureParameter;

        /// <summary>
        /// Gets or sets the normal map.
        /// </summary>
        public Texture2D NormalMap { get; set; }

        protected internal override void OnBind()
        {
            if ((textureParameter = GetParameter("Texture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        protected internal override void GetDependentParts(IList<Type> result)
        {
            result.Add(typeof(TangentTransformMaterialPart));
        }

        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            textureParameter.SetValue(NormalMap);
        }

        protected internal override MaterialPart Clone()
        {
            return new NormalMapMaterialPart() { NormalMap = this.NormalMap };
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.NormalMap)
                NormalMap = texture as Texture2D;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("NormalMap") : null;
        }
    }
}
