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

    /// <summary>
    /// Represents an effect for drawing geometry depth.
    /// This effect is usually used to generate shadow map depth values.
    /// </summary>
    partial class DepthEffect : IEffectMatrices, IEffectSkinned
    {
        private Matrix world;
        private Matrix view;
        private Matrix projection;

        public bool SkinningEnabled
        {
            get { return shaderIndex == 0; }
            set { shaderIndex = value ? 1 : 0; }
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
            if ((dirtyFlag & worldViewProjectionDirtyFlag) != 0)
            {
                Matrix wvp;
                Matrix.Multiply(ref view, ref projection, out wvp);
                Matrix.Multiply(ref world, ref wvp, out wvp);
                worldViewProjection = wvp;
            }
        }
    }

#endif
}
