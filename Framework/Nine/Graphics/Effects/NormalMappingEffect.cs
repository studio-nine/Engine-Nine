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

    public partial class NormalMappingEffect : IEffectMatrices
    {
        private void OnCreated() { }
        private void OnClone(NormalMappingEffect cloneSource) { }
        private void OnApplyChanges() { }
    }

#endif
}
