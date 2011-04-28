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
    /// Represents an effect for drawing geometry normal.
    /// </summary>
    public partial class NormalEffect : IEffectMatrices, IEffectSkinned
    {
        public bool SkinningEnabled
        {
            get { return Parameters["ShaderIndex"].GetValueInt32() == 0; }
            set { Parameters["ShaderIndex"].SetValue(value ? 1 : 0); }
        }
        
        public Matrix[] GetBoneTransforms(int count)
        {
            return bones;
        }
        
        public void SetBoneTransforms(Matrix[] boneTransforms)
        {
            bones = boneTransforms;
        }

        private void OnClone(NormalEffect cloneSource) 
        {
            SkinningEnabled = cloneSource.SkinningEnabled;
        }

        private void OnCreated() { }
		private void OnApplyChanges() { }
    }	

#endif
}
