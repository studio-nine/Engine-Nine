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
    /// Represents an effect for drawing decals.
    /// </summary>
    partial class DecalEffect : IEffectMatrices, IEffectTexture, IEffectTextureTransform
    {
        private Matrix view;
        private Matrix projection;

        public Matrix View
        {
            get { return view; }
            set { view = value; dirtyFlag |= viewProjectionDirtyFlag; }
        }

        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; dirtyFlag |= viewProjectionDirtyFlag; }
        }

        private void OnCreated() 
        {
            Alpha = 1;
        }
        
        private void OnClone(DecalEffect cloneSource)
        {

        }

        private void OnApplyChanges()
        {
            if ((dirtyFlag & viewProjectionDirtyFlag) != 0)
            {
                Matrix vp;
                Matrix.Multiply(ref view, ref projection, out vp);
                viewProjection = vp;
            }
        }

        Matrix IEffectMatrices.World
        {
            get { return Matrix.Identity; }
            set { }
        }

        Texture2D IEffectTexture.Texture { get { return null; } set { } }

        void IEffectTexture.SetTexture(TextureUsage usage, Texture texture)
        {
            if (usage == TextureUsage.Decal)
                Texture = texture as Texture2D;
        }
    }

#endif
}
