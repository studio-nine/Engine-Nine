#region Copyright 2009 - 2010 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2010 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    public partial class DepthEffect : IEffectMatrices, IEffectSkinned
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

        private void OnCreated() 
        {
        }

        private void OnClone(DepthEffect cloneSource) 
        {
            SkinningEnabled = cloneSource.SkinningEnabled;
        }

        private void OnApplyChanges()
        {
            farClip = Math.Abs(Projection.M43 / (Math.Abs(Projection.M33) - 1));
        }
    }

#endif
}