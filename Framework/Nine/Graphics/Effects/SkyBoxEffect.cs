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
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Nine.Graphics.Effects
{
#if !WINDOWS_PHONE

    public partial class SkyBoxEffect : IEffectMatrices
    {
        private Matrix world;
        private Matrix view;
        private Matrix projection;

        [ContentSerializerIgnore]
        public Matrix World
        {
            get { return world; }
            set { world = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        [ContentSerializerIgnore]
        public Matrix View
        {
            get { return view; }
            set { view = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        [ContentSerializerIgnore]
        public Matrix Projection
        {
            get { return projection; }
            set { projection = value; dirtyFlag |= worldViewProjectionDirtyFlag; }
        }

        private void OnCreated() 
        {

        }

        private void OnClone(SkyBoxEffect cloneSource) 
        {

        }
        
		private void OnApplyChanges()
        {
            if ((this.dirtyFlag & worldViewProjectionDirtyFlag) != 0)
            {
                Matrix positionIndependentView = view;
                positionIndependentView.Translation = Vector3.Zero;

                Matrix wvp;
                Matrix.Multiply(ref world, ref positionIndependentView, out wvp);
                Matrix.Multiply(ref wvp, ref projection, out wvp);
                worldViewProjection = wvp;
            }
        }
    }

#endif
}