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

    public partial class ShadowEffect : IEffectMatrices
    {
        private void OnCreated() { }
        
        private void OnApplyChanges()
        {
            farClip = Math.Abs(LightProjection.M43 / (Math.Abs(LightProjection.M33) - 1));
        }

        private void OnClone(ShadowEffect cloneSource) { }
    }

#endif
}
