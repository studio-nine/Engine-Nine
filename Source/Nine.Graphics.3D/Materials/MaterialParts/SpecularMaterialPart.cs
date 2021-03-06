﻿namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a material part that provides specular color and specular texture
    /// </summary>
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
            get { return specularColor.HasValue ? specularColor.Value : Vector3.One; }
            set { specularColor = (value == Vector3.One ? (Vector3?)null : value); }
        }
        private Vector3? specularColor;

        /// <summary>
        /// Gets or sets the specular power.
        /// </summary>
        public float SpecularPower
        {
            get { return specularPower.HasValue ? specularPower.Value : Constants.SpecularPower; }
            set { specularPower = (value == Constants.SpecularPower ? (float?)null : value); }
        }
        private float? specularPower;

        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal override void  OnBind()
        {
            textureParameter = GetParameter("Texture");
            specularColorParameter = GetParameter("SpecularColor");
            specularPowerParameter = GetParameter("SpecularPower");
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext3D context, MaterialGroup material)
        {
            if (textureParameter != null)
                textureParameter.SetValue(SpecularMap);
            if (specularColor.HasValue && specularColorParameter != null)
                specularColorParameter.SetValue(specularColor.Value);
            if (specularPower.HasValue && specularPowerParameter != null)
                specularPowerParameter.SetValue(specularPower.Value);
        }

        /// <summary>
        /// Restores any local shader parameters changes after drawing the promitive.
        /// </summary>
        protected internal override void EndApplyLocalParameters(DrawingContext3D context)
        {
            if (specularColor.HasValue && specularColorParameter != null)
                specularColorParameter.SetValue(Vector3.One);
            if (specularPower.HasValue && specularPowerParameter != null)
                specularPowerParameter.SetValue(Constants.SpecularPower);
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
            // Specular power will be output to the alpha channel of the normal texture.
            return (usage != MaterialUsage.Default || usage != MaterialUsage.DepthAndNormal) ?
                GetShaderCode("SpecularMap").Replace("{$S1}", specularColorEnabled ? "" : "//")
                                            .Replace("{$S2}", specularColorEnabled ? "//" : "")
                                            .Replace("{$ST}", specularMapEnabled ? "" : "//") : null;
        }
    }
}
