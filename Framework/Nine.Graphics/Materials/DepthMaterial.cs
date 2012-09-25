namespace Nine.Graphics.Materials
{
    using System.ComponentModel;
    using Microsoft.Xna.Framework;
    using Nine.Graphics.Drawing;

    /// <summary>
    /// Defines a material to show object depth.
    /// </summary>
    [NotContentSerializable]
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

        public bool AlphaTestEnabled
        {
            get { return alphaTestEnabled; }
            set { alphaTestEnabled = value; UpdateShaderIndex(); }
        }
        bool alphaTestEnabled;

        public int ReferenceAlpha
        {
            get { return referenceAlpha; }
            set { referenceAlpha = value; }
        }
        internal int referenceAlpha = Constants.ReferenceAlpha;

        private int shaderIndex;
        #endregion

        #region Methods
        private void UpdateShaderIndex()
        {
            shaderIndex = skinningEnabled ? (alphaTestEnabled ? 3 : 1) : (alphaTestEnabled ? 2 : 0);
        }

        partial void BeginApplyLocalParameters(DrawingContext context, DepthMaterial previousMaterial)
        {
            effect.CurrentTechnique = effect.Techniques[shaderIndex];
            if (alphaTestEnabled)
            {
                effect.Texture.SetValue(texture);
                effect.referenceAlpha.SetValue(referenceAlpha / 255f);
            }
            effect.worldViewProjection.SetValue(world * context.matrices.ViewProjection);
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            effect.bones.SetValue(boneTransforms);
        }
        #endregion
    }
}