#region Copyright 2009 - 2011 (c) Engine Nine
//=============================================================================
//
//  Copyright 2009 - 2011 (c) Engine Nine. All Rights Reserved.
//
//=============================================================================
#endregion

#region Using Directives
using System.Collections.Generic;

#if !WINDOWS_PHONE

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