namespace Nine.Graphics.Materials
{
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;
    using Nine.Graphics.Drawing;

    [NotContentSerializable]
    partial class GraphicsBufferMaterial : IEffectSkinned
    {
        public float SpecularPower
        {
            get { return specularPower.HasValue ? specularPower.Value : MaterialConstants.SpecularPower; }
            set { specularPower = (value == MaterialConstants.SpecularPower ? (float?)null : value); }
        }
        private float? specularPower;

        public bool SkinningEnabled
        {
            get { return skinningEnabled; }
            set { skinningEnabled = value; }
        }
        private bool skinningEnabled;

        partial void BeginApplyLocalParameters(DrawingContext context, GraphicsBufferMaterial previousMaterial)
        {
            if (previousMaterial == null || skinningEnabled != previousMaterial.skinningEnabled)
                effect.shaderIndex.SetValue(skinningEnabled ? 1 : 0);
            if (specularPower.HasValue)
                effect.SpecularPower.SetValue(specularPower.Value);
            effect.World.SetValue(world);
            effect.WorldViewProjection.SetValue(world * context.matrices.ViewProjection);
        }

        partial void EndApplyLocalParameters(DrawingContext context)
        {
            if (specularPower.HasValue)
                effect.SpecularPower.SetValue(MaterialConstants.SpecularPower);
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            effect.bones.SetValue(boneTransforms);
        }
    }
}