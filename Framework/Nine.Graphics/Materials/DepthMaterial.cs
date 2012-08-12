namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a material to show object depth.
    /// </summary>
    [ContentSerializable]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public partial class DepthMaterial : IEffectSkinned
    {
        #region Properties
        /// <summary>
        /// Gets or sets if vertex skinning is enabled by this effect.
        /// </summary>
        public bool SkinningEnabled
        {
            get { return skinningEnabled; }
            set { skinningEnabled = value; UpdateShaderIndex(); }
        }
        bool skinningEnabled;

        /// <summary>
        /// Gets or sets a value indicating whether texture is enabled.
        /// </summary>
        public bool TextureEnabled
        {
            get { return textureEnabled; }
            set { textureEnabled = value; UpdateShaderIndex(); }
        }
        bool textureEnabled;

        public int ReferenceAlpha
        {
            get { return referenceAlpha.HasValue ? referenceAlpha.Value : Constants.ReferenceAlpha; }
            set { referenceAlpha = (value == Constants.ReferenceAlpha ? (int?)null : value); }
        }
        internal int? referenceAlpha;

        private int shaderIndex;
        #endregion

        #region Methods
        private void UpdateShaderIndex()
        {
            shaderIndex = skinningEnabled ? (textureEnabled ? 3 : 1) :
                                            (textureEnabled ? 2 : 0);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, DepthMaterial previousMaterial)
        {
            if (previousMaterial != null && shaderIndex != previousMaterial.shaderIndex)
                effect.shaderIndex.SetValue(shaderIndex);
            if (referenceAlpha.HasValue)
                effect.referenceAlpha.SetValue(referenceAlpha.Value);
            effect.worldViewProjection.SetValue(world * context.matrices.ViewProjection);
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            if (referenceAlpha.HasValue)
                effect.referenceAlpha.SetValue(Constants.ReferenceAlpha);
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            effect.bones.SetValue(boneTransforms);
        }
        #endregion
    }
}