#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    /// <summary>
    /// Defines a shadow effect that can be used to draw multi-pass shadows.
    /// </summary>
    partial class ShadowEffect: IEffectMatrices, IEffectSkinned, IEffectShadowMap
    {
        private Matrix view;
        private Matrix projection;

        public bool SkinningEnabled
        {
            get { return shaderIndex == 1; }
            set { shaderIndex = value ? 1 : 0; }
        }

        public Matrix View
        {
            get { return view; }
            set { view = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        public Matrix[] GetBoneTransforms(int count)
        {
            return bones;
        }

        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            bones = boneTransforms;
        }

        private void OnCreated()
        {

        }

        private void OnClone(ShadowEffect cloneSource) 
        {
            SkinningEnabled = cloneSource.SkinningEnabled;
        }

        private void OnApplyChanges() 
        {
            if ((dirtyFlag & worldViewProjectionDirtyFlag) != 0 || (dirtyFlag & WorldDirtyFlag) != 0)
            {
                Matrix wvp;
                Matrix.Multiply(ref view, ref projection, out wvp);
                Matrix.Multiply(ref _World, ref wvp, out wvp);
                worldViewProjection = wvp;
            }

            if ((dirtyFlag & ShadowMapDirtyFlag) != 0)
            {
                shadowMapTexelSize = new Vector2(1.0f / ShadowMap.Width, 1.0f / ShadowMap.Height);
            }
        }
    }

#endif
}
