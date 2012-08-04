namespace Nine.Graphics.Materials.MaterialParts
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Determines how the alpha channel of the diffuse texture will be used.
    /// </summary>
    public enum TextureAlphaUsage
    {
        /// <summary>
        /// The alpha channel will determine the opacity of the object.
        /// </summary>
        Opacity,

        /// <summary>
        /// The alpha channel will be replaced with the specified overlay color.
        /// Don't forget to turn off premultiplied alpha when using this flag.

        /// </summary>
        Overlay,

        /// <summary>
        /// The alpha channle will be used as the specular map.
        /// Don't forget to turn off premultiplied alpha when using this flag.

        /// </summary>
        Specular,

        /// <summary>
        /// The alpha channel of the texture is ignored.
        /// </summary>
        None,
    }

    /// <summary>
    /// Defines a material part that provides diffuse color and diffuse texture
    /// </summary>
    public class DiffuseMaterialPart : MaterialPart
    {
        private EffectParameter textureParameter;
        private EffectParameter diffuseColorParameter;
        private EffectParameter overlayColorParameter;

        /// <summary>
        /// Gets or sets a value indicating whether vertex color is used as diffuse color.
        /// The default value is not enabled.
        /// </summary>
        public bool VertexColorEnabled
        {
            get { return vertexColorEnabled; }
            set { vertexColorEnabled = value; NotifyShaderChanged(); }
        }
        private bool vertexColorEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether diffuse color is enabled.
        /// The default value is enabled.
        /// </summary>
        public bool DiffuseColorEnabled
        {
            get { return diffuseColorEnabled; }
            set { diffuseColorEnabled = value; NotifyShaderChanged(); }
        }
        private bool diffuseColorEnabled = true;

        /// <summary>
        /// Gets or sets a value indicating whether diffuse texture is enabled.
        /// The default value is enabled.
        /// </summary>
        public bool TextureEnabled
        {
            get { return textureEnabled; }
            set { textureEnabled = value; NotifyShaderChanged(); }
        }
        private bool textureEnabled = true;
        
        /// <summary>
        /// Gets or sets the diffuse texture.
        /// This value will override Material.Texture property when it's not null.
        /// </summary>
        public Texture2D Texture { get; set; }

        /// <summary>
        /// Gets or sets the diffuse color.
        /// </summary>
        public Vector3 DiffuseColor
        {
            get { return diffuseColor; }
            set { diffuseColor = value; }
        }
        private Vector3 diffuseColor = MaterialConstants.DiffuseColor;

        /// <summary>
        /// Gets or sets the overlay color when texture alpha is used as overlay.
        /// </summary>
        public Vector3 OverlayColor
        {
            get { return overlayColor.HasValue ? overlayColor.Value : MaterialConstants.DiffuseColor; }
            set { overlayColor = (value == MaterialConstants.DiffuseColor ? (Vector3?)null : value); }
        }
        private Vector3? overlayColor;

        /// <summary>
        /// Gets or sets the texture alpha usage.
        /// Make sure the texture is processed with no premultiplied alpha when usages
        /// are not set to TextureAlphaUsage.Opacity.
        /// </summary>        
        public TextureAlphaUsage TextureAlphaUsage
        {
            get { return textureAlphaUsage; }
            set
            {
                if (value != textureAlphaUsage)
                {
                    textureAlphaUsage = value;
                    NotifyShaderChanged();
                }
            }
        }
        private TextureAlphaUsage textureAlphaUsage;

        protected internal override void OnBind()
        {
            textureParameter = GetParameter("Texture");
            diffuseColorParameter = GetParameter("DiffuseColor");
            overlayColorParameter = GetParameter("OverlayColor");            
        }

        /// <summary>
        /// Applies all the local shader parameters before drawing any primitives.
        /// </summary>
        protected internal override void BeginApplyLocalParameters(DrawingContext context, MaterialGroup material)
        {
            if (textureParameter != null)
                textureParameter.SetValue(Texture ?? material.texture);

            if (overlayColorParameter != null && overlayColor.HasValue)
                overlayColorParameter.SetValue(overlayColor.Value);

            if (diffuseColorParameter != null)
            {
                var diffuseColorWithAlpha = new Vector4();
                diffuseColorWithAlpha.X = diffuseColor.X * material.alpha;
                diffuseColorWithAlpha.Y = diffuseColor.Y * material.alpha;
                diffuseColorWithAlpha.Z = diffuseColor.Z * material.alpha;
                diffuseColorWithAlpha.W = material.alpha;
                diffuseColorParameter.SetValue(diffuseColorWithAlpha);
            }
        }

        /// <summary>
        /// Restores any local shader parameters changes after drawing the promitive.
        /// </summary>
        protected internal override void EndApplyLocalParameters(DrawingContext context)
        {
            if (overlayColorParameter != null && overlayColor.HasValue)
                overlayColorParameter.SetValue(MaterialConstants.DiffuseColor);
        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// Returns null if this material part do not have any local parameters.
        /// </summary>
        protected internal override MaterialPart Clone()
        {
            var result = new DiffuseMaterialPart();
            result.Texture = this.Texture;
            result.overlayColor = this.overlayColor;
            result.textureEnabled = this.textureEnabled;
            result.textureAlphaUsage = this.textureAlphaUsage;
            result.DiffuseColor = this.DiffuseColor;
            result.diffuseColorEnabled = this.diffuseColorEnabled;
            return result;
        }

        protected internal override void OnResolveMaterialPart(MaterialUsage usage, MaterialPart existingInstance)
        {
            var part = ((DiffuseMaterialPart)existingInstance);
            part.Texture = this.Texture;
            part.diffuseColor = diffuseColor;
            part.diffuseColorEnabled = diffuseColorEnabled;
            part.overlayColor = overlayColor;
            part.textureAlphaUsage = textureAlphaUsage;
            part.textureEnabled = textureEnabled;
            part.vertexColorEnabled = vertexColorEnabled;
        }

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            if (usage != MaterialUsage.Default && usage != MaterialUsage.Depth && usage != MaterialUsage.DepthAndNormal)
                return null;

            return GetShaderCode("DiffuseTexture").Replace("{$V1}", vertexColorEnabled ? "" : "//")
                                                  .Replace("{$V2}", vertexColorEnabled ? "//" : "")
                                                  .Replace("{$AO}", textureAlphaUsage == TextureAlphaUsage.Overlay ? "" : "//")
                                                  .Replace("{$AA}", textureAlphaUsage == TextureAlphaUsage.Opacity ? "" : "//")
                                                  .Replace("{$AN}", textureAlphaUsage == TextureAlphaUsage.None ? "" : "//")
                                                  .Replace("{$AS}", textureAlphaUsage == TextureAlphaUsage.Specular ? "" : "//")
                                                  .Replace("{$D1}", diffuseColorEnabled ? "" : "//")
                                                  .Replace("{$D2}", diffuseColorEnabled ? "//" : "")
                                                  .Replace("{$TE}", textureEnabled ? "" : "//");
        }
    }
}
