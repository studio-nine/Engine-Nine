#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
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
    /// Defines a material part that provides specular color and specular texture
    /// </summary>
    [ContentSerializable]
    public class SpecularMaterialPart : MaterialPart
    {
        private EffectParameter specularColorParameter;
        private EffectParameter specularPowerParameter;
        private EffectParameter textureParameter;

        /// <summary>
        /// Gets or sets a value indicating whether specular map is enabled.
        /// The default value is not enabled.
        /// </summary>
        public bool SpecularMapEnabled
        {
            get { return specularMapEnabled; }
            set { specularMapEnabled = value; NotifyShaderChanged(); }
        }
        private bool specularMapEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether specular color is enabled.
        /// The default value is enabled.
        /// </summary>
        public bool SpecularColorEnabled
        {
            get { return specularColorEnabled; }
            set { specularColorEnabled = value; NotifyShaderChanged(); }
        }
        private bool specularColorEnabled = true;

        /// <summary>
        /// Gets or sets the specular map.
        /// </summary>
        public Texture2D SpecularMap { get; set; }

        /// <summary>
        /// Gets or sets the specular color.
        /// </summary>
        public Vector3 SpecularColor
        {
            get { return specularColor.HasValue ? specularColor.Value : MaterialConstants.SpecularColor; }
            set { specularColor = (value == MaterialConstants.SpecularColor ? (Vector3?)null : value); }
        }
        private Vector3? specularColor;

        /// <summary>
        /// Gets or sets the specular power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPower.HasValue ? specularPower.Value : MaterialConstants.SpecularPower; }
            set { specularPower = (value == MaterialConstants.SpecularPower ? (float?)null : value); }
        }
        private float? specularPower;

        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal override void  OnBind()
        {
            if (specularMapEnabled && (textureParameter = GetParameter("Texture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if (specularColorEnabled && (specularColorParameter = GetParameter("SpecularColor")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if ((specularPowerParameter = GetParameter("SpecularPower")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (specularMapEnabled)
                textureParameter.SetValue(SpecularMap);
            if (specularColorEnabled)
            {
                if (specularColor.HasValue)
                    specularColorParameter.SetValue(specularColor);
                if (specularPower.HasValue)
                    specularPowerParameter.SetValue(specularPower);
            }
        }

        /// <summary>
        /// Restores any local shader parameters changes after drawing the promitive.
        /// </summary>
        protected internal override void EndApplyLocalParameters()
        {
            if (specularColorEnabled)
            {
                if (specularColor.HasValue)
                    specularColorParameter.SetValue(MaterialConstants.SpecularColor);
                if (specularPower.HasValue)
                    specularPowerParameter.SetValue(MaterialConstants.SpecularPower);
            }
        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// Returns null if this material part do not have any local parameters.
        /// </summary>
        protected internal override MaterialPart Clone()
        {
            var result = new SpecularMaterialPart();
            result.specularPower = this.specularPower;
            result.specularColorEnabled = this.specularColorEnabled;
            result.specularMapEnabled = this.specularMapEnabled;
            result.SpecularMap = this.SpecularMap;
            return result;
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Specular)
                SpecularMap = texture as Texture2D;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            if (usage != MaterialUsage.Default)
                return null;

            return GetShaderCode("SpecularMap").Replace("{$S1}", specularColorEnabled ? "" : "//")
                                               .Replace("{$S2}", specularColorEnabled ? "//" : "")
                                               .Replace("{$ST}", specularMapEnabled ? "" : "//");
        }
    }
}
