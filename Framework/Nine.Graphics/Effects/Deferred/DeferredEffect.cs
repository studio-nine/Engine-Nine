#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
#if !WINDOWS_PHONE

    public partial class DeferredEffect : IEffectMatrices, IEffectMaterial, IEffectTexture, IEffectSkinned
    {
        public bool SkinningEnabled
        {
            get { return shaderIndex == 0; }
            set { shaderIndex = value ? 1 : 0; }
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
            DiffuseColor = Vector3.One;
            EmissiveColor = Vector3.Zero;
            SpecularColor = Vector3.Zero;
        }

        private void OnClone(DeferredEffect cloneSource)
        {
            SkinningEnabled = cloneSource.SkinningEnabled;
        }

        private void OnApplyChanges()
        {
            halfPixel = new Vector2(0.5f / GraphicsDevice.Viewport.Width, 0.5f / GraphicsDevice.Viewport.Height);
        }

        float IEffectMaterial.SpecularPower
        {
            get { return 16; }
            set { }
        }

        float IEffectMaterial.Alpha
        {
            get { return 1; }
            set { }
        }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.LightBuffer)
                LightTexture = texture as Texture2D;
        }
    }

#endif
}
