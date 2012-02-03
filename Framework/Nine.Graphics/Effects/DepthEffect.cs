#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    /// <summary>
    /// Represents an effect for drawing geometry depth.
    /// This effect is usually used to generate shadow map depth values.
    /// </summary>
    partial class DepthEffect : IEffectMatrices, IEffectSkinned, IEffectTexture
    {
        private Matrix world;
        private Matrix view;
        private Matrix projection;

        public bool SkinningEnabled
        {
            get { return skinningEnabled; }
            set { skinningEnabled = value; UpdateShaderIndex(); }
        }
        bool skinningEnabled;

        public bool TextureEnabled
        {
            get { return textureEnabled; }
            set { textureEnabled = value; UpdateShaderIndex(); }
        }
        bool textureEnabled;

        public float ReferenceAlpha
        {
            get { return referenceAlpha * 255; }
            set { referenceAlpha = value / 255; }
        }

        public Matrix World
        {
            get { return world; }
            set { world = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
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

        private void UpdateShaderIndex()
        {
            shaderIndex = skinningEnabled ? (textureEnabled ? 3 : 1) :
                                            (textureEnabled ? 2 : 0);
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
            referenceAlpha = 0.5f;
        }

        private void OnClone(DepthEffect cloneSource) 
        {
            SkinningEnabled = cloneSource.SkinningEnabled;
        }

        private void OnApplyChanges()
        {
            if ((dirtyFlag & worldViewProjectionDirtyFlag) != 0)
            {
                Matrix wvp;
                Matrix.Multiply(ref view, ref projection, out wvp);
                Matrix.Multiply(ref world, ref wvp, out wvp);
                worldViewProjection = wvp;
            }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture) { }
    }

#endif
}
