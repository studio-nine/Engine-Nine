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
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Nine.Graphics.ParticleEffects;
#if !WINDOWS_PHONE
using Nine.Graphics.Effects;
#endif
#endregion

namespace Nine.Graphics.ObjectModel
{
    class LightingData
    {
        public List<Light> AffectingLights;
        public List<Light> MultiPassLights;
        public List<Light> MultiPassShadows;
    }
}