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
        Matrix IEffectMatrices.World { get; set; }

        private void OnCreated() 
        {

        }

        private void OnClone(SkyBoxEffect cloneSource) 
        {

        }
        
		private void OnApplyChanges()
        {
            farClip = Math.Abs(Projection.M43 / (Math.Abs(Projection.M33) - 1));
        }
    }

#endif
}
