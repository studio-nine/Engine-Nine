namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a material part that provides emissive color and emissive texture
    /// </summary>
    [ContentSerializable]
    public class EmissiveMaterialPart : MaterialPart
    {
        private EffectParameter emissiveColorParameter;
        private EffectParameter textureParameter;

        /// <summary>
        /// Gets or sets a value indicating whether emissive map is enabled.
        /// The default value is not enabled.
        /// </summary>
        public bool EmissiveMapEnabled
        {
            get { return emissiveMapEnabled; }
            set { emissiveMapEnabled = value; NotifyShaderChanged(); }
        }
        private bool emissiveMapEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether emissive color is enabled.
        /// The default value is enabled.
        /// </summary>
        public bool EmissiveColorEnabled
        {
            get { return emissiveColorEnabled; }
            set { emissiveColorEnabled = value; NotifyShaderChanged(); }
        }
        private bool emissiveColorEnabled = true;

        /// <summary>
        /// Gets or sets the emissive map.
        /// </summary>
        public Texture2D EmissiveMap { get; set; }

        /// <summary>
        /// Gets or sets the emissive color.
        /// </summary>
        public Vector3 EmissiveColor
        {
            get { return emissiveColor.HasValue ? emissiveColor.Value : MaterialConstants.EmissiveColor; }
            set { emissiveColor = (value == MaterialConstants.EmissiveColor ? (Vector3?)null : value); }
        }
        private Vector3? emissiveColor;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmissiveMaterialPart"/> class.
        /// </summary>
        protected internal override void OnBind()
        {
            if (emissiveMapEnabled && (textureParameter = GetParameter("Texture")) == null)
                MaterialGroup.MaterialParts.Remove(this);
            if (emissiveColorEnabled && (emissiveColorParameter = GetParameter("EmissiveColor")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (emissiveMapEnabled)
                textureParameter.SetValue(EmissiveMap);
            if (emissiveColorEnabled && emissiveColor.HasValue)
                emissiveColorParameter.SetValue(emissiveColor.Value);
        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// Returns null if this material part do not have any local parameters.
        /// </summary>
        protected internal override MaterialPart Clone()
        {
            var result = new EmissiveMaterialPart();
            result.emissiveColor = this.emissiveColor;
            result.emissiveColorEnabled = this.emissiveColorEnabled;
            result.emissiveMapEnabled = this.emissiveMapEnabled;
            result.EmissiveMap = this.EmissiveMap;
            return result;
        }

        public override void SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Emissive)
                EmissiveMap = texture as Texture2D;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            if (usage != MaterialUsage.Default)
                return null;

            return GetShaderCode("EmissiveMap").Replace("{$E1}", emissiveColorEnabled ? "" : "//")
                                               .Replace("{$E2}", emissiveColorEnabled ? "//" : "")
                                               .Replace("{$ET}", emissiveMapEnabled ? "" : "//");
        }
    }
}
