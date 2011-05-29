#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.ComponentModel;
using Nine.Graphics.ParticleEffects;
#endregion

namespace Nine.Graphics
{
    public class GraphicsPasses
    {
        public event EventHandler<DrawEventArgs> Draw;
        public event EventHandler<DrawOverrideEventArgs> DrawOverride;
        public event EventHandler<DrawEventArgs> DrawOverlay;
    }
}
