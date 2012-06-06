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
    public class DirectionalLightMaterialPart : MaterialPart
    {
        private int lightIndex;
        private int lightVersion;
        private int lightCollectionVersion;

        private EffectParameter directionParameter;
        private EffectParameter diffuseColorParameter;
        private EffectParameter specularColorParameter;

        protected internal override void OnBind()
        {
            if ((directionParameter = GetParameter("DirLightDirection")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((diffuseColorParameter = GetParameter("DirLightDiffuseColor")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((specularColorParameter = GetParameter("DirLightSpecularColor")) == null)
                MaterialGroup.MaterialParts.Remove(this);

            foreach (var part in MaterialGroup.MaterialParts)
            {
                if (part is DirectionalLightMaterialPart)
                {
                    if (part == this)
                        break;
                    lightIndex++;
                }
            }
        }

        protected internal override void ApplyGlobalParameters(DrawingContext context)
        {
            var light = context.DirectionalLights[Index];
            if (light != null && light.version != lightVersion)
            {
                directionParameter.SetValue(light.Direction);
                diffuseColorParameter.SetValue(light.DiffuseColor);
                specularColorParameter.SetValue(light.SpecularColor);
            }

            if (lightCollectionVersion != lightVersion && light == null)
            {
                directionParameter.SetValue(Vector3.Zero);
                directionParameter.SetValue(Vector3.Zero);
            }
        }

        protected internal override MaterialPart Clone()
        {
            return new DirectionalLightMaterialPart();
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return usage == MaterialUsage.Default ? GetShaderCode("DirectionalLight") : null;
        }
    }
}
