#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects.Deferred
{
#if !WINDOWS_PHONE

    /// <summary>
    /// An effect that generates depth and normal info of the scene for deferred lighting.
    /// </summary>
    public partial class GraphicsBufferEffect : IEffectMatrices, IEffectSkinned, IEffectTexture, IEffectMaterial
    {
        bool skinningEnabled;
        bool normalMappingEnabled;
        bool shaderIndexNeedsUpdate;

        public bool SkinningEnabled
        {
            get { return skinningEnabled; }
            set { skinningEnabled = value; shaderIndexNeedsUpdate = true; }
        }

        public bool NormalMappingEnabled
        {
            get { return normalMappingEnabled; }
            set { normalMappingEnabled = value; shaderIndexNeedsUpdate = true; }
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
            SpecularPower = 32;
        }

        private void OnClone(GraphicsBufferEffect cloneSource)
        {
            SkinningEnabled = cloneSource.SkinningEnabled;
        }

        private void OnApplyChanges()
        {
            if (shaderIndexNeedsUpdate)
            {
                int shaderIndex = SkinningEnabled ? 1 : 0;
                shaderIndex += NormalMappingEnabled ? 2 : 0;

                Parameters["ShaderIndex"].SetValue(shaderIndex);

                shaderIndexNeedsUpdate = false;
            }

            frustumLength = Projection.GetFrustumLength();
        }

        Texture2D IEffectTexture.Texture
        {
            get { return null; }
            set { }
        }

        void IEffectTexture.SetTexture(string name, Texture texture)
        {
            if (name == TextureNames.NormalMap)
                NormalMap = texture as Texture2D;
        }

        Vector3 IEffectMaterial.DiffuseColor
        {
            get { return Vector3.Zero; }
            set { }
        }

        Vector3 IEffectMaterial.EmissiveColor
        {
            get { return Vector3.Zero; }
            set { }
        }

        Vector3 IEffectMaterial.SpecularColor
        {
            get { return Vector3.Zero; }
            set { }
        }
    }

#endif
}
