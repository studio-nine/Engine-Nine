namespace Nine.Graphics.Materials.MaterialParts
{
    using System;
    using Microsoft.Xna.Framework;
    using Microsoft.Xna.Framework.Graphics;

    /// <summary>
    /// Defines a material part that allows vertex skinning based on bone hierarchy.
    /// </summary>
    [ContentSerializable]
    public class SkinnedMaterialPart : MaterialPart, IEffectSkinned
    {
        EffectParameter bonesParameter;

        /// <summary>
        /// Gets or sets the max number of bones.
        /// </summary>
        public int MaxBones
        {
            get { return maxBones; }
            set { maxBones = value; NotifyShaderChanged(); }
        }
        private int maxBones = 72;

        /// <summary>
        /// Gets or sets the weights per vertex.
        /// </summary>
        public int WeightsPerVertex
        {
            get { return weightsPerVertex; }
            set { weightsPerVertex = value; NotifyShaderChanged(); }
        }
        private int weightsPerVertex = 4;

        /// <summary>
        /// Called when this material part is bound to a material group.
        /// </summary>
        protected internal override void OnBind()
        {
            if ((bonesParameter = GetParameter("Bones")) == null)
                MaterialGroup.MaterialParts.Remove(this);
        }

        /// <summary>
        /// Copies data from an existing object to this object.
        /// Returns null if this material part do not have any local parameters.
        /// </summary>
        protected internal override MaterialPart Clone()
        {
            return new SkinnedMaterialPart() { maxBones = maxBones, weightsPerVertex = weightsPerVertex };
        }

        /// <summary>
        /// Sets an array of skinning bone transform matrices.
        /// </summary>
        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            if ((boneTransforms == null) || (boneTransforms.Length == 0))
                throw new ArgumentNullException("boneTransforms");

            if (boneTransforms.Length > MaxBones)
                throw new ArgumentException();

            bonesParameter.SetValue(boneTransforms);
        }

#if !SILVERLIGHT
        /// <summary>
        /// Gets a copy of the current skinning bone transform matrices.
        /// </summary>
        public Matrix[] GetBoneTransforms(int count)
        {
            if (count <= 0 || count > MaxBones)
                throw new ArgumentOutOfRangeException("count");

            Matrix[] bones = bonesParameter.GetValueMatrixArray(count);

            // Convert matrices from 43 to 44 format.
            for (int i = 0; i < bones.Length; i++)
            {
                bones[i].M44 = 1;
            }

            return bones;
        }
#endif

        protected internal override string GetShaderCode(MaterialUsage usage)
        {
            return GetShaderCode("SkinTransform")
                  .Replace("{$MAXBONES}", MaxBones.ToString())
                  .Replace("{$BONECOUNT}", WeightsPerVertex.ToString());
        }

        bool IEffectSkinned.SkinningEnabled
        {
            get { return true; }
            set { }
        }
    }
}
