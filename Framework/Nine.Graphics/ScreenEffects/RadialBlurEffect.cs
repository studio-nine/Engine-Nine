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

namespace Nine.Graphics.ScreenEffects
{
#if !WINDOWS_PHONE

    /// <summary>
    /// A post processing screen effect that blurs the whole screen radially.
    /// </summary>
    [ContentSerializable]
    public partial class RadialBlurEffect
    {
        private void OnCreated() { }
        private void OnClone(RadialBlurEffect cloneSource) { }
        private void OnApplyChanges() { }
    }

#endif
}
